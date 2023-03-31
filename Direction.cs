using System;
using System.Collections.Generic;
using Godot;

namespace Dungeoner;

public enum Direction {
    Up = 0, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft, Count
}

public static class DirectionExtensions {
    private static Vector2I s_xUnit = new(1, 0);
    private static Vector2I s_yUnit = new(0, 1);
    private static Vector2I s_dUnit = new(1, 1);

    public static Direction Reverse(this Direction dir) => (Direction)(((int)dir + (int)Direction.Count / 2) % (int)Direction.Count);
    public static IEnumerable<(Vector2I, Direction)> GetEdgeNeighbors(Vector2I gridPosition, Direction direction) {
        return direction switch {
            Direction.Up => new [] { 
                (gridPosition - s_yUnit, Direction.Right), (gridPosition + s_xUnit, Direction.Up), 
                (gridPosition, Direction.Right), (gridPosition, Direction.Left),
                (gridPosition - s_xUnit, Direction.Up), (gridPosition - s_yUnit, Direction.Left) 
            },
            Direction.Right => new [] { 
                (gridPosition - s_yUnit, Direction.Right), (gridPosition + s_xUnit, Direction.Up), 
                (gridPosition + s_xUnit, Direction.Down), (gridPosition + s_yUnit, Direction.Right),
                (gridPosition, Direction.Down), (gridPosition, Direction.Up) 
            },
            Direction.Down => new [] { 
                (gridPosition, Direction.Right), (gridPosition + s_xUnit, Direction.Down), 
                (gridPosition + s_yUnit, Direction.Right), (gridPosition + s_yUnit, Direction.Left),
                (gridPosition - s_xUnit, Direction.Down), (gridPosition, Direction.Left) 
            },
            Direction.Left => new [] { 
                (gridPosition - s_yUnit, Direction.Left), (gridPosition, Direction.Up), 
                (gridPosition, Direction.Down), (gridPosition + s_yUnit, Direction.Left),
                (gridPosition - s_xUnit, Direction.Down), (gridPosition - s_xUnit, Direction.Up) 
            },
            _ => throw new ArgumentException("Edges do not have diagonal neighbors", nameof(direction))
        };
    }
}