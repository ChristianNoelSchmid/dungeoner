
using Dungeoner.Collections;
using Dungeoner.Importers;
using Dungeoner.GodotExtensions;
using Godot;
using System.Linq;
using System;
using System.Collections.Generic;
using Dungeoner.Painters;

namespace Dungeoner.Maps;

public partial class WallMap : Node2D {
    // The World to associate the WallGenerator to
    // All wall sprites are appended into this World
    private TokenMap _world;

    // The TileGenerator containing all tiles which the WallGenerator
    // will set walls onto. Necessary in order to properly update
    // wall shadowing
    private TileMap _tileMap;

    // The Scene representing a back-panel wall
    private PackedScene _backWallPane = ResourceLoader.Load<PackedScene>("res://maps/back_wall_template.tscn");
    // The Scene representing a side-panel wall
	private PackedScene _sideWallPane = ResourceLoader.Load<PackedScene>("res://maps/side_wall_template.tscn");

    // All metadata associated with each wall panel
	private GridNeighborGraph<WallMeta> _wallMetas = new();
    // Each WallPanelRenderer associated with the WallGenerator
	private GridNeighborGraph<WallPanelRenderer> _wallSprites = new();

    public WallMap(TokenMap world, TileMap tileMap) {
        _world = world;
        _tileMap = tileMap;
    }

    /// <summary>
    /// Adds a new wall panel to the WallGenerator.
    /// Replaces already existing wall panels at that location
    /// </summary>
    /// <param name="from">The first vertex making up the wall</param>
    /// <param name="to">The second vertex making up the wall</param>
    /// <param name="value">The WallMeta to assign to the position</param>
    public void AddWallPanel(Vector2I from, Vector2I to, WallMeta value)  {
        // Create the new WallPanelRenderer, assigning either a side or back panel
        // based on the direction the vertices are directed
        WallPanelRenderer wallPanel;
		if(from.DirectionTo(to) == Direction.Up || from.DirectionTo(to) == Direction.Down) {
            wallPanel = (WallPanelRenderer)_sideWallPane.Instantiate();
        } else {
            wallPanel = (WallPanelRenderer)_backWallPane.Instantiate();
        }
        // Assign the position of the WallPanelRenderer to the grid position, with an offset to the 
        // top-left corner of the grid box
		wallPanel.Position = from * Constants.GRID_SIZE + new Vector2(Constants.GRID_SIZE / 2, Constants.GRID_SIZE / 2);
        // Add the WallPanelRenderer to the World and set it up
        _world.AddChild(wallPanel);
        wallPanel.Setup(from, to, value);

        // Insert the sprite and the metadata into the collections
        _wallSprites.InsertEdge(from, to, wallPanel);
        _wallMetas.InsertEdge(from, to, value);

        // Finally, update every neighbor wall, in case it's shadowing
        // or drywall needs to be updated
        for(Direction d = Direction.Up; d < Direction.Count; d += 1) {
            UpdateWallPanel(from, from.GetNeighbor(d));
            UpdateWallPanel(to, to.GetNeighbor(d));
        }
    }

    /// <summary>
    /// Removes the wall panel at the specified position
    /// </summary>
    public void RemoveWallPanel(Vector2I from, Vector2I to) {
        if(_wallSprites.TryGetEdge(from, to, out var wall)) {
            _world.RemoveChild(wall);
            _wallSprites.RemoveEdge(from, to);
            _wallMetas.RemoveEdge(from, to);

            QueueRedraw();
        }
    }

    /// <summary>
    /// Clears the WallGenerator of all walls
    /// </summary>
    public void Clear() {
        _wallMetas.Clear();
        foreach(var sprite in _wallSprites.Edges()) _world.RemoveChild(sprite.value);
        _wallSprites.Clear();
    }

    
    private void UpdateWallPanel(Vector2I from, Vector2I to) {
        if(_wallSprites.TryGetEdge(from, to, out var sprite)) {
            IEnumerable<(Vector2I, Tile?)> tiles = Array.Empty<(Vector2I, Tile?)>();
            tiles = _tileMap.GetNeighborTiles(from).Concat(_tileMap.GetNeighborTiles(to));

            sprite!.UpdateWall(
                Utilities.GetEdgeNeighbors(from, to).Select(pair => (pair.Item1, pair.Item2, _wallMetas.TryGetEdge(from, to, out _))), 
                tiles.Select(t => (t.Item1, t.Item2 != null))
            );
        }
    }
}