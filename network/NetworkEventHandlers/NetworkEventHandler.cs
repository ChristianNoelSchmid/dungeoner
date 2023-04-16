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

namespace Dungeoner.Server.Events.NetworkEventHandlers;

public abstract partial class NetworkEventHandler : Node { 
    internal NetworkManager _manager;
    internal abstract void Invoke(byte[] data, IPEndPoint senderEndPoint); 
}

public abstract partial class NetworkEventHandler<T> : NetworkEventHandler {
    private readonly ConcurrentQueue<(T netEvent, IPEndPoint sender)> _events = new();
    private bool TryGetEvent(out T? netEvent, out IPEndPoint? sender) {
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
    // Called by the 
    internal override void Invoke(byte[] data, IPEndPoint senderEndPoint) {
        _events.Enqueue((Serializer.Deserialize<T>(new MemoryStream(data)), senderEndPoint));
    }

    // Called when an Event is processed
    protected abstract void OnHostEventProcess(T netEvent, IPEndPoint sender, HostCallback callback);
    protected abstract void OnClientEventProcess(T netEvent, ClientCallback callback);

    public override void _Process(double delta) {
        while(TryGetEvent(out var netEvent, out var sender)) {
            if(_manager.IsHost) {
                OnHostEventProcess(
                    netEvent!, sender!, new HostCallback(
                        SendToCaller: (model, isRel) => _manager.SendTo(sender!, model, isRel),
                        SendToOthers: (model, isRel) => _manager.SendToOthers(sender!, model, isRel),
                        SendToAll: (model, isRel) => _manager.SendToAll(model, isRel)
                    )
                );
            } else { 
                OnClientEventProcess(
                    netEvent!, new ClientCallback(
                        SendToHost: (model, isRel) => _manager.SendToHost(model, isRel)
                    )
                );
            }
        }
    }
}