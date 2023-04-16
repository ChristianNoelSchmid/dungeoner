using System.Collections.Generic;
using Godot;

namespace Dungeoner.TokenManipulation;

public partial class DraggingTool : Node2D
{
	[Export]
	private SelectionTool _selectionTool = default!;

	private Dictionary<Token, Sprite2D> _dragSpriteMapping = new();
	private Dictionary<Sprite2D, Vector2> _offsets = new();

	private bool _isDragging = false;
	public bool IsDragging { 
		get => _isDragging;
		set {
			_isDragging = value;
			var mousePosition = GetGlobalMousePosition();
			if(_isDragging) {
				_dragSpriteMapping.Clear();
				_offsets.Clear();

				foreach(var token in _selectionTool.SelectedTokens) {
					var tokenSprite = token.GetChild<Sprite2D>(0);
					var sprite = new Sprite2D() { 
						Texture = tokenSprite.Texture,
						Offset = tokenSprite.Offset,
						Centered = false,
						GlobalPosition = token.GlobalPosition,
						Modulate = new Color(2.0f, 2.0f, 2.0f, 0.75f)
					};
					_dragSpriteMapping[token] = sprite;
					_offsets.Add(sprite, token.Position - mousePosition);

					AddChild(sprite);
				}
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		var mousePosition = GetGlobalMousePosition();
		if(IsDragging) {
			foreach(var token in _selectionTool.SelectedTokens) {
				var mappedSprite = _dragSpriteMapping[token];
				mappedSprite.GlobalPosition = _offsets[mappedSprite] + mousePosition;

				if(!Input.IsActionPressed("alt")) {
					var gridPosition = new Vector2(
						Mathf.Round(mappedSprite.GlobalPosition.X / Constants.GRID_SIZE),
						Mathf.Round(mappedSprite.GlobalPosition.Y / Constants.GRID_SIZE)
					);
					mappedSprite.GlobalPosition = new(
						gridPosition.X * Constants.GRID_SIZE,
						gridPosition.Y * Constants.GRID_SIZE
					);
				}
			}
		}
	}

	public void CommitPositions() {
		foreach(var token in _selectionTool.SelectedTokens) {
			var mappedSprite = _dragSpriteMapping[token];
			token.MapPosition = mappedSprite.GlobalPosition;
			RemoveChild(mappedSprite);
		}
	}
}
