using System;
using System.Linq;
using System.Net;
using Dungeoner;
using Dungeoner.Importers;
using Dungeoner.Maps;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Godot;

[NetworkEventModel("TileRemoved")]
public record TileRemovedModel((int, int)[] Placements) : NetworkEventModel;

[NetworkEvent("TileRemoved")]
public partial class TileRemovedEventHandler : NetworkEventHandler<TileRemovedModel>
{
    [Export] private Dungeoner.Maps.TileMap _map = default!;

    protected override void OnClientEventProcess(TileRemovedModel netEvent, ClientCallback _callback)
    {
        foreach ((int x, int y) in netEvent.Placements)
            _map.Remove(new(x, y));
    }

    protected override void OnHostEventProcess(TileRemovedModel netEvent, IPEndPoint sender, HostCallback callback) { }
}
