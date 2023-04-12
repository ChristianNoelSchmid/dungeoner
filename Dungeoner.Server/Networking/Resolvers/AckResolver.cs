using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Server.Networking.Resolvers
{
    /// <summary>
    /// Resolves acknowledgements when the Server sends a reliable
    /// message to the Client.
    /// </summary>
    public class AckResolver
    {
        /// <summary>
        /// Dictionary which contains all information about needed acknoledgements.
        /// Ensures specific messages are sent. When an ensured message is sent,
        /// it's IPEndPoint is stored in this Dictionary, as well as all information
        /// pertaining to the datagram. Periodically, this Dictionary is checked and,
        /// upon a time which a values been in it longer than a specified timeout,
        /// it resends the datagram. Acknowledgements that do come remove the
        /// value from the Dictionary.
        /// </summary>
        private Dictionary<IPEndPoint, List<AckResolverData>> _resolverBuffer;

        // Lock for the buffer. There are multiple threads which can 
        // retrieve information from it.
        private object _resolverLock = new object();

        /// <summary>
        /// Length of ticks for a packet timeout
        /// </summary>
        /// <returns>Length of a network timeout - 1 second</returns>
        private readonly double timeout = Math.Pow(10.0, 7.0);

        /// <summary>
        /// The EventHandler which invokes upon a udp packets timing
        /// out, requiring the connected NetworkDatagramHandler to resent
        /// the message.
        /// </summary>
        public EventHandler<AckResolverData> AckTimedOut;

        public AckResolver()
        {
            _resolverBuffer = new();
            new Thread(StartAckResolver){ IsBackground = true }.Start();
        }

        /// <summary>
        /// Adds a new AckResolver to the resolver buffer
        /// </summary>
        /// <param name="resolver">The new AckResolver to add</param>
        public void AddResolver(in AckResolverData resolver)
        {
            lock(_resolverLock)
            {
                // Retrieve the AckResolver List (if it exists) for
                // the specified connection, and add the new AckResolver
                // to the List
                if(!_resolverBuffer.TryGetValue(resolver.IPEndPoint, out var ackList))
                    ackList = new();

                ackList.Add(resolver); 
                ackList.Sort((ack1, ack2) => ack1.AckIndex.CompareTo(ack2.AckIndex));
            }
        }

        /// <summary>
        /// Removes the specified Client's endpoint from the resolver buffer
        /// </summary>
        /// <param name="endPoint">The endpoint of the Client</param>
        public void RemoveClientEndPoint(in IPEndPoint endPoint)
        {
            lock(_resolverLock)
            {
                if(_resolverBuffer.ContainsKey(endPoint))
                    _resolverBuffer.Remove(endPoint);
            }
        }

        /// <summary>
        /// Accepts a single acknowledgement, matching it with the
        /// specific Client's resolver list.
        /// </summary>
        /// <param name="endPoint">The Client's endpoint</param>
        /// <param name="ack">The acknowledgement number</param>
        public void AcceptAck(in IPEndPoint endPoint, ulong ack)
        {
            int index;
            lock(_resolverLock)
            {
                if(_resolverBuffer.TryGetValue(endPoint, out var resolverList))
                {
                    index = resolverList.FindIndex(res => res.AckIndex == ack);
                    if(index != -1) resolverList.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Detects if a Client's resolver buffer is full.
        /// </summary>
        /// <param name="endPoint">The Client's endpoint</param>
        /// <returns>True if the client's buffer is full, false otherwise.</returns>
        public bool IsClientBufferFull(in IPEndPoint endPoint)
        {
            lock(_resolverLock)
            {
                if(_resolverBuffer.TryGetValue(endPoint, out var resolverList))
                    return resolverList.Count > 100;
            }

            return false;
        }

        /// <summary>
        /// Invokes the timeout EventHandler to send all reliable
        /// datagrams in a buffer to the specified Client.
        /// </summary>
        /// <param name="endPoint">The Client's endpoint</param>
        public void ResendRel(in IPEndPoint endPoint)
        {
            lock(_resolverLock)
            {
                foreach(var res in _resolverBuffer[endPoint])
                    AckTimedOut?.Invoke(null, res);
            }
        }

        /// <summary>
        /// Initiates the AckListener, which will routinely send
        /// reliable messages every time its AckResolver times out
        /// in the resolver buffer.
        /// </summary>
        private void StartAckResolver() {
            AckResolverData? resolver;

            while(true) {
                Thread.Sleep(100);

                lock(_resolverLock) {
                    // For each end point in the buffer, 
                    // check if the oldest one has reached the timeout length
                    // If so, resend the reliable datagram
                    foreach(var list in _resolverBuffer.Values) {
                        resolver = list.FirstOrDefault();
                        if(DateTime.Now.Ticks - resolver?.TicksStart > timeout) {
                            foreach(var res in list)
                                AckTimedOut?.Invoke(null, res);
                        }
                    }
                }
            }
        }
    }
}