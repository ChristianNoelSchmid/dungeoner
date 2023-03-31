
using Dungeoner.Collections;
using Dungeoner.GodotExtensions;
using Dungeoner.Importers;
using Godot;
using System.Collections.Generic;
using System.Linq;

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
            // If the wall was drawn on the left or right edge, it is a side panel
            if(direction == Direction.Left) {

            // If the wall was drawn on the top or bottom edge, it is a back panel   
            } else if(direction == Direction.Up || direction == Direction.Down) {

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
            wallPanel.Position = gridPosition * Constants.GRID_SIZE - new Vector2(0, Constants.GRID_SIZE / 2);
            // Add the WallPanelRenderer to the World and set it up
            _world.AddChild(wallPanel);

            // Insert the sprite and the metadata into the collections
            _wallRenderers.InsertEdge(gridPosition, edge, wallPanel);
            _wallMetas.InsertEdge(gridPosition, edge, value);
        }
        _uncommittedWalls[(gridPosition, edge)] = true;
        _uncommittedTiles[gridPosition] = _tileMap.ContainsPosition(gridPosition);
        foreach(var neighborPosition in gridPosition.GetNeighbors()) {
            _uncommittedTiles[neighborPosition] = _tileMap.ContainsPosition(neighborPosition);
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

            _uncommittedWalls.Add((from, edge), false);
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
            _uncommittedWalls.Add((gridPosition, edge), true);

            _uncommittedTiles[gridPosition] = _tileMap.ContainsPosition(gridPosition);
            foreach(var neighbor in gridPosition.GetNeighbors()) {
                _uncommittedTiles[neighbor] = _tileMap.ContainsPosition(neighbor);
            }
        }
    }

    public void AddTileUpdate(Vector2I gridPosition, bool active) => _uncommittedTiles[gridPosition] = active;
}