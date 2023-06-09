using System.Collections.Generic;
using System.Linq;
using Dungeoner.Importers;
using Dungeoner.Painters;
using Godot;

public partial class UiTileList : UiList
{
    private TileImporter _importer;
    private TilePainter _painter;

    private bool _mouseOver = false;
    List<TileInstance> _listTileMetas = new();

    public override void _Ready()
    {
        _importer = (TileImporter)GetNode("/root/Main/Importers/TileImporter");
        _painter = (TilePainter)GetNode("/root/Main/TilePainter");
    }

    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.Escape)) { ReleaseFocus(); }
        if (Input.IsActionJustPressed("select") && !_mouseOver) ReleaseFocus();
    }
    public override void QueryList(params string[] globs)
    {
        Clear();
        _listTileMetas.Clear();

        List<TileInstance> metas = new();
        foreach (var glob in globs)
        {
            metas.AddRange(_importer.GetAllMatchingMetas(glob));
        }
        foreach (var tile in metas.Distinct())
        {
            _listTileMetas.Add(tile);
            AddIconItem(tile.GetTexture(new(0, 0)));
        }
    }
    public void OnItemSelected(int index)
        => _painter.UpdateTileTexture(_listTileMetas[index]);
}
