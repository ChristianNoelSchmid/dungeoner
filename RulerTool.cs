using System;
using System.Collections.Generic;
using Godot;

namespace Dungeoner;

public partial class RulerTool : Node2D
{
	[Export]
	private NetworkManager _netManager = default!;
	[Export]
	private UserMap _userMap = default!;

	private readonly Dictionary<Guid, (Vector2, Vector2)> _clientRulers = new();

	private Vector2? _start, _end;
	private double _timer;
	public void Start(Vector2 start) => _start = start;
	public void Stop() => _start = null;

	public bool IsActive { get; set; } = false;
	/// <summary>
	/// Toggles whether the RulerTool is visible to all clients
	/// </summary>
	public bool IsNetworkVisible { get; set; } = true;

	public override void _Draw()
	{
		if(_start != null && IsActive) 
		{
			bool shiftDown = Input.IsActionPressed("shift");
			var mousePosition = GetGlobalMousePosition();

			_start = shiftDown ? 
				_start.Value.ToHalfGridPosition() * Constants.GRID_SIZE :
				_start.Value.ToGridPosition() * Constants.GRID_SIZE;

			_end = shiftDown ? 
				mousePosition.ToHalfGridPosition() * Constants.GRID_SIZE :
				mousePosition.ToGridPosition() * Constants.GRID_SIZE;

			DrawRulerLine(_start.Value, _end.Value);	
		}

		foreach((var start, var end) in _clientRulers.Values) {
			DrawRulerLine(start, end);
		}
	}

	public override void _Process(double delta) 
	{
		QueueRedraw();
		if(IsActive) 
		{
			if(Input.IsActionJustPressed("select")) 
				Start(GetLocalMousePosition());
			if(Input.IsActionJustReleased("select"))
			{
				Stop();
				_netManager.SendToAll(new RulerOffModel(_userMap.ClientId), false);
			}

			if(Input.IsActionPressed("select")) 
			{
				_timer += delta;
				if(_timer > 0.1 && IsNetworkVisible)
				{
					var rulerOnModel = new RulerOnModel(
						_userMap.ClientId, 
						(_start!.Value.X, _start!.Value.Y), 
						(_end!.Value.X, _end!.Value.Y)
					);
					_netManager.SendToAll(rulerOnModel, false);
					_timer = 0.0;
				}
			}
			else _timer = 0.0;
		}
	}

	public void AddOrUpdate(Guid id, Vector2 start, Vector2 end) => _clientRulers[id] = (start, end);
	public void Remove(Guid id) => _clientRulers.Remove(id);

	private void DrawRulerLine(Vector2 start, Vector2 end) {
		DrawLine(start, end, new Color(1f, 1f, 1f));

        int dx = (int)(Math.Abs(end.X - start.X) / Constants.GRID_SIZE);
		int dy = (int)(Math.Abs(end.Y - start.Y) / Constants.GRID_SIZE);

		int diagonalSteps = Mathf.Min(dx, dy);
		int remainingSteps = Mathf.Abs(dx - dy);

		int distance = (diagonalSteps + remainingSteps) * 5;
		DrawString(ThemeDB.FallbackFont, end, $"{distance}ft", fontSize: 12);
    }
}

public enum RulerPosition 
{
	Exact,
	Middle,
	Corner
}