using System.Collections.Generic;
using System.Linq;
using Dungeoner.GodotExtensions;
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
	private TokenMap _world = default!;
	[Export]
    private Maps.TileMap _worldTileMap = default!;
	[Export]
	private Maps.TileMap _commitTileMap = default!;
	[Export]
	private WallMap _worldWallMap = default!;

	public TileFillType FillType { get; set; } = TileFillType.Single;

	private Vector2 _halfShadowSize;
	private Vector2 _halfGridSize;

	private Tile? _selectedTile;

	private Node2D _floorNode = new();

	private bool _activated = false;
	public bool Activated { 
		get => _activated;
		set {
			_activated = value;
		}
	}

	public void UpdateTileTexture(Tile tile) {
		_selectedTile = tile;
	}

	public override void _Ready() {
		_halfGridSize = new Vector2(Constants.GRID_SIZE / 2f, Constants.GRID_SIZE / 2f);
		AddChild(_floorNode);
	}

	public override void _Process(double delta) {
		var mouseGridPosition = GetGlobalMousePosition().ToGridPosition();
		if(Input.IsActionPressed("select") && !_ui.UiFocused && _selectedTile != null && Activated) {
			if(FillType == TileFillType.Single) {
				_worldTileMap[mouseGridPosition] = _selectedTile;
			} else {
				foreach((var position, var tile) in _commitTileMap) {
					_worldTileMap[position]	= tile;
				}
			}
		}

		_commitTileMap.Clear();

		if(!Activated) return;

		if(_selectedTile != null) {
			if(FillType == TileFillType.Single) {
				_commitTileMap[mouseGridPosition] = _selectedTile;
			} else {
				HashSet<Vector2I> visited = new();
				Queue<Vector2I> next = new();
				var directions = new [] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };

				int count = 0;	
				_worldTileMap.TryGetTile(mouseGridPosition, out var target);

				next.Enqueue(mouseGridPosition);
				while(next.Any()) {
					var from = next.Dequeue();
					foreach(var to in directions.Select(dir => from.GetNeighbor(dir))) {
						if(visited.Contains(to)) continue;
						visited.Add(to);
						if(!_worldWallMap.TryGetEdge(new(from, from.DirectionTo(to)!.Value), out _)) {
							_worldTileMap.TryGetTile(to, out var test);
							if(test == target) {
								_commitTileMap[to] = _selectedTile;
								next.Enqueue(to);
							}
						}
					}

					count += 1;
					if(count == 1000) {
						_commitTileMap.Clear();
						break;
					}
				}
			}
		}
	}
}
