using System;

namespace Dungeoner;

public enum Direction {
    Up = 0, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft, Count
}

public static class DirectionExtensions {
    public static Direction Reverse(this Direction dir) => (Direction)(((int)dir + (int)Direction.Count / 2) % (int)Direction.Count);
}