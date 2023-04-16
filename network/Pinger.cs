
using Dungeoner.Server.Events;
using Godot;

public partial class Pinger : Node {
    private const double PING_FREQUENCY = 3.0;

    [Export] private NetworkManager _manager = default!;
    [Export] private UserMap _userMap = default!;

    private double _lastPing;
    private double _total;

    public override void _Process(double delta) {
        _total += delta;
        if(_total - _lastPing > PING_FREQUENCY)  {
            if(_manager.IsHost) _manager.SendToAll(new PingModel(_userMap.ClientId), false);
            else if(_userMap.ClientId != default) _manager.SendToHost(new PingModel(_userMap.ClientId), false);
            _lastPing = _total;
        }
    }
}