
using System;
using System.Collections.Generic;
using System.Linq;
using Dungeoner.GodotExtensions;
using Dungeoner.Importers;
using Godot;

namespace Dungeoner.Maps;

public partial class TileMap : Node2D {

    [Export]
    private WallMap _wallMap = default!;

    private Dictionary<Vector2I, Tile> _tiles = new();

    /// <summary>
    /// Retrieves the Tile located at the given position, or null if none is found
    /// </summary>
    public Tile this[Vector2I position] {
        get => _tiles.GetValueOrDefault(position) ?? throw new ArgumentException("Tile not found", nameof(position));
        set {
		    _tiles[position] = value;
            _wallMap.AddTileUpdate(position, true);
		    QueueRedraw();
        }
    }
    public bool ContainsPosition(Vector2I position) => _tiles.ContainsKey(position);

    /// <summary>
    /// Removes the Tile from the given position, returning whether a value was found.
    /// </summary>
	public bool Remove(Vector2I position) {
		if(_tiles.Remove(position)) {
            _wallMap.AddTileUpdate(position, false);
            QueueRedraw();
            return true;
        }
        return false;
	}

    public override void _Draw() {
        foreach((var gridPosition, var tile) in _tiles) {
            var rect = new Rect2(gridPosition * Constants.GRID_SIZE - Constants.GRID_VECTOR / 2, Constants.GRID_VECTOR);
            DrawTextureRect(tile.Texture, rect, true);
        }
    }
}