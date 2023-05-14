using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dungeoner.Importers;
using Dungeoner.Maps;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Godot;

public record TokenModel(string Key, Guid TokenId, (float, float) Position, bool ControlledByClient);

[NetworkEventModel("TokenCreated")]
public record TokensCreatedModel(Guid UserId, IEnumerable<TokenModel> Models) : NetworkEventModel;

[NetworkEvent("TokenCreated")]
public partial class TokensCreatedEventHandler : NetworkEventHandler<TokensCreatedModel>
{
    [Export] private TokenImporter _importer = default!;
    [Export] private TokenMap _map = default!;
    [Export] private PermissionsMap _permissionsMap = default!;

    protected override void OnClientEventProcess(TokensCreatedModel netEvent, ClientCallback _callback) 
        => CreateTokensFromEvent(netEvent);

    protected override void OnHostEventProcess(TokensCreatedModel netEvent, IPEndPoint sender, HostCallback callback) { 
        if(_permissionsMap[netEvent.UserId, Permission.CreateTokens]) {
            CreateTokensFromEvent(netEvent);
        }
    }

    private void CreateTokensFromEvent(TokensCreatedModel netEvent)
    {
        foreach (var model in netEvent.Models)
        {
            var meta = _importer.GetAllMatchingMetas(model.Key).FirstOrDefault();

            if (meta != null)
            {
                var token = _importer.GetToken(meta);
                _map[model.TokenId] = token;

                token.Teleport(new(model.Position.Item1, model.Position.Item2));
            }
        }
    }
}
