using Dungeoner.Importers;
using Dungeoner.Maps;
using Dungeoner.Ui;
using Godot;

namespace Dungeoner.Painters;

public partial class TilePainter : Node2D
{
	[Export]
	private UiCanvas _ui = default!;
	[Export]
	private TileImporter _importer = default!;
	[Export]
	Sprite2D _cursorSprite = default!;
	[Export]
	TokenMap _world = default!;
	[Export]
    Maps.TileMap _tileMap = default!;

	private Vector2 _halfShadowSize;
	private Vector2 _halfGridSize;

	private Tile? _selectedTile;

	private Node2D _floorNode = new();

	private bool _activated = true;
	public bool Activated { 
		get => _activated;
		set {
			_activated = value;
			_cursorSprite.Visible = value;
		}
	}

	public void UpdateTileTexture(Tile tile) {
		_selectedTile = tile;
		_cursorSprite.Texture = tile.Texture;
	}

	public override void _Ready() {
		_halfGridSize = new Vector2(Constants.GRID_SIZE / 2f, Constants.GRID_SIZE / 2f);
		AddChild(_floorNode);
	}

	public override void _Process(double delta) {
		if(!Activated) {
			return;
		}
		var mouseGridPosition = GetGlobalMousePosition().ToGridPosition();
		if(Input.IsActionPressed("select") && !_ui.UiFocused && _selectedTile != null) {
			_tileMap[mouseGridPosition] = _selectedTile;
		}

		_cursorSprite.Position = mouseGridPosition * Constants.GRID_SIZE;
	}
}
