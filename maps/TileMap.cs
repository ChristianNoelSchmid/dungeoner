
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dungeoner.GodotExtensions;
using Dungeoner.Importers;
using Godot;

namespace Dungeoner.Maps;

public partial class TileMap : Node2D, IEnumerable<KeyValuePair<Vector2I, TileInstance>>
{

    [Export]
    private WallMap? _wallMap = default!;

    private Dictionary<Vector2I, TileInstance> _tiles = new();

    /// <summary>
    /// Retrieves the Tile located at the given position, or null if none is found
    /// </summary>
    public TileInstance this[Vector2I position]
    {
        get => _tiles.GetValueOrDefault(position) ?? throw new ArgumentException("Tile not found", nameof(position));
        set
        {
            _tiles[position] = value;
            _wallMap?.AddTileUpdate(position, true);
            QueueRedraw();
        }
    }

    public bool TryGetTile(Vector2I position, out TileInstance? tile) => _tiles.TryGetValue(position, out tile);

    /// <summary>
    /// Removes the Tile from the given position, returning whether a value was found.
    /// </summary>
	public bool Remove(Vector2I position)
    {
        if (_tiles.Remove(position))
        {
            _wallMap?.AddTileUpdate(position, false);
            QueueRedraw();
            return true;
        }
        return false;
    }

    public void Clear() => _tiles.Clear();

    public override void _Draw()
    {
        foreach ((var gridPosition, var tile) in _tiles)
        {
            var rect = new Rect2(gridPosition * Constants.GRID_SIZE - Constants.GRID_VECTOR / 2, Constants.GRID_VECTOR);
            DrawTextureRect(tile.GetTexture(gridPosition), rect, true);
        }
    }

    public TilePlacedModel? ToTilePlacedModel() 
    {
        if(!_tiles.Any()) return null;

        Dictionary<string, List<(int, int)>> placements = new();
        foreach((var gridPosition, var instance) in _tiles) 
        {
            if(!placements.TryGetValue(instance.Part.Key, out var list))
            {
                list = new();
                placements.Add(instance.Part.Key, list);
            }
            list.Add((gridPosition.X, gridPosition.Y));
        }

        return new(placements.Select(pl => new TilePlacements(pl.Key, pl.Value.ToArray())).ToArray());
    }

    IEnumerator IEnumerable.GetEnumerator() => _tiles.GetEnumerator();

    IEnumerator<KeyValuePair<Vector2I, TileInstance>> IEnumerable<KeyValuePair<Vector2I, TileInstance>>.GetEnumerator()
        => _tiles.GetEnumerator();
}