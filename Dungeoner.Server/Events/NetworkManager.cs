using Dungeoner.Server.Events;
using Godot;
using ProtoBuf;
using Server.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public partial class NetworkManager : Node {
    [Export]
    private int _port = 8000;

    // The datagram handler
    private  NetworkDatagramHandler _datagramHandler;
    // Collection of network event invokers, and the type they represent
    private  IReadOnlyDictionary<string, NetworkEventHandler> _invokers;
    // Collection of network models, mapped to their event names
    private  IReadOnlyDictionary<Type, string> _models;
    // A ConcurrentQueue storing all outgoing messages
    private  ConcurrentQueue<DatagramOutgoingMessage> _sendQueue;
    // Buffer for all possible event names
    private  byte[] _buffer = Array.Empty<byte>();

    Random random = new();

    private bool _isHost = true;
    public bool IsHost { 
        get => _isHost;
        set {
            _isHost = value;
            _datagramHandler.Close();
            _port = _isHost ? 8000 : random.Next(8001, 8999);
        } 
    }

    public override void _Ready() {
        Dictionary<string, NetworkEventHandler> invokers = new();
        Dictionary<Type, string> models = new();

        foreach(var type in Assembly.GetExecutingAssembly().GetTypes()) {
            // Find all types with the NetworkEventAttribute assigned
            var networkEventAttribute = type.GetCustomAttribute(typeof(NetworkEventAttribute)) as NetworkEventAttribute;
            if(networkEventAttribute != null) {
                var invoker = (NetworkEventHandler)Activator.CreateInstance(type)!;
                invoker._parser = this;
                invokers[networkEventAttribute.Name] = invoker;
            }
            // Find all types with the NetworkEventModelAttribute assigned
            var networkEventModelAttribute = type.GetCustomAttribute(typeof(NetworkEventModelAttribute)) as NetworkEventModelAttribute;
            if(networkEventModelAttribute != null) {
                models[type] = networkEventModelAttribute.Name;
            }
        }
        foreach(var key in invokers.Keys) {
            // Ensure there are matching NetworkEventModels for every NetworkEvent
            if(!models.ContainsValue(key)) {
                throw new KeyNotFoundException($"Cannot find matching NetworkEventModel for NetworkEvent {key}");
            }
            // Ensure the generic for the INetworkEventHandler matches the NetworkEventModel type
            var networkEventInterface = invokers[key].GetType().GetInterface($"{nameof(NetworkEventHandler)}'1");
        }

        _invokers = invokers;
        _models = models;
        _sendQueue = new();

        _buffer = new byte[_invokers.Keys.Max(key => key.Length)];

        _datagramHandler = new();
        _datagramHandler.MessageRecieved += (_, callback) => OnMessageReceived(callback);

        _ = RunParser();
    }
 
    internal void QueueMessage(
        DatagramOutgoingSendType sendType, 
        NetworkEventModel model, 
        bool isRel, 
        params IPEndPoint[]? ipAddresses
    ) {
        var eventNameBytes = Encoding.UTF8.GetBytes(_models[model.GetType()]);

        var stream = new MemoryStream();
        Serializer.Serialize(stream, model);

        var data = eventNameBytes.Concat(stream.ToArray()).ToArray();
        _sendQueue.Enqueue(new(sendType, data, isRel, ipAddresses));
    }

    private async Task RunParser() {
        while(true) {
            while(_sendQueue.TryDequeue(out var msg)) {
                await _datagramHandler.SendAsync(msg);
            }
            await Task.Delay(100);           
        }
    }

    private void OnMessageReceived(DatagramIncomingMessage callback) {
        var eventName = Encoding.UTF8.GetString(callback.Data, 0, _buffer.Length);

        if(_invokers.TryGetValue(eventName, out var invoker)) {
            invoker.Invoke(callback.Data[_buffer.Length..], callback.EndPoint);
        } else {
            throw new KeyNotFoundException($"Could not find network event {eventName}.");
        }
    }
}