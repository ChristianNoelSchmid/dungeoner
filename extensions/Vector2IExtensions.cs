using Dungeoner;
using Godot;

public static class Vector2IExtensions
{
    public static Vector2I ToGridPosition(this Vector2 position) => new(
        Mathf.RoundToInt(position.X / Constants.GRID_SIZE),
        Mathf.RoundToInt(position.Y / Constants.GRID_SIZE)
    );
}