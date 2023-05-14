using Dungeoner;
using Godot;

public partial class Grid : Node2D
{
    [Export]
    private Camera2D _mainCamera;
    private Vector2 _prevCameraPosition;

    public override void _Process(double delta)
    {
        if (_mainCamera.Position != _prevCameraPosition)
        {
            _prevCameraPosition = _mainCamera.Position;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        Vector2I cameraGridPosition = _mainCamera.Position.ToGridPosition();

        int startX = cameraGridPosition.X - 80;
        int startY = cameraGridPosition.Y - 80;
        int length = Constants.GRID_SIZE * 160;

        Vector2 offset = (Vector2)Constants.GRID_VECTOR / 2.0f;

        for (int x = cameraGridPosition.X - 80; x <= cameraGridPosition.X + 80; x += 1)
        {
            DrawLine(
                new Vector2(x * Constants.GRID_SIZE, startY * Constants.GRID_SIZE) - offset,
                new Vector2(x * Constants.GRID_SIZE, startY + length) - offset,
                new Color(1.0f, 1.0f, 1.0f, 0.25f)
            );
        }
        for (int y = cameraGridPosition.Y - 80; y <= cameraGridPosition.Y + 80; y += 1)
        {
            DrawLine(
                new Vector2(startX * Constants.GRID_SIZE, y * Constants.GRID_SIZE) - offset,
                new Vector2(startX + length, y * Constants.GRID_SIZE) - offset,
                new Color(1.0f, 1.0f, 1.0f, 0.25f)
            );
        }
    }
}
