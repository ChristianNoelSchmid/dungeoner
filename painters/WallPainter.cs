using Godot;

using Dungeoner.Importers;
using Dungeoner.Ui;
using System.Linq;
using Dungeoner.Collections;
using System.Collections;
using Dungeoner.GodotExtensions;

namespace Dungeoner.Maps;

public partial class WallPainter : Node2D {
	[Export]
	private UiCanvas _ui;
	[Export]
	private WallImporter _importer = default!;
	[Export]
	private Sprite2D _cursor = default!;
	[Export]
	private TileMap _tileMap = default!;
	[Export]
	private WallMap _worldWallMap = default!;
	[Export]
	private WallMap _commitMap = default!;

	private WallMeta _selectedWall = default!;	
	private Vector2I? _mouseDragStart = null;

	private bool _activated;
	public bool Activated {
		get => _activated;
		set {
			_activated = value;
			_cursor.Visible = value;
		}
	}

	public void UpdateWallTexture(WallMeta wall) {
		_selectedWall = wall;
	}

	public override void _Ready() {
		UpdateWallTexture(_importer.GetAllMatchingMetas("simple/dungeon").First());
		Activated = false;
	}

	public override void _Process(double delta) {
		if(!_activated) return;
		var mouseGridPosition = (GetGlobalMousePosition() + Constants.GRID_VECTOR / 2).ToGridPosition();
		_cursor.Position = mouseGridPosition * Constants.GRID_SIZE - Constants.GRID_VECTOR / 2;
		
		if(Input.IsActionJustReleased("select")) {
			_commitMap.CopyTo(_worldWallMap);
			_mouseDragStart = null;
		}
		_commitMap.Clear();

		if(Input.IsActionJustPressed("select") && !_ui.UiFocused) {
			_mouseDragStart = mouseGridPosition;
		}
		if(Input.IsActionPressed("select") && !_ui.UiFocused && _mouseDragStart != null) {
			Vector2I diff = mouseGridPosition - _mouseDragStart!.Value;
			if(Mathf.Abs(diff.X) > Mathf.Abs(diff.Y)) diff.Y = 0;
			else diff.X = 0;
			Vector2I normalized = new(Mathf.Sign(diff.X), Mathf.Sign(diff.Y));

			var target = _mouseDragStart.Value + diff;
			if(normalized.X < 0) target -= new Vector2I(1, 0);
			if(normalized.Y < 0) target -= new Vector2I(0, 1);

			var currentGridPosition = _mouseDragStart.Value;
			if(normalized.X < 0) currentGridPosition -= new Vector2I(1, 0);
			if(normalized.Y < 0) currentGridPosition -= new Vector2I(0, 1);

			var direction = normalized.X != 0 ? Direction.Up : Direction.Left;

			while(currentGridPosition != target) {
				_commitMap.AddWallPanel(currentGridPosition, direction, _selectedWall);
				currentGridPosition += normalized;
			}
		}
	}
}
