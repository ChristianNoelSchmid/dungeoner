using Dungeoner.Maps;
using Godot;
using System;

public partial class UiWallMenuFillChoice : ItemList
{
    [Export]
    private WallPainter _painter = default!;
    private bool _mouseOver = false;
    public override void _Ready()
    {
        _painter = (WallPainter)GetNode("/root/Main/WallPainter");
        Select(0, true);
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("select") && !_mouseOver) ReleaseFocus();
    }

    public void OnChoiceChange(int idx) => _painter.FillType = idx == 0 ? WallFillType.Side : WallFillType.Room;

    public void OnMouseEnter() => _mouseOver = true;
    public void OnMouseExit() => _mouseOver = false;
}
