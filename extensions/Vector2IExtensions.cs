using System;
using Dungeoner;
using Godot;

public static class Vector2IExtensions
{
    public static Vector2I ToGridPosition(this Vector2 position) => new(
        Mathf.RoundToInt(position.X / Constants.GRID_SIZE),
        Mathf.RoundToInt(position.Y / Constants.GRID_SIZE)
    );

    public static Vector2 ToHalfGridPosition(this Vector2 position) {
        Vector2 offset = position - (Vector2I)position / Constants.GRID_SIZE * Constants.GRID_SIZE;
        position -= offset;

        position.X += Constants.GRID_SIZE / 2 * Mathf.Sign(offset.X - Constants.GRID_SIZE / 2);
        position.Y += Constants.GRID_SIZE / 2 * Mathf.Sign(offset.Y - Constants.GRID_SIZE / 2);
        return new Vector2(
            (float)Math.Round(position.X / Constants.GRID_SIZE * 2, 1) / 2.0f,
            (float)Math.Round(position.Y / Constants.GRID_SIZE * 2, 1) / 2.0f
        );
    }
}