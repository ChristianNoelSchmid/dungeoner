using Godot;

using Dungeoner.Importers;
using Dungeoner.Ui;
using System.Linq;
using Dungeoner.Collections;
using System.Collections;

namespace Dungeoner.Maps;

public partial class WallPainter : Node2D {
	[Export]
	private UiCanvas _ui;
	[Export]
	private WallImporter _importer = default!;
	[Export]
	private Sprite2D _cursor = default!;
	[Export]
	private TokenMap _world = default!;
	[Export]
	private TileMap _tileGenerator = default!;

	private WallMeta _selectedWall = default!;	

	private WallMap _commitGenerator = default!;

	private Vector2I? _mouseDragStart = null;

	private bool _isActivated = true;
	public bool IsActivated {
		get => _isActivated;
		set {
			_isActivated = value;
			_cursor.SetProcess(value);
		}
	}

	public void UpdateWallTexture(WallMeta wall) {
		_selectedWall = wall;
	}

	public override void _Ready() {
		_commitGenerator = new(_world, _tileGenerator);
		UpdateWallTexture(_importer.GetAllMatchingMetas("simple/dungeon").First());
		AddChild(_commitGenerator);
	}

	public override void _Process(double delta) {
		var mouseGridPosition = GetGlobalMousePosition().ToGridPosition();
		_cursor.Position = mouseGridPosition * Constants.GRID_SIZE - new Vector2(Constants.GRID_SIZE / 2, Constants.GRID_SIZE / 2);
		
		if(Input.IsActionJustReleased("select")) {
		}

		_commitGenerator.Clear();

		if(Input.IsActionJustPressed("select") && !_ui.UiFocused) {
			_mouseDragStart = mouseGridPosition;
		}
		if(Input.IsActionPressed("select")) {
			Vector2I diff = mouseGridPosition - _mouseDragStart!.Value;
			if(Mathf.Abs(diff.X) > Mathf.Abs(diff.Y)) diff.Y = 0;
			else diff.X = 0;

			var target = _mouseDragStart.Value + diff;
			Vector2I normalized = new(Mathf.Sign(diff.X), Mathf.Sign(diff.Y));
			var currentGridPosition = _mouseDragStart.Value;

			while(currentGridPosition != target) {
				_commitGenerator.AddWallPanel(currentGridPosition, currentGridPosition + normalized, _selectedWall);
				currentGridPosition += normalized;
			}
		}
	}
}
