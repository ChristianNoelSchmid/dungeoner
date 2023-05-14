using System;
using System.Linq;
using System.Net;
using Dungeoner;
using Dungeoner.Importers;
using Dungeoner.Maps;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Godot;

public record WallPlacement(int X, int Y, Direction Direction);

[NetworkEventModel("WallPlaced")]
public record WallPlacedModel(string Key, WallPlacement[] Placements) : NetworkEventModel;

[NetworkEvent("WallPlaced")]
public partial class WallPlacedEventHandler : NetworkEventHandler<WallPlacedModel>
{
    [Export] private WallImporter _importer = default!;
    [Export] private WallMap _map = default!;

    protected override void OnClientEventProcess(WallPlacedModel netEvent, ClientCallback _callback)
    {
        var meta = _importer.GetAllMatchingMetas(netEvent.Key).FirstOrDefault();

        if (meta != null)
        {
            foreach (var placement in netEvent.Placements)
            {
                _map.AddWallPanel(new(placement.X, placement.Y), placement.Direction, meta);
            }
        }
    }

    protected override void OnHostEventProcess(WallPlacedModel netEvent, IPEndPoint sender, HostCallback callback) { }
}
