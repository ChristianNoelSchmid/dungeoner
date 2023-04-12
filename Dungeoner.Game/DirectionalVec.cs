
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dungeoner;
using Godot;

public struct DirectionalVec {
    private static Vector2I s_xUnit = new(1, 0);
    private static Vector2I s_yUnit = new(0, 1);

    private static Direction[] s_backDirectionOrder = new [] {
        Direction.UpRight, Direction.Right, Direction.DownRight,
        Direction.DownLeft, Direction.Left, Direction.UpLeft
    };
    private static Direction[] s_sideDirectionOrder = new [] {
        Direction.Up, Direction.UpRight, Direction.DownRight,
        Direction.Down, Direction.DownLeft, Direction.UpLeft
    };

    public DirectionalVec(Vector2I gridPosition, Direction direction) {
        GridPosition = gridPosition;
        Direction = direction;
    }

    public Vector2I GridPosition { get; private set; }
    public Direction Direction { get; private set; }

    public override string ToString() => $"{{ GridPosition: {GridPosition}, Direction: {Direction} }}";

    public override bool Equals([NotNullWhen(true)] object? obj) {
        if(obj is DirectionalVec vec) {
            Vector2I trThis = GridPosition * 2 + Direction.Vector();
            Vector2I trOther = vec.GridPosition * 2 + vec.Direction.Vector();

            return trThis == trOther;
        }
        return false;
    }
    public override int GetHashCode() {
        Vector2I trThis = GridPosition * 2 + Direction.Vector();
        return trThis.GetHashCode();
    }
    public IEnumerable<DirectionalVec> GetNeighbors() {
        return Direction switch {
            Direction.Up => new DirectionalVec[] { 
                new(GridPosition - s_yUnit, Direction.Right), new(GridPosition + s_xUnit, Direction.Up), 
                new(GridPosition, Direction.Right), new(GridPosition, Direction.Left),
                new(GridPosition - s_xUnit, Direction.Up), new(GridPosition - s_yUnit, Direction.Left) 
            },
            Direction.Right => new DirectionalVec[] { 
                new(GridPosition - s_yUnit, Direction.Right), new(GridPosition + s_xUnit, Direction.Up), 
                new(GridPosition + s_xUnit, Direction.Down), new(GridPosition + s_yUnit, Direction.Right),
                new(GridPosition, Direction.Down), new(GridPosition, Direction.Up) 
            },
            Direction.Down => new DirectionalVec[] { 
                new(GridPosition, Direction.Right), new(GridPosition + s_xUnit, Direction.Down), 
                new(GridPosition + s_yUnit, Direction.Right), new(GridPosition + s_yUnit, Direction.Left),
                new(GridPosition - s_xUnit, Direction.Down), new(GridPosition, Direction.Left) 
            },
            Direction.Left => new DirectionalVec[] { 
                new(GridPosition - s_yUnit, Direction.Left), new(GridPosition, Direction.Up), 
                new(GridPosition, Direction.Down), new(GridPosition + s_yUnit, Direction.Left),
                new(GridPosition - s_xUnit, Direction.Down), new(GridPosition - s_xUnit, Direction.Up) 
            },
            _ => throw new ArgumentException("Edges do not have diagonal neighbors", nameof(Direction))
        };
	}
    public static bool operator==(DirectionalVec first, DirectionalVec second) => first.Equals(second);
    public static bool operator!=(DirectionalVec first, DirectionalVec second) => !first.Equals(second);

    public Direction DirectionTo(DirectionalVec other) {
        if(this == other) throw new ArgumentException($"DirectionalVecs are equal. {this} and {other}", nameof(other));
        var neighbors = GetNeighbors().ToList();

        var index = neighbors.FindIndex(dv => dv == other);
        if(index != -1) {
            return Direction.IsHorizontal() ? s_sideDirectionOrder[index] : s_backDirectionOrder[index];
        }
        throw new ArgumentException("Given DirectionalVec is not a neighbor of this one", nameof(other));
    }
}