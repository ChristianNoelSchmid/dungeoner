
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Dungeoner.GodotExtensions;

public static class Vector2Extensions
{
    private static readonly IReadOnlyDictionary<Vector2I, Direction> s_directionMatches
        = new Dictionary<Vector2I, Direction>() {
            { new( 0, -1), Direction.Up },
            { new( 1, -1), Direction.UpRight },
            { new( 1,  0), Direction.Right },
            { new( 1,  1), Direction.DownRight },
            { new( 0,  1), Direction.Down },
            { new(-1,  1), Direction.DownLeft },
            { new(-1,  0), Direction.Left },
            { new(-1, -1), Direction.UpLeft }
        };

    public static Direction? DirectionTo(this Vector2I vector, Vector2I other)
    {
        var distSquared = (vector.X - other.X) * (vector.X - other.X) + (vector.Y - other.Y) * (vector.Y - other.Y);
        if (distSquared > 2) return null;

        return s_directionMatches[other - vector];
    }
    public static Vector2I GetNeighbor(this Vector2I vector, Direction to)
    {
        return to switch
        {
            Direction.Up => new(vector.X, vector.Y - 1),
            Direction.UpRight => new(vector.X + 1, vector.Y - 1),
            Direction.Right => new(vector.X + 1, vector.Y),
            Direction.DownRight => new(vector.X + 1, vector.Y + 1),
            Direction.Down => new(vector.X, vector.Y + 1),
            Direction.DownLeft => new(vector.X - 1, vector.Y + 1),
            Direction.Left => new(vector.X - 1, vector.Y),
            Direction.UpLeft => new(vector.X - 1, vector.Y - 1),
            _ => throw new ArgumentException("Cannot provide Count Direction value.", nameof(to))
        };
    }

    public static HashSet<Vector2I> GetNeighbors(this Vector2I vector)
    {
        return new HashSet<Vector2I>(
            new Vector2I[] {
                new(vector.X,     vector.Y - 1),
                new(vector.X + 1, vector.Y - 1),
                new(vector.X + 1, vector.Y),
                new(vector.X + 1, vector.Y + 1),
                new(vector.X,     vector.Y + 1),
                new(vector.X - 1, vector.Y + 1),
                new(vector.X - 1, vector.Y),
                new(vector.X - 1, vector.Y - 1),
            }
        );
    }
}