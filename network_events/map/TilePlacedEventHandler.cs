using System.Linq;
using System.Net;
using Dungeoner.Importers;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Godot;

public record TilePlacements(string Key, (int, int)[] Positions);

[NetworkEventModel("TilePlaced")]
public record TilePlacedModel(TilePlacements[] Placements) : NetworkEventModel;

[NetworkEvent("TilePlaced")]
public partial class TilePlacedEventHandler : NetworkEventHandler<TilePlacedModel>
{
    [Export] private TileImporter _importer = default!;
    [Export] private Dungeoner.Maps.TileMap _map = default!;

    protected override void OnClientEventProcess(TilePlacedModel netEvent, ClientCallback _callback)
    {
        foreach(var placements in netEvent.Placements) 
        {
            var meta = _importer.GetAllMatchingMetas(placements.Key).FirstOrDefault();

            foreach((int x, int y) in placements.Positions) 
                if (meta != null)
                    foreach (var placement in netEvent.Placements)
                        _map[new(x, y)] = meta;
        }
    }

    protected override void OnHostEventProcess(TilePlacedModel netEvent, IPEndPoint sender, HostCallback callback) { }
}
