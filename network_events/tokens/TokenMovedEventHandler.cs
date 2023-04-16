using System;
using System.Net;
using Dungeoner.Maps;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Godot;

[NetworkEventModel("TokenMoved")]
public record TokenMovedModel(Guid TokenId, float X, float Y) : NetworkEventModel;

[NetworkEvent("TokenMoved")]
public partial class TokenMovedEventHandler : NetworkEventHandler<TokenMovedModel> {
    [Export] private TokenMap _map = default!;

    protected override void OnClientEventProcess(TokenMovedModel netEvent, ClientCallback _callback)  {
        _map.PositionToken(netEvent.TokenId, new(netEvent.X, netEvent.Y));
    }

    protected override void OnHostEventProcess(TokenMovedModel netEvent, IPEndPoint sender, HostCallback callback) {
        _map.PositionToken(netEvent.TokenId, new(netEvent.X, netEvent.Y));
        callback.SendToOthers(netEvent, true);
    }
}