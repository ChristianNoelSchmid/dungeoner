using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server.Networking.Datagrams;
using Server.Networking.Resolvers;

namespace Server.Networking {
    /// 
    /// <summary>
    /// Handles the close-to-metal transmission of UDP
    /// datagram packets. Handles relevant flags (such as REL, ACK
    /// and RES), and automatically handles retransmission of reliable
    /// packets, and requesting retransmissions from other UDP clients.
    /// </summary>
    /// 
    /// The handler can send messages to any arbitrary server, but it can
    /// also react to messages received by connections. This is done via
    /// an event being triggered with two callbacks - one to send a new
    /// datagram to the caller, and another to send information to all
    /// other connected clients.
    /// 
    public class NetworkDatagramHandler {
        private AckResolver _ackResolver;

        /// <summary>
        /// The client for the UDP network. Handles sending and receiving
        /// of datagrams.
        /// </summary>
        private UdpClient _client;

        /// <summary>
        /// Represents the indices of the local client's current index count
        /// of ack datagrams, per connection. A dictionary is needed as the local
        /// client may send different amounts of ensured datagrams to each client.
        /// </summary>
        private ConcurrentDictionary<IPEndPoint, ulong> _ackCurrentIndices;

        /// <summary>
        /// A dictionary which holds a collection of client IPEndPoints,
        /// and an index representing the total number of ensured datagrams 
        /// sent. This is updated on both the client side and distant client side,
        /// To ensure that datagrams which need acknowledgements are not lost 
        /// in the network.
        /// </summary>
        private ConcurrentDictionary<IPEndPoint, ulong> _ackExpectedIndices;

        private ConcurrentDictionary<IPEndPoint, DateTime> _lastMessages;

        public EventHandler<DatagramIncomingMessage> MessageRecieved;
        public EventHandler<DatagramIncomingMessage> LostConnection;
        public bool IsListening { get; private set; } = true;
        private CancellationTokenSource? _cancellationTokenSource = new();

        /// <summary>
        /// Generically sends a datagram to the specified end points, either reliable or unreliable.
        /// </summary>
        /// <param name="data">The message to send</param>
        /// <param name="isReliable">Whether the handler should ensure 
        /// the message reaches its intended recipient</param>
        /// <param name="endPoints">The client(s) to send the datagram to</param>
        public async Task SendAsync(DatagramOutgoingMessage msg) {
            byte[] buffer;
            ulong ackIndex;

            IEnumerable<IPEndPoint> endPoints = (msg.EndPoints ?? Array.Empty<IPEndPoint>());
            if(msg.SendType == DatagramOutgoingSendType.SendToAll) {
                endPoints = _lastMessages.Keys;
            } else if(msg.SendType == DatagramOutgoingSendType.SendToOthers) {
                endPoints = _lastMessages.Keys.Where(ep => !endPoints.Contains(ep));
            } 

            foreach(var endPoint in endPoints) {
                if(msg.IsRel) {
                    // Since this code is read-only, it doesn't need to be locked
                    if(_ackResolver.IsClientBufferFull(endPoint)) continue;

                    // Update the _ackLocalToRemoteCounts to reflect the new
                    // # of reliable messages sent to specific client (+ 1)
                    if(!_ackCurrentIndices.TryGetValue(endPoint, out ackIndex))
                        ackIndex = 0;

                    // Append the ack index count to the message, to communicate to the
                    // client what reliable datagram count it represents
                    buffer = Reliable.Create(ackIndex, msg.Data);

                    // Add a new AckResolver to the resolver buffer, which will
                    // resend the datagram if the timeout is reached
                    // before receiving an ACK
                    _ackResolver.AddResolver(
                        new (
                            AckIndex: ackIndex,
                            IPEndPoint: endPoint,
                            TicksStart: DateTime.Now.Ticks,
                            Data: buffer 
                        )
                    );

                    _ackCurrentIndices.AddOrUpdate(endPoint, ackIndex + 1, (_, __) => ackIndex + 1);
                }
                else
                    buffer = Unreliable.Create(msg.Data);


                await _client.SendAsync(buffer, buffer.Length, endPoint);
            }
        }

        public void Open(int port) { 
            _cancellationTokenSource?.Cancel();

            _client = new UdpClient(port);
            _ackResolver = new AckResolver();
            _ackResolver.AckTimedOut += async (_, ackData) => await SendMessage(ackData);

            _ackCurrentIndices = new ConcurrentDictionary<IPEndPoint, ulong>();
            _ackExpectedIndices = new ConcurrentDictionary<IPEndPoint, ulong>();
            _lastMessages = new ConcurrentDictionary<IPEndPoint, DateTime>();

            _cancellationTokenSource = new CancellationTokenSource();

            // Start the AckResolver and Begin recieving
            _ = CheckClientDisconnectionsAsync(_cancellationTokenSource.Token);
            _ = StartReceivingAsync(_cancellationTokenSource.Token);

            IsListening = true;
        }

        public void Close() {
            _cancellationTokenSource?.Cancel();
            IsListening = false;
        }

        /// <summary>
        /// Sends a datagram with the specified AckResolver info
        /// The datagram must be sent reliably, but the AckResolver
        /// is already in the buffer, so there's no need to add it again.
        /// </summary>
        /// <param name="resolver">The AckResolver with the datagram info</param>
        private async Task SendMessage(AckResolverData resolver) {
            // Append the ack index count to the message, to communicate to the
            // client what reliable datagram count it represents
            var buffer = Reliable.Create(resolver.AckIndex, resolver.Data);
            await _client.SendAsync(buffer, buffer.Length, resolver.IPEndPoint);
        }

        /// <summary>
        /// Converts an incoming datagram into the correct
        /// datagram type, based on which tag is attached
        /// to the message.
        /// </summary>
        /// <param name="datagram">The sequence of bytes representing the datagram message</param>
        /// <returns></returns>
        private Datagram ParseDatagram(in byte[] datagram) {
            string prefix = Encoding.ASCII.GetString(datagram[..3]);

            try {
                switch(prefix) {
                    case string s when s.StartsWith("ACK"):
                        return new Ack(datagram[3..]);
                    case string s when s.StartsWith("REL"):
                        return new Reliable(datagram[3..]);
                    case string s when s.StartsWith("RES"):
                        return new Resend();
                    default:
                        return new Unreliable(datagram);
                };
            }
            catch(Exception) { return null; }
        }


        #region Background Async Methods

        /// <summary>
        /// The system which handles recieving datagrams, converting
        /// them to their appropriate type, and passing basic information
        /// back to the client and / or the local application.
        /// </summary>
        private async Task StartReceivingAsync(CancellationToken cancellationToken) {
            IPEndPoint endPoint = default!;
            Datagram datagram = default!;
            ulong ackExpected = 0;
            byte[] bytes;

            while(!cancellationToken.IsCancellationRequested)
            {
                if(!IsListening) continue;

                var result = await _client.ReceiveAsync(cancellationToken); 
                if(cancellationToken.IsCancellationRequested) break;

                bytes = result.Buffer;
                endPoint = result.RemoteEndPoint;

                _lastMessages.AddOrUpdate(endPoint, DateTime.UtcNow, (_, __) => DateTime.Now);
                
                if(!IsListening) continue;

                datagram = ParseDatagram(bytes);
                _ackExpectedIndices.TryGetValue(endPoint, out ackExpected);

                switch(datagram)
                {
                    // Datagram is reliable, but the ACK index is too high.
                    // Ask client to resend awaiting data.
                    case Reliable rel when rel.AckIndex > ackExpected:
                        await SendAsync(new(DatagramOutgoingSendType.SendTo, Resend.Create(), false, new [] { endPoint }));                       break;

                    // Message is reliable, but the ACK index is too low.
                    // Already recieved datagram: simply resend ACK and return
                    case Reliable rel when rel.AckIndex < ackExpected:
                        await SendAsync(new(DatagramOutgoingSendType.SendTo, Ack.Create(rel.AckIndex), false, new [] { endPoint }));   
                                                                                                        break;

                    // Message is reliable, and was the expected index.
                    // Accept message, send ACK, and invoke MessageRecieved event
                    case Reliable rel:
                        _ackExpectedIndices.AddOrUpdate(endPoint, ackExpected + 1, (_, __) => ackExpected + 1);
                        await SendAsync(new(DatagramOutgoingSendType.SendTo, Ack.Create(rel.AckIndex), false, new [] { endPoint }));
                        MessageRecieved.Invoke(null, new DatagramIncomingMessage(rel.Data, endPoint));
                                                                                                        break;

                    // Message contains request to resend packages
                    // Resend earliest package
                    case Resend: _ackResolver.ResendRel(endPoint);                                      break;

                    // Message is an acknowledgement datagram
                    // Accept and remove awaiting AckResolver
                    case Ack ack: _ackResolver.AcceptAck(endPoint, ack.AckIndex);                       break;

                    // Message is unreliable - invoke MessageRecieved event
                    case Unreliable unrel: 
                        MessageRecieved.Invoke(null, new DatagramIncomingMessage(unrel.Data, endPoint));        break; 
                }
            }
        }

        // Checks to see if any Client has not sent a datagram for longer than 7.5 seconds.
        // If this is the case, the Client is removed from the Server's memory, as well
        // as from the AckResolver's buffer
        private async Task CheckClientDisconnectionsAsync(CancellationToken cancellationToken) {
            while(!cancellationToken.IsCancellationRequested) {
                foreach(var pair in _lastMessages) {
                    if(DateTime.UtcNow - pair.Value > TimeSpan.FromSeconds(7000.5f)) {
                        // Invoke the LostConnection method, to be used by the 
                        // NetworkEventHandler to dictate PlayerLeft messages
                        LostConnection.Invoke(null, new DatagramIncomingMessage(Array.Empty<byte>(), pair.Key));

                        // Remove the Client's data from the Server                                
                        _ackExpectedIndices.Remove(pair.Key, out _); 
                        _ackResolver.RemoveClientEndPoint(pair.Key); 
                        _lastMessages.Remove(pair.Key, out _);
                    }
                }

                await Task.Delay(100, cancellationToken);
            }
        }
        #endregion
    }
}