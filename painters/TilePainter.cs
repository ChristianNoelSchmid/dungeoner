using System.Collections.Generic;
using System.Linq;
using Dungeoner.GodotExtensions;
using Dungeoner.Importers;
using Dungeoner.Maps;
using Dungeoner.Ui;
using Godot;

namespace Dungeoner.Painters;

public partial class TilePainter : Node2D
{
    [Export] private UiMain _ui = default!;
    [Export] private TileImporter _importer = default!;
    [Export] private TokenMap _world = default!;
    [Export] private Maps.TileMap _worldTileMap = default!;
    [Export] private Maps.TileMap _commitTileMap = default!;
    [Export] private WallMap _worldWallMap = default!;
    [Export] private NetworkManager _networkManager = default!;

    public TileFillType FillType { get; set; } = TileFillType.Single;

    private Vector2 _halfShadowSize;
    private Vector2 _halfGridSize;

    private TileInstance? _selectedTile;

    private Node2D _floorNode = new();

    private bool _activated = false;
    public bool Activated
    {
        get => _activated;
        set
        {
            _activated = value;
        }
    }

    public void UpdateTileTexture(TileInstance tile)
    {
        _selectedTile = tile;
    }

    public override void _Ready()
    {
        _halfGridSize = new Vector2(Constants.GRID_SIZE / 2f, Constants.GRID_SIZE / 2f);
        AddChild(_floorNode);
    }

    public override void _Process(double delta)
    {
        var mouseGridPosition = GetGlobalMousePosition().ToGridPosition();
        if (Input.IsActionPressed("select") && !_ui.IsFocused && _selectedTile != null && Activated)
        {
            if (FillType == TileFillType.Single)
            {
                _worldTileMap[mouseGridPosition] = _selectedTile;
                _networkManager.SendToAll(
                    new TilePlacedModel(
                        new [] { 
                            new TilePlacements(
                                _selectedTile.Part.Key, 
                                new [] { (mouseGridPosition.X, mouseGridPosition.Y) }
                            )
                        }
                    ), true);
            }
            else
            {
                foreach ((var position, var tile) in _commitTileMap)
                {
                    _worldTileMap[position] = tile;
                }
                _networkManager.SendToAll(
                    new TilePlacedModel(
                        new [] { 
                            new TilePlacements(
                                _selectedTile.Part.Key,
                                _commitTileMap.Select(pair => (pair.Key.X, pair.Key.Y)).ToArray()
                            )
                        }
                    ),
                    true
                );
            }
        }
        if (Input.IsActionPressed("right-click") && !_ui.IsFocused && _selectedTile != null && Activated)
        {
            if (FillType == TileFillType.Single)
            {
                _worldTileMap.Remove(mouseGridPosition);
                _networkManager.SendToAll(
                    new TileRemovedModel(new [] { (mouseGridPosition.X, mouseGridPosition.Y) }), 
                    true
                );
            }
            else
            {
                foreach ((var position, var tile) in _commitTileMap)
                {
                    _worldTileMap.Remove(position);
                }
                _networkManager.SendToAll(
                    new TileRemovedModel(_commitTileMap.Select(pair => (pair.Key.X, pair.Key.Y)).ToArray()),
                    true
                );
            }
        }

        _commitTileMap.Clear();

        if (!Activated) return;

        if (_selectedTile != null)
        {
            if (FillType == TileFillType.Single)
            {
                _commitTileMap[mouseGridPosition] = _selectedTile;
            }
            else
            {
                Queue<DirectionalVec> next = new();
                HashSet<DirectionalVec> considered = new();

                var directions = new[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };

                int count = 0;
                _worldTileMap.TryGetTile(mouseGridPosition, out var target);

                foreach (var direction in directions) next.Enqueue(new(mouseGridPosition, direction));
                _commitTileMap[mouseGridPosition] = _selectedTile;

                while (next.Any())
                {
                    var dirVec = next.Dequeue();
                    var neighbor = dirVec.GridPosition.GetNeighbor(dirVec.Direction);
                    if (considered.Contains(dirVec)) continue;

                    if (!_worldWallMap.TryGetEdge(dirVec, out _))
                    {
                        _worldTileMap.TryGetTile(neighbor, out var test);
                        if (test == target)
                        {
                            _commitTileMap[neighbor] = _selectedTile;
                            foreach (var direction in directions) next.Enqueue(new(neighbor, direction));
                        }
                    }
                    considered.Add(dirVec);

                    count += 1;
                    if (count == 1000)
                    {
                        _commitTileMap.Clear();
                        break;
                    }
                }
            }
        }
    }
}
