using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Godot;
using ProtoBuf;
using Server.Networking;

namespace Dungeoner.Server.Events;



[AttributeUsage(AttributeTargets.Class)]
public class NetworkEventAttribute : Attribute {
    public string Name { get; private set; }
    public NetworkEventAttribute(string name) {
        Name = name;
    }
}
[AttributeUsage(AttributeTargets.Class)]
public class NetworkEventModelAttribute : Attribute {
    public string Name { get; private set; }
    public NetworkEventModelAttribute(string name) {
        Name = name;
    }
}
public record NetworkEventModel;
public abstract partial class NetworkEventHandler : Node { 
    internal NetworkManager _parser;

    protected void SendTo(IPEndPoint endPoint, NetworkEventModel netEvent, bool isRel) {
        _parser.QueueMessage(DatagramOutgoingSendType.SendTo, netEvent, isRel, endPoint);
    }
    protected void SendToAll(NetworkEventModel netEvent, bool isRel) {
        _parser.QueueMessage(DatagramOutgoingSendType.SendToAll, netEvent, isRel);
    }
    protected void SendToOthers(IPEndPoint allButEndPoint, NetworkEventModel netEvent, bool isRel) {
        _parser.QueueMessage(DatagramOutgoingSendType.SendToOthers, netEvent, isRel, allButEndPoint);
    }

    internal abstract void Invoke(byte[] data, IPEndPoint senderEndPoint); 
}
public partial class NetworkEventHandler<T> : NetworkEventHandler { 
    private readonly ConcurrentQueue<(T netEvent, IPEndPoint sender)> _events = new();
    protected bool TryGetEvent(out T netEvent, out IPEndPoint sender) {
        if(_events.TryDequeue(out var contents)) {
            netEvent = contents.netEvent;
            sender = contents.sender;
            return true;
        }

        netEvent = default;
        sender = default;
        return false;
    }

    // Invocation from general object to actual parameter type
    internal override void Invoke(byte[] data, IPEndPoint senderEndPoint) {
        _events.Enqueue((Serializer.Deserialize<T>(new MemoryStream(data)), senderEndPoint));
    }
}