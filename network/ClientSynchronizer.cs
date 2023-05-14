
using System.Collections.Generic;
using System.Net;
using Dungeoner.Maps;
using Godot;

namespace Dungeoner.Server;

public partial class ClientSynchronizer : Node
{
    [Export] private NetworkManager _netManager = default!;
    [Export] private TokenMap _tokenMap = default!;
    [Export] private Maps.TileMap _tileMap = default!;
    [Export] private WallMap _wallMap = default!;
    [Export] private PermissionsMap _permissionsMap = default!;

    private readonly Queue<IPEndPoint> _synchEvents = new();

    public void EnqueueEndPoint(IPEndPoint endPoint) => _synchEvents.Enqueue(endPoint);

    public override void _Process(double delta)
    {
        if (_synchEvents.TryDequeue(out var endPoint))
        {
            var userId = _netManager.GetUserId(endPoint);

            var tokensCreatedModel = _tokenMap.ToTokensCreatedModel(userId);
            var tilePlacedModel = _tileMap.ToTilePlacedModel();
            var wallPlacedModel = _wallMap.ToWallPlacedModel();

            if(tokensCreatedModel != null)
                _netManager.SendTo(endPoint, tokensCreatedModel, true);
            if(tilePlacedModel != null)
                _netManager.SendTo(endPoint, tilePlacedModel, true);
            if(wallPlacedModel != null)
                _netManager.SendTo(endPoint, wallPlacedModel, true);
        }
    }
}