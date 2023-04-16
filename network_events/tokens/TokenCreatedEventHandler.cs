using System;
using System.Linq;
using System.Net;
using Dungeoner.Importers;
using Dungeoner.Maps;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Godot;

[NetworkEventModel("TokenCreated")]
public record TokenCreatedModel(string Key, Guid TokenId, float X, float Y) : NetworkEventModel;

[NetworkEvent("TokenCreated")]
public partial class TokenCreatedEventHandler : NetworkEventHandler<TokenCreatedModel> {
    [Export] private TokenImporter _importer = default!;
    [Export] private TokenMap _map = default!;

    protected override void OnClientEventProcess(TokenCreatedModel netEvent, ClientCallback _callback) {
        var meta = _importer.GetAllMatchingMetas(netEvent.Key).FirstOrDefault();

        if(meta != null) {
            var token = _importer.GetToken(meta);
            _map[netEvent.TokenId] = token;

            token.GlobalPosition = new(netEvent.X, netEvent.Y);
            token.MapPosition = token.GlobalPosition;
        }
    }

    protected override void OnHostEventProcess(TokenCreatedModel netEvent, IPEndPoint sender, HostCallback callback) { }
}
