using Dungeoner;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Godot;
using System;
using System.Net;

[NetworkEventModel("RulerOn")]
public record RulerOnModel(Guid Id, (float X, float Y) Start, (float X, float Y) End) : NetworkEventModel;

[NetworkEvent("RulerOn")]
public partial class RulerOnEventHandler : NetworkEventHandler<RulerOnModel>
{
	[Export]
	private RulerTool _rulerTool = default!;

    protected override void OnHostEventProcess(RulerOnModel netEvent, IPEndPoint sender, HostCallback callback)
    {
        _rulerTool.AddOrUpdate(netEvent.Id, new(netEvent.Start.X, netEvent.Start.Y), new(netEvent.End.X, netEvent.End.Y));
        callback.SendToOthers(netEvent, false);
    }

    protected override void OnClientEventProcess(RulerOnModel netEvent, ClientCallback callback)
    {
		_rulerTool.AddOrUpdate(netEvent.Id, new(netEvent.Start.X, netEvent.Start.Y), new(netEvent.End.X, netEvent.End.Y));
    }
}
