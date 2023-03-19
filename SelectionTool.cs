using System.Collections.Generic;
using System.Linq;
using Dungeoner;
using Dungeoner.Collections;
using Godot;

public partial class SelectionTool : Node2D
{	
	[Export]
	private Brackets _brackets;

	private HashSet<Token> _tokens = new();
	private HashSet<Token> _selectedTokens = new();

	private Heap<Token> _tokenHeap = new((a, b) => a.Position.Y.CompareTo(b.Position.Y));
	private Vector2? _mouseDragStart;
	private Vector2 _previousMousePosition;
	private ResizeDirection? _mouseOverBracket;
	private ResizeDirection? _resizeDirection;
	private TokenResizer _resizer;

	public override void _Process(double delta) {
		var mousePosition = GetGlobalMousePosition();

		// If the left mouse is pressed down...
		if(Input.IsActionJustPressed("select")) {
			// If there's a resize bracket the mouse is currently hovering over,
			// begin resizing
			if(_mouseOverBracket != null) {
				_resizeDirection = _mouseOverBracket;
				_mouseDragStart = mousePosition;
				_resizer = new(_selectedTokens, _selectedTokens.Select(t => t.Bounds).OuterBounds(), _resizeDirection.Value);
			} else {
				// If the mouse is currently over a Token...
				if(_tokenHeap.Peek(out var token)) {
					// If shift is also being pressed, add/remove the Token
					// to/from the selected collection
					if(Input.IsActionPressed("shift")) {
						if(_selectedTokens.Contains(token)) _selectedTokens.Remove(token);
						else _selectedTokens.Add(token);
					// Otherwise, if the token is not part of the collection, clear the collection
					// and add the single Token to the collection
					} else if(!_selectedTokens.Contains(token)) {
						_selectedTokens.Clear();
						_selectedTokens.Add(token);
					}
				// If the mouse is currently not over any Token, clear the selected token collection
				// and store the mouse position as drag start
				} else {
					_selectedTokens.Clear();
					_mouseDragStart = mousePosition;
				}	
			}
		}
		// If the left mouse is released, and there is currently a select box being dragged...
		if(Input.IsActionJustReleased("select") && _mouseDragStart != null) {
			if(_resizeDirection != null) {
				_resizeDirection = null;
				_resizer = null;
			} else {
				// Create bounds over the rect and select any Tokens that lie within those bounds
				Rect2 dragBounds = new Rect2(_mouseDragStart.Value, mousePosition - _mouseDragStart.Value).Abs();
				foreach(var token in _tokens) {
					if(dragBounds.Encloses(token.Bounds)) _selectedTokens.Add(token);
				}
				// Then clear the select box
			}
			_mouseDragStart = null;
		}

		// If select is being pressed
		if(Input.IsActionPressed("select")) {
			// Determine if there is a resize bracket being pulled.
			// If so, update the resizer
			if(_resizeDirection != null) {
				Rect2 bounds = _selectedTokens.Select(t => t.Bounds).OuterBounds();
				Rect2 newRect = _resizeDirection switch {
					ResizeDirection.Top => new(bounds.Position.X, mousePosition.Y, bounds.Size.X, bounds.End.Y - mousePosition.Y),
					ResizeDirection.TopRight => new(bounds.Position.X, mousePosition.Y, mousePosition.X - bounds.Position.X, bounds.End.Y - mousePosition.Y),
					ResizeDirection.Right => new(bounds.Position.X, bounds.Position.Y, mousePosition.X - bounds.Position.X, bounds.Size.Y),
					ResizeDirection.BottomRight => new(bounds.Position.X, bounds.Position.Y, mousePosition.X - bounds.Position.X, mousePosition.Y - bounds.Position.Y),
					ResizeDirection.Bottom => new(bounds.Position.X, bounds.Position.Y, bounds.Size.X, mousePosition.Y - bounds.Position.Y),
					ResizeDirection.BottomLeft => new(mousePosition.X, bounds.Position.Y, bounds.End.X - mousePosition.X, mousePosition.Y - bounds.Position.Y),
					ResizeDirection.Left => new(mousePosition.X, bounds.Position.Y, bounds.End.X - mousePosition.X, bounds.Size.Y),
					_ => new(mousePosition.X, mousePosition.Y, bounds.End.X - mousePosition.X, bounds.End.Y - mousePosition.Y)
				};
				_resizer.Resize(newRect, Input.IsActionPressed("shift"));
			// Otherwise, move all selected tokens based on delta mouse movement
			} else {
				foreach(var token in _selectedTokens) {
					token.Position -= _previousMousePosition - mousePosition;
				}
			}
		}

		// If there are selected tokens, update the resize brackets to surround them
		if(_selectedTokens.Count != 0) {
			// Show the brackets if they're currently invisible
			if(!_brackets.IsProcessing()) {
				_brackets.Visible = true;
				_brackets.SetProcess(true);
			}

			var bound = _selectedTokens.Select(t => t.Bounds).OuterBounds();
			_brackets.SetRect(bound);
		// Otherwise, make them 
		} else {
			_brackets.Visible = false;
			_brackets.SetProcess(false);
		}

		_previousMousePosition = mousePosition;
		QueueRedraw();
	}

    public override void _Draw() {
		if(_mouseDragStart != null && _selectedTokens.Count == 0) {
			Rect2 dragBounds = new Rect2(_mouseDragStart.Value, GetGlobalMousePosition() - _mouseDragStart.Value).Abs();
			DrawRect(dragBounds, new Color(1.0f, 1.0f, 1.0f, 0.25f), filled: true);
			foreach(var token in _tokens) {
				if(dragBounds.Encloses(token.Bounds)) {
					DrawRect(token.Bounds, new Color(0.75f, 0.75f, 0.75f), filled: false, width: -1);
				}
			}
		}
		foreach(var token in _selectedTokens) {
			DrawRect(token.Bounds, new Color(0.75f, 0.75f, 0.75f), filled: false, width: -1);
		}
    }

	public void RegisterToken(Token token) {
		_tokens.Add(token);
		token.MouseEnter += OnMouseEnterToken;
		token.MouseExit += OnMouseExitToken;
	}
	public void RemoveToken(Token token) => _tokens.Remove(token);
	public void OnMouseEnterToken(Token token) => _tokenHeap.Push(token);
	public void OnMouseExitToken(Token token) =>_tokenHeap.Remove(token);
	public void OnAnchorEntered(int anchorIdx) => _mouseOverBracket = (ResizeDirection)anchorIdx;
	public void OnAnchorExited(int anchorIdx) => _mouseOverBracket = null;
}
