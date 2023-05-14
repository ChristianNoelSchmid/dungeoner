
using System.Collections.Generic;
using Dungeoner.Importers;
using Godot;

namespace Dungeoner.Maps;

public abstract partial class WallPanelRenderer : Node2D
{
    public abstract void UpdateWallMeta(WallInstance meta, Vector2I gridPosition);

    /// <summary>
    /// Updates the WallPanelRenderer based on the adjacent neighbor edges
    /// </summary>
    /// <param name="direction">The direction the edge is relative to the edge</param>
    public abstract void UpdateNeighborEdge(Direction direction, bool active);
    /// <summary>
    /// Updated the WallPanelRenderer based on the adjacent neighbor tiles
    /// </summary>
    /// <param name="direction">The direction the tile is relative to the edge</param>
    public abstract void UpdateNeighborTile(Direction direction, bool active);
}