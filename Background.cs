using Godot;
using System;

public partial class Background : ColorRect
{
    [Export]
    private Camera2D _mainCamera = default!;
    public override void _Process(double delta) => Position = _mainCamera.Position - GetRect().Size / 2;
}
