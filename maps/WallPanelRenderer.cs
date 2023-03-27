
using System.Collections.Generic;
using Dungeoner.Importers;
using Godot;

namespace Dungeoner.Maps;

public abstract partial class WallPanelRenderer : Node2D {
	protected Vector2I _vertex1, _vertex2;

    /// <summary>
    /// Updates the wall panel with the given WallMeta information 
    /// </summary>
    /// <param name="wall">The WallMeta to update the panel with</param>
    public virtual void Setup(Vector2I vertex1, Vector2I vertex2, WallMeta wall) {
        _vertex1 = vertex1;
        _vertex2 = vertex2;
    }

    /// <summary>
    /// Updates the wall render based on the adjacent neighbor tiles and walls
    /// </summary>
    /// <param name="vertex1Neighbors">The neighbor vertices of the first vertex</param>
    /// <param name="vertex2Neighbors">The neighbor vertices of the second vertex</param>
    /// <param name="tileNeighbors">The wall's neighboring tiles</param>
    public abstract void UpdateWall(
        IEnumerable<(Vector2I, Vector2I, bool)> edgeNeighbors, 
        IEnumerable<(Vector2I, bool)> tileNeighbors
    );
}