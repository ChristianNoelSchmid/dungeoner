using System.Collections.Generic;
using Godot;

namespace Dungeoner.TokenManipulation;

public partial class DraggingTool : Node2D
{
	[Export]
	private SelectionTool _selectionTool;

	private Dictionary<Token, Vector2> _offsets = new();

	private bool _isDragging = false;
	public bool IsDragging { 
		get => _isDragging;
		set {
			_isDragging = value;
			var mousePosition = GetGlobalMousePosition();
			if(_isDragging) {
				_offsets.Clear();
				foreach(var token in _selectionTool.SelectedTokens) {
					_offsets.Add(token, token.Position - mousePosition);
				}
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		var mousePosition = GetGlobalMousePosition();
		if(IsDragging) {
			foreach(var token in _selectionTool.SelectedTokens) {
				token.Position = _offsets[token] + mousePosition;
				if(!Input.IsActionPressed("alt")) {
					var gridPosition = new Vector2(
						Mathf.Round(token.Position.X / Constants.GRID_SIZE),
						Mathf.Round(token.Position.Y / Constants.GRID_SIZE)
					);
					token.Position = new(
						gridPosition.X * Constants.GRID_SIZE,
						gridPosition.Y * Constants.GRID_SIZE
					);
				}
			}
		}
	}
}
