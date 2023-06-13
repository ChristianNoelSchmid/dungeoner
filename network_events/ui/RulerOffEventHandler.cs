using Dungeoner;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Godot;
using System;
using System.Net;

[NetworkEventModel("RulerOff")]
public record RulerOffModel(Guid Id) : NetworkEventModel;

[NetworkEvent("RulerOff")]
public partial class RulerOffEventHandler : NetworkEventHandler<RulerOffModel>
{
	[Export]
	private RulerTool _rulerTool = default!;

    protected override void OnHostEventProcess(RulerOffModel netEvent, IPEndPoint sender, HostCallback callback)
    {
        _rulerTool.Remove(netEvent.Id);
        callback.SendToOthers(netEvent, true);
    }

    protected override void OnClientEventProcess(RulerOffModel netEvent, ClientCallback callback)
        => _rulerTool.Remove(netEvent.Id);
}
