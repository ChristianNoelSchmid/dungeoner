
using System;
using System.Net;
using Dungeoner.Server.Events.NetworkEventHandlers;

[NetworkEventModel("UsernameInUse")]
public record UsernameInUseModel : NetworkEventModel;

[NetworkEvent("UsernameInUse")]
public partial class UsernameInUseHandler : NetworkEventHandler<UsernameInUseModel>
{
    protected override void OnClientEventProcess(UsernameInUseModel netEvent, ClientCallback callback)
    {
        // TODO - popup that informs user th    at they need to choose a different username
    }
    protected override void OnHostEventProcess(UsernameInUseModel netEvent, IPEndPoint sender, HostCallback callback) { }
}