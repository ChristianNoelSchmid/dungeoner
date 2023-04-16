
using Dungeoner.Collections;
using Dungeoner.GodotExtensions;
using Dungeoner.Importers;
using Godot;
using System.Collections.Generic;

namespace Dungeoner.Maps;

public partial class WallMap : Node2D {
    // The World to associate the WallGenerator to
    // All wall sprites are appended into this World
    [Export]
    private TokenMap _world = default!;

    // The TileGenerator containing all tiles which the WallGenerator
    // will set walls onto. Necessary in order to properly update
    // wall shadowing
    [Export]
    private TileMap _tileMap = default!;

    // The Scene representing a back-panel wall
    private PackedScene _backWallPane = ResourceLoader.Load<PackedScene>("res://maps/back_wall_template.tscn");
    // The Scene representing a side-panel wall
	private PackedScene _sideWallPane = ResourceLoader.Load<PackedScene>("res://maps/side_wall_template.tscn");

    // All metadata associated with each wall panel
	private GridEdgeGraph<WallMeta> _wallMetas = new();

    // Each WallPanelRenderer associated with the WallGenerator
	private GridEdgeGraph<WallPanelRenderer> _wallRenderers = new();

    private Dictionary<(Vector2I, Direction), bool> _uncommittedWalls = new();
    private Dictionary<Vector2I, bool> _uncommittedTiles = new();

    public override void _Process(double delta) {
        foreach(((var gridPosition, var direction), bool active) in _uncommittedWalls) {
            var dirVec = new DirectionalVec(gridPosition, direction);
            var neighbors = dirVec.GetNeighbors();
            foreach(var neighbor in neighbors) {
                if(_wallRenderers.TryGetEdge(neighbor.GridPosition, neighbor.Direction, out var edge)) {
                    edge!.UpdateNeighborEdge(neighbor.DirectionTo(dirVec), active);
                }
            }
        }
        foreach((var gridPosition, bool active) in _uncommittedTiles) {
            if(_wallRenderers.TryGetEdges(gridPosition, out var edges)) {
                edges![(int)Direction.Left]?.UpdateNeighborTile(Direction.Right, active);
                edges[(int)Direction.Right]?.UpdateNeighborTile(Direction.Left, active);
                edges[(int)Direction.Up]?.UpdateNeighborTile(Direction.Down, active);
            }
        }
        _uncommittedWalls.Clear();
        _uncommittedTiles.Clear();
    }

    public bool TryGetEdge(DirectionalVec dir, out WallMeta? wallMeta) {
        return _wallMetas.TryGetEdge(dir.GridPosition, dir.Direction, out wallMeta);
    }

    /// <summary>
    /// Adds a new wall panel to the WallGenerator.
    /// Replaces already existing wall panels at that location
    /// </summary>
    /// <param name="gridPosition">The first vertex making up the wall</param>
    /// <param name="to">The second vertex making up the wall</param>
    /// <param name="value">The WallMeta to assign to the position</param>
    public void AddWallPanel(Vector2I gridPosition, Direction edge, WallMeta value)  {
        if(_wallMetas.ContainsEdge(gridPosition, edge)) {
            _wallMetas.InsertEdge(gridPosition, edge, value);
            _wallRenderers.TryGetEdge(gridPosition, edge, out var sprite);
        } else {
            // Create the new WallPanelRenderer, assigning either a side or back panel
            // based on the direction the vertices are directed
            WallPanelRenderer wallPanel;
            if(edge == Direction.Left || edge == Direction.Right) {
                wallPanel = (WallPanelRenderer)_sideWallPane.Instantiate();
            } else {
                wallPanel = (WallPanelRenderer)_backWallPane.Instantiate();
            }
            // Assign the position of the WallPanelRenderer to the grid position, with an offset to the 
            // top-left corner of the grid box
            wallPanel.Position = gridPosition * Constants.GRID_SIZE + (Vector2)edge.Vector() * (Constants.GRID_SIZE / 2.0f);
            // Add the WallPanelRenderer to the World and set it up
            _world.AddChild(wallPanel);
            wallPanel.UpdateWallMeta(value, gridPosition);

            // Insert the sprite and the metadata into the collections
            _wallRenderers.InsertEdge(gridPosition, edge, wallPanel);
            _wallMetas.InsertEdge(gridPosition, edge, value);
        }
        _uncommittedWalls[(gridPosition, edge)] = true;
        _uncommittedTiles[gridPosition] = _tileMap.TryGetTile(gridPosition, out _);
        foreach(var edgeNeighbor in new DirectionalVec(gridPosition, edge).GetNeighbors()) {
            _uncommittedWalls[(edgeNeighbor.GridPosition, edgeNeighbor.Direction)] 
                = _wallMetas.ContainsEdge(edgeNeighbor.GridPosition, edgeNeighbor.Direction);
        }
        foreach(var neighborPosition in gridPosition.GetNeighbors()) {
            _uncommittedTiles[neighborPosition] = _tileMap.TryGetTile(neighborPosition, out _);
        }
    }

    /// <summary>
    /// Removes the wall panel at the specified position
    /// </summary>
    public void RemoveWallPanel(Vector2I from, Direction edge) {
        if(_wallRenderers.TryGetEdge(from, edge, out var wall)) {
            _world.RemoveChild(wall);
            _wallRenderers.RemoveEdge(from, edge);
            _wallMetas.RemoveEdge(from, edge);

            _uncommittedWalls[(from, edge)] = false;
            foreach(var edgeNeighbor in new DirectionalVec(from, edge).GetNeighbors()) {
                _uncommittedWalls[(edgeNeighbor.GridPosition, edgeNeighbor.Direction)] 
                    = _wallMetas.ContainsEdge(edgeNeighbor.GridPosition, edgeNeighbor.Direction);
            }
        }
    }

    /// <summary>
    /// Clears the WallGenerator of all walls
    /// </summary>
    public void Clear() {
        foreach(var sprite in _wallRenderers.Edges()) {
            _world.RemoveChild(sprite.edgeValue);
        }
        _wallRenderers.Clear();
        _wallMetas.Clear();
    }

    public void CopyTo(WallMap wallMap) {
        foreach((var vertex1, var dir, var edge) in _wallMetas.Edges()) {
            wallMap.AddWallPanel(vertex1, dir, edge);
        }
        foreach((var gridPosition, var edge, var renderer) in _wallRenderers.Edges()) {
            _uncommittedWalls[(gridPosition, edge)] = true;

            _uncommittedTiles[gridPosition] = _tileMap.TryGetTile(gridPosition, out _);
            foreach(var neighbor in gridPosition.GetNeighbors()) {
                _uncommittedTiles[neighbor] = _tileMap.TryGetTile(neighbor, out _);
            }
        }
    }

    public void RemoveAll(WallMap wallMap) {
        foreach((var vertex1, var edge, _) in wallMap._wallMetas.Edges()) {
            RemoveWallPanel(vertex1, edge);
        }
    }

    public void AddTileUpdate(Vector2I gridPosition, bool active) => _uncommittedTiles[gridPosition] = active;
}