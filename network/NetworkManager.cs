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
using System.Threading;
using Dungeoner.Server.Exceptions;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Dungeoner.Collections;
using Dungeoner.Server;

public partial class NetworkManager : Node
{
    private const int HOST_PORT = 8000;
    private const int GUID_BYTE_COUNT = 16;
    [Export] private UserMap _userMap = default!;
    [Export] private ClientSynchronizer _sync = default!;
    private int _port = HOST_PORT;

    // The datagram handler
    private NetworkDatagramHandler _datagramHandler = default!;
    // Collection of network event invokers, and the type they represent
    private IReadOnlyDictionary<string, NetworkEventHandler> _invokers = default!;
    // Collection of network models, mapped to their event names
    private IReadOnlyDictionary<Type, string> _models = default!;
    // A ConcurrentQueue storing all outgoing messages
    private ConcurrentQueue<DatagramOutgoingMessage> _sendQueue = new();
    // Stores collection of Guids that each IPEndPoint is assigned.
    // Used to synchronize the UserMap
    private BiMap<IPEndPoint, Guid> _endPointIds = new();

    // Buffer for all possible event names
    private byte[] _buffer = Array.Empty<byte>();
    private CancellationTokenSource? _tokenSource;

    private List<Exception> _exceptions = new();
    private int _bufferLength = 0;

    Random random = new();

    private bool _isHost = true;
    public bool IsHost
    {
        get => _isHost;
        set
        {
            _isHost = value;
            bool isListening = _datagramHandler.IsListening;
            if (isListening) _datagramHandler.Close();
            _port = _isHost ? HOST_PORT : random.Next(8001, 8999);
            if (isListening) _datagramHandler.Open(_port);
        }
    }

    public void Open()
    {
        _datagramHandler.Open(_port);
        if (!IsHost) SendToHost(new HelloModel($"Test{random.Next(0, 1000)}"), false);
    }
    public void Close() => _datagramHandler?.Close();

    public Guid GetUserId(IPEndPoint endPoint) => _endPointIds[endPoint];

    /// <summary>
    /// Initializes the NetworkManager with its specific settings.
    /// </summary>
    /// <param name="serverType">Whether this is the host or a client server</param>
    public void Setup(NetworkServerType serverType)
    {
        bool datagramHandlerRunning = _datagramHandler.IsListening;
        if (datagramHandlerRunning) _datagramHandler.Close();

        // Cancel current threads if any exist
        _tokenSource?.Cancel();

        Dictionary<string, NetworkEventHandler> invokers = new();
        Dictionary<Type, string> models = new();

        // A queue of all (recursive) children for this Node
        Queue<Node> nodes = new();

        // Enqueue all direct children of the NetworkManager
        foreach (var child in GetChildren()) nodes.Enqueue(child);

        // For every child in the NetworkManager, check if each is a NetworkEventHandler
        // with the correct server type (Host or Client), and add it to the collection
        // of invokers
        while (nodes.TryDequeue(out var node))
        {
            if (node is NetworkEventHandler netEventHandler)
            {
                var netEventAttr = node.GetType().GetCustomAttribute(typeof(NetworkEventAttribute)) as NetworkEventAttribute;

                // Throw an exception if the NetworkEventHandler doesn't have the NetworkEventAttribute assigned to it
                // (they must always be coupled together)
                if (netEventAttr == null)
                {
                    throw new NetworkManagerException($"NetworkEventAttribute not found on class inheriting NetworkEventHandler: {node.GetType()}");
                }

                // Set the NetworkEventHandler's NetworkManager to this instance
                netEventHandler._manager = this;

                invokers.Add(netEventAttr.Name, netEventHandler);
            }
            // Enqueue each of the node's children to be checked as well
            foreach (var child in node.GetChildren()) nodes.Enqueue(child);
        }

        // Find all NetworkEventModels
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(type => type.BaseType == typeof(NetworkEventModel)))
        {
            // Throw an exception if the NetworkEventModel doens't have the NetworkEventModelAttrbute assigned to it
            // (they must always be coupled together)
            var networkEventModelAttribute = type.GetCustomAttribute(typeof(NetworkEventModelAttribute)) as NetworkEventModelAttribute;
            if (networkEventModelAttribute == null)
            {
                throw new NetworkManagerException($"NetworkEventModelAttribute not found on class inheriting NetworkEventModel");
            }
            models[type] = networkEventModelAttribute!.Name;
        }

        // Update the read-buffer to be able to store that largest possible name for a network event
        _buffer = new byte[models.Values.Max(eventName => eventName.Length)];
        _bufferLength = _buffer.Length + 1;
        foreach ((Type typ, string eventName) in models)
        {
            models[typ] = eventName.PadRight(_bufferLength, ' ');
        }
        var keys = invokers.Keys.ToArray();
        foreach (string key in keys)
        {
            invokers[key.PadRight(_bufferLength, ' ')] = invokers[key];
            invokers.Remove(key);
        }

        // Assign all found invokers and models
        _invokers = invokers;
        _models = models;


        _tokenSource = new CancellationTokenSource();
        _ = RunParser(_tokenSource.Token);

        if (datagramHandlerRunning) _datagramHandler.Open(_port);
    }
    public override void _Ready()
    {
        _datagramHandler = new();
        _datagramHandler.MessageRecieved += OnMessageReceived;
        _datagramHandler.LostConnection += OnConnectionLost;
        Setup(NetworkServerType.Host);
    }

    internal void QueueMessage(
        DatagramOutgoingSendType sendType,
        NetworkEventModel model,
        bool isRel,
        params IPEndPoint[]? ipAddresses
    )
    {
        if (!_datagramHandler.IsListening) return;
        var eventNameBytes = Encoding.UTF8.GetBytes(_models[model.GetType()].PadRight(_bufferLength, ' '));

        var stream = new MemoryStream();
        Serializer.Serialize(stream, model);

        IEnumerable<byte> msgBytes = stream.ToArray();

        // If this is a client to host message, append the Id of the Client to the datagram 
        // (or (Guid)default if authorization has not yet occurred
        if (sendType == DatagramOutgoingSendType.SendToHost)
        {
            msgBytes = _userMap.ClientId.ToByteArray().Concat(msgBytes);
        }

        _sendQueue.Enqueue(new(sendType, eventNameBytes.Concat(msgBytes).ToArray(), isRel, ipAddresses));
    }

    public void SendTo(User user, NetworkEventModel netEvent, bool isRel)
        => SendTo(_endPointIds[user.Id], netEvent, isRel);

    public void SendTo(IPEndPoint endPoint, NetworkEventModel netEvent, bool isRel)
        => QueueMessage(DatagramOutgoingSendType.SendTo, netEvent, isRel, endPoint);

    public void SendToAll(NetworkEventModel netEvent, bool isRel)
    {
        if(!IsHost) SendToHost(netEvent, isRel);
        else QueueMessage(DatagramOutgoingSendType.SendTo, netEvent, isRel, _endPointIds.Select(pair => pair.Item1).ToArray());
    }
    public void SendToOthers(IPEndPoint allButEndPoint, NetworkEventModel netEvent, bool isRel)
    {
        var endPoints = _endPointIds.Select(pair => pair.Item1).Where(p => !p.Equals(allButEndPoint)).ToArray();
        QueueMessage(DatagramOutgoingSendType.SendTo, netEvent, isRel, endPoints);
    }
    public void SendToHost(NetworkEventModel netEvent, bool isRel)
    {
        if (IsHost) throw new NetworkManagerException($"Attempted to send message to host as the host: {netEvent}");
        QueueMessage(DatagramOutgoingSendType.SendToHost, netEvent, isRel, IPEndPoint.Parse($"127.0.0.1:{HOST_PORT}"));
    }
    public void SendToUsers(NetworkEventModel netEvent, bool isRel, params Guid[] ids)
    {
        foreach (var id in ids)
        {
            QueueMessage(DatagramOutgoingSendType.SendTo, netEvent, isRel, _endPointIds[id]);
        }
    }

    public override void _Process(double delta)
    {
        foreach (var ex in _exceptions) GD.PrintErr(ex);
        _exceptions.Clear();
    }
    private async Task RunParser(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            while (_sendQueue.TryDequeue(out var msg))
            {
                if (_datagramHandler.IsListening)
                {
                    await _datagramHandler.SendAsync(msg);
                }
            }
            await Task.Delay(100);
        }
    }
    private void OnMessageReceived(object? _sender, DatagramIncomingMessage callback)
    {
        try
        {
            var eventName = Encoding.UTF8.GetString(callback.Data, 0, _bufferLength).PadRight(_bufferLength, ' ');

            // If the event is an auth event, handle it via the authorization process
            // If auth process is successful (or not an auth process)
            // inject the model into the NetworkEventHandler system
            var result = HandleAuth(callback, eventName);

            // If authorization was successful, but the message still needs to be processed
            // continue to NetworkEventHandler injection
            if (result.IsSuccessful && !result.IsCompleted)
            {
                if (_invokers.TryGetValue(eventName, out var invoker))
                {
                    invoker.Invoke(result.Data!, callback.EndPoint);
                }
            }
        }
        catch (Exception ex)
        {
            _exceptions.Add(ex);
        }
    }

    private void OnConnectionLost(object? _sender, IPEndPoint endPoint)
    {
        var clientId = _endPointIds[endPoint];
        _userMap.RemoveId(clientId);

        if (IsHost) SendToAll(new ClientDisconnectModel(clientId), true);
    }

    private NetworkAuthResult HandleAuth(DatagramIncomingMessage callback, string eventName)
    {
        var bytes = callback.Data[_bufferLength..];

        // If the server is just pinging this one, transaction is completed 
        if (eventName == _models[typeof(PingModel)]) return NetworkAuthResult.Completed;
        Guid? clientId = null;

        // If this is the host server, get the client id
        // Auth requests will have zeroed-out client ids
        // which is handled below
        if (IsHost)
        {
            var id = new Guid(bytes[..GUID_BYTE_COUNT]);
            if (_userMap.ContainsId(id))
            {
                clientId = id;
            }
            bytes = bytes[GUID_BYTE_COUNT..];
        }

        // If a client is requesting authorization
        if (eventName == _models[typeof(HelloModel)] && IsHost)
        {
            // Deserialize the model
            var model = Serializer.Deserialize<HelloModel>(new MemoryStream(bytes));

            // If there are any other connected users with the same username,
            // send a UsernameInUserModel back
            if (_userMap.Users.Any(val => val.Username == model.Username))
            {
                QueueMessage(DatagramOutgoingSendType.SendTo, new UsernameInUseModel(), false, callback.EndPoint);
                return NetworkAuthResult.Failed;
            }

            // Otherwise, create a new Guid and assign it to the new User.
            // Inform the new User of this, and all other connected Users
            var newId = Guid.NewGuid();
            _endPointIds[callback.EndPoint] = newId;
            _userMap[newId] = new User(model.Username, newId);

            QueueMessage(DatagramOutgoingSendType.SendTo, new WelcomeModel(newId, _userMap.Users.ToArray()), false, callback.EndPoint);
            // QueueMessage(DatagramOutgoingSendType.SendToOthers, new NewClientModel(newId, model.Username), true, callback.EndPoint);
            _sync.EnqueueEndPoint(callback.EndPoint);

            return NetworkAuthResult.Completed;

            // If authorization is successful and the host has given the client an id
        }
        else if (eventName == _models[typeof(WelcomeModel)] && !IsHost)
        {
            // Deserialize the model
            var model = Serializer.Deserialize<WelcomeModel>(new MemoryStream(bytes));

            // Assign the client's User Id, and add all Users currently connected
            _userMap.ClientId = model.NewId;
            foreach (var user in _userMap.Users) _userMap[user.Id] = user;

            return NetworkAuthResult.Completed;

            // If the host is informing a client that a new User has connected
        }
        else if (eventName == _models[typeof(NewClientModel)] && !IsHost)
        {
            // Deserialize the model
            var model = Serializer.Deserialize<NewClientModel>(new MemoryStream(bytes));

            // Assign the new User to the map
            _userMap[model.NewClientId] = new User(model.Username, model.NewClientId);
            return NetworkAuthResult.Completed;

        }

        // If this is a client request but could not be authenticated
        // return an AuthRequiredModel
        if (IsHost && clientId == null)
        {
            SendTo(callback.EndPoint, new AuthRequiredModel(), false);
            return NetworkAuthResult.Failed;
        }

        return new NetworkAuthResult(bytes);
    }
}