using System;
using System.Linq;
using System.Net;
using Dungeoner.Maps;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Godot;

[NetworkEventModel("UpdateTokenControl")]
public record UpdateTokenControlModel(Guid TokenId, Guid UserId, bool CanControl) : NetworkEventModel;

[NetworkEvent("UpdateTokenControl")]
public partial class UpdateTokenControlEventHandler : NetworkEventHandler<UpdateTokenControlModel>
{
    [Export] private PermissionsMap _permissionsMap = default!;

    protected override void OnClientEventProcess(UpdateTokenControlModel netEvent, ClientCallback _callback)
        => _permissionsMap.UpdateUserTokenControl(netEvent.UserId, netEvent.TokenId, netEvent.CanControl);

    protected override void OnHostEventProcess(UpdateTokenControlModel netEvent, IPEndPoint sender, HostCallback callback) { }
}