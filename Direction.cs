using System;
using System.Collections.Generic;
using Godot;

namespace Dungeoner;

public enum Direction
{
    Up = 0, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft, Count
}

public static class DirectionExtensions
{
    public static bool IsHorizontal(this Direction dir) => dir == Direction.Left || dir == Direction.Right;
    public static bool IsVertical(this Direction dir) => dir == Direction.Up || dir == Direction.Down;
    public static Direction Reverse(this Direction dir) => (Direction)(((int)dir + (int)Direction.Count / 2) % (int)Direction.Count);
    public static Vector2I Vector(this Direction dir) => dir switch
    {
        Direction.Up => new(0, -1),
        Direction.UpRight => new(1, -1),
        Direction.Right => new(1, 0),
        Direction.DownRight => new(1, 1),
        Direction.Down => new(0, 1),
        Direction.DownLeft => new(-1, 1),
        Direction.Left => new(-1, 0),
        Direction.UpLeft => new(-1, -1),
        _ => throw new ArgumentException("Cannot find vector of Count", nameof(dir))
    };
}