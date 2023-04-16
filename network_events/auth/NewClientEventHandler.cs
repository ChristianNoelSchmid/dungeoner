
using System;
using System.Net;
using Dungeoner.Server.Events;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Godot;

[NetworkEvent("NewClient")]
public partial class NewClientEventHandler : NetworkEventHandler<NewClientModel> {
    [Export] private UserMap _userMap = default!;

    protected override void OnClientEventProcess(NewClientModel netEvent, ClientCallback callback) {   
        // TODO - Create notifications for clients about new client
    }
    protected override void OnHostEventProcess(NewClientModel netEvent, IPEndPoint sender, HostCallback callback) { }
}