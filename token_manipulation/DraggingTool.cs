using Godot;

namespace Dungeoner.TokenManipulation;

public partial class DraggingTool : Node2D
{
	[Export]
	private SelectionTool _selectionTool;

	private Vector2 _previousMousePosition;
	public bool IsDragging { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var mousePosition = GetGlobalMousePosition();
		if(IsDragging) {
			foreach(var token in _selectionTool.SelectedTokens) {
				token.Position -= _previousMousePosition - mousePosition;
			}
		}
		_previousMousePosition = mousePosition;
	}
}
