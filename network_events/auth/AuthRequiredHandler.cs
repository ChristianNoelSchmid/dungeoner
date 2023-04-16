using System;
using System.Net;
using Dungeoner.Server.Events.NetworkEventHandlers;

[NetworkEventModel("AuthRequired")]
public record AuthRequiredModel : NetworkEventModel;

[NetworkEvent("AuthRequired")]
public partial class AuthRequiredHandler : NetworkEventHandler<HelloModel> {
    protected override void OnClientEventProcess(HelloModel netEvent, ClientCallback callback) { }
    protected override void OnHostEventProcess(HelloModel netEvent, IPEndPoint sender, HostCallback callback) { }
}