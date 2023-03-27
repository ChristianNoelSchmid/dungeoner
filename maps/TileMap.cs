
using System;
using System.Collections.Generic;
using System.Linq;
using Dungeoner.GodotExtensions;
using Dungeoner.Importers;
using Godot;

namespace Dungeoner.Maps;

public partial class TileMap : Node2D {

    private Dictionary<Vector2I, Tile> _tiles = new();

    /// <summary>
    /// Retrieves the Tile located at the given position, or null if none is found
    /// </summary>
    public Tile this[Vector2I position] {
        get => _tiles.GetValueOrDefault(position) ?? throw new ArgumentException("Tile not found", nameof(position));
        set {
		    _tiles[position] = value;
		    QueueRedraw();
        }
    }
    /// <summary>
    /// Removes the Tile from the given position, returning whether a value was found.
    /// </summary>
	public bool Remove(Vector2I position) {
		if(_tiles.Remove(position)) {
            QueueRedraw();
            return true;
        }
        return false;
	}
    /// <summary>
    /// Returns all 8 neighbor tiles adjacent to the given position.
    /// Assigns null for neighbors that don't exist
    /// </summary>
    /// <param name="position">The center of the tiles returned</param>
    /// <returns>A tuple of positions mapped to their Tile, or null if no Tile is at that position</returns>
	public IEnumerable<(Vector2I, Tile?)> GetNeighborTiles(Vector2I position)
		=> position.GetNeighbors().Select(p => (p, _tiles.GetValueOrDefault(p)));
}