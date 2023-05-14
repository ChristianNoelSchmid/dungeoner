using System;
using System.Net;
using Dungeoner.Maps;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Godot;

[NetworkEventModel("TokensDeleted")]
public record TokensDeletedModel(Guid UserId, Guid[] TokenIds) : NetworkEventModel;

[NetworkEvent("TokensDeleted")]
public partial class TokensDeletedEventHandler : NetworkEventHandler<TokensDeletedModel>
{
    [Export] private TokenMap _tokenMap = default!;
    [Export] private PermissionsMap _permissionsMap = default!;

    protected override void OnClientEventProcess(TokensDeletedModel netEvent, ClientCallback _callback) 
        => DeleteTokensFromEvent(netEvent);

    protected override void OnHostEventProcess(TokensDeletedModel netEvent, IPEndPoint sender, HostCallback callback) { 
        if(_permissionsMap[netEvent.UserId, Permission.DeleteTokens]) {
            DeleteTokensFromEvent(netEvent);
            callback.SendToOthers(netEvent, true);
        }
    }

    private void DeleteTokensFromEvent(TokensDeletedModel netEvent)
    {
        foreach (var id in netEvent.TokenIds)
            _tokenMap.RemoveToken(id);
    }
}
