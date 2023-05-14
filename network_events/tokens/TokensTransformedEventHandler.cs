using System;
using System.Linq;
using System.Net;
using Dungeoner;
using Dungeoner.Maps;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Godot;

public record TokenTransform(Guid TokenId, float PositionX, float PositionY, float ScaleX, float ScaleY, Direction Direction);

[NetworkEventModel("TokensTransformed")]
public record TokensTransformedModel(Guid UserId, TokenTransform[] Transforms) : NetworkEventModel
{
    public override string ToString() => $"[{string.Join(",", Transforms.Select(tr => tr.ToString()))}]";
}

[NetworkEvent("TokensTransformed")]
public partial class TokensTransformedEventHandler : NetworkEventHandler<TokensTransformedModel>
{
    [Export] private PermissionsMap _permissionsMap = default!;
    [Export] private TokenMap _tokenMap = default!;

    protected override void OnClientEventProcess(TokensTransformedModel netEvent, ClientCallback _callback)
    {
        foreach (var tr in netEvent.Transforms)
        {
            _tokenMap[tr.TokenId].PivotPosition = new(tr.PositionX, tr.PositionY);
            _tokenMap[tr.TokenId].Direction = tr.Direction;
            _tokenMap.ScaleToken(tr.TokenId, new(tr.ScaleX, tr.ScaleY));
        }
    }

    protected override void OnHostEventProcess(TokensTransformedModel netEvent, IPEndPoint sender, HostCallback callback)
    {
        // Before transforming the Tokens, ensure the connected Client has control
        // over the tokens in question. If any one Token isn't under their control,
        // do not move any of them
        if (netEvent.Transforms.All(tr => _permissionsMap.UserCanControlToken(netEvent.UserId, tr.TokenId)))
        {
            OnClientEventProcess(netEvent, null!);
            callback.SendToOthers(netEvent, true);
        }
    }
}