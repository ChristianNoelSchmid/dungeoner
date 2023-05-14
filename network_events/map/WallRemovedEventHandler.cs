using System;
using System.Linq;
using System.Net;
using Dungeoner;
using Dungeoner.Importers;
using Dungeoner.Maps;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Godot;

[NetworkEventModel("WallRemoved")]
public record WallRemovedModel(WallPlacement[] Placements) : NetworkEventModel;

[NetworkEvent("WallRemoved")]
public partial class WallRemovedEventHandler : NetworkEventHandler<WallRemovedModel>
{
    [Export] private WallMap _map = default!;

    protected override void OnClientEventProcess(WallRemovedModel netEvent, ClientCallback _callback)
    {
        foreach (var placement in netEvent.Placements)
        {
            _map.RemoveWallPanel(new(placement.X, placement.Y), placement.Direction);
        }
    }

    protected override void OnHostEventProcess(WallRemovedModel netEvent, IPEndPoint sender, HostCallback callback) { }
}