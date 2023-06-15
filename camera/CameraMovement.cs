using Dungeoner.Ui;
using Godot;
using System;

public partial class CameraMovement : Camera2D
{
    [Export]
    public float Speed { get; set; } = 400;
    [Export]
    public float Accel { get; set; } = 10;
    [Export]
    public float ZoomSpeed { get; set; } = 1;
    [Export]
    private UiMain _ui = default!;

    private Vector2 _targetPosition;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _targetPosition = Position;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_ui.IsFocused) return;

        float deltaf = (float)delta;
        Vector2 axes = new(Input.GetAxis("left", "right"), Input.GetAxis("up", "down"));

        float zoom = Zoom.X;

        float zoomDelta = 0.0f;
        if (Input.IsActionJustReleased("zoom_out")) zoomDelta -= 1.0f;
        if (Input.IsActionJustReleased("zoom_in")) zoomDelta += 1.0f;
        zoomDelta *= ZoomSpeed * deltaf;

        zoom += zoomDelta;
        zoom = Mathf.Clamp(zoom, 1.0f, 6.0f);

        _targetPosition += axes * Speed * deltaf * ((6.5f - zoom) * 0.25f);

        Position = new(
            Mathf.Lerp(Position.X, _targetPosition.X, deltaf * Accel),
            Mathf.Lerp(Position.Y, _targetPosition.Y, deltaf * Accel)
        );

        Zoom = new(zoom, zoom);
    }
}
