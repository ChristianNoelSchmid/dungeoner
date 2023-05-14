using Dungeoner.Maps;
using Dungeoner.Painters;
using Dungeoner.TokenManipulation;
using Godot;
using System;

public partial class UiMainTabBar : Godot.TabBar
{
    [Export]
    private Control _tokenMenu = default!;
    [Export]
    private Control _mapMenu = default!;

    private SelectionTool _selectionTool = default!;
    private TilePainter _tilePainter = default!;
    private WallPainter _wallPainter = default!;

    public override void _Ready()
    {
        _selectionTool = (SelectionTool)GetNode("/root/Main/SelectionTool");
        _tilePainter = (TilePainter)GetNode("/root/Main/TilePainter");
        _wallPainter = (WallPainter)GetNode("/root/Main/WallPainter");
    }

    public void OnTabChange(int idx)
    {
        switch (idx)
        {
            case 0:
                _tokenMenu.Show();
                _mapMenu.Hide();
                _selectionTool.Activated = true;
                _tilePainter.Activated = false;
                _wallPainter.Activated = false;
                break;
            case 1:
                _selectionTool.Activated = false;
                _tokenMenu.Hide();
                _mapMenu.Show();
                break;
        }
    }
}
