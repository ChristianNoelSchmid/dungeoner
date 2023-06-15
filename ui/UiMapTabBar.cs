using Dungeoner.Maps;
using Dungeoner.Painters;
using Godot;
using System;

public partial class UiMapTabBar : Node
{
    [Export]
    private Control _tileMenu = default!;

    [Export]
    private Control _wallMenu = default!;

    private TilePainter _tilePainter = default!;
    private WallPainter _wallPainter = default!;

    public override void _Ready()
    {
        _tilePainter = (TilePainter)GetNode("/root/Main/TilePainter");
        _wallPainter = (WallPainter)GetNode("/root/Main/WallPainter");
    }

    public void OnTabChange(int idx)
    {
        switch (idx)
        {
            case 0:
                _tilePainter.Activated = true;
                _wallPainter.Activated = false;
                _tileMenu.Visible = true;
                _wallMenu.Visible = false;
                break;
            case 1:
                _tilePainter.Activated = false;
                _wallPainter.Activated = true;
                _tileMenu.Visible = false;
                _wallMenu.Visible = true;
                break;
        }
    }
}
