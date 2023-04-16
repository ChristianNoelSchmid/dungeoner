using Godot;

using Dungeoner.Importers;
using Dungeoner.Ui;
using System.Linq;
using Dungeoner.Collections;
using System.Collections;
using Dungeoner.GodotExtensions;
using System.Collections.Generic;

namespace Dungeoner.Maps;

public partial class WallPainter : Node2D {
	[Export]
	private UiCanvas _ui = default!;
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
	
    public WallFillType FillType { get; set; } = WallFillType.Side;

	private WallMeta _selectedWall = default!;	
	private Vector2I? _mouseDragStart = null;
	private bool _isDeleting = false;
	private WallMeta _deleteWall = new() {
		FilePath = "./assets/delete_wall.png",
		BackPanelRects = new List<Rect2> { new(0, 0, 27, 20) },
		DrywallColor = new Color(1.0f, 0.0f, 0.0f, 0.3f)
	};

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
		if(Input.IsActionJustReleased("right-click")) {
			_worldWallMap.RemoveAll(_commitMap);
			_mouseDragStart = null;
		}
		_commitMap.Clear();

		if(Input.IsActionJustPressed("select") && !_ui.UiFocused) {
			_mouseDragStart = mouseGridPosition;
			_isDeleting = false;
		}
		if(Input.IsActionJustPressed("right-click") && !_ui.UiFocused) {
			_mouseDragStart = mouseGridPosition;
			_isDeleting = true;
		}

		if(
			(Input.IsActionPressed("select") || Input.IsActionPressed("right-click")) 
			&& !_ui.UiFocused && _mouseDragStart != null
		) {
			if(FillType == WallFillType.Side) {
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
					var meta = _isDeleting ? _deleteWall : _selectedWall;
					_commitMap.AddWallPanel(currentGridPosition, direction, meta);
					currentGridPosition += normalized;
				}
			} else {
				var border = new Rect2(_mouseDragStart.Value, mouseGridPosition - _mouseDragStart.Value).Abs();
				var meta = _isDeleting ? _deleteWall : _selectedWall;

				for(int x = (int)border.Position.X; x <= (int)border.End.X - 1; x += 1) {
					_commitMap.AddWallPanel(new(x, (int)border.Position.Y), Direction.Up, meta);
					_commitMap.AddWallPanel(new(x, (int)border.End.Y), Direction.Up, meta);
				}
				for(int y = (int)border.Position.Y; y <= (int)border.End.Y - 1; y += 1) {
					_commitMap.AddWallPanel(new((int)border.Position.X, y), Direction.Left, meta);
					_commitMap.AddWallPanel(new((int)border.End.X - 1, y), Direction.Right, meta);
				}
			}
		}
	}
}
