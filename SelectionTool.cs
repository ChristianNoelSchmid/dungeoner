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
	private AnchorPosition? _anchorHovering;
	private AnchorPosition? _dragAnchor;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		var mousePosition = GetGlobalMousePosition();

		// If the left mouse is pressed down...
		if(Input.IsActionJustPressed("select")) {
			if(_anchorHovering != null) {
				GD.Print($"Begin resizing {_anchorHovering}");
				_dragAnchor = _anchorHovering;
				_mouseDragStart = mousePosition;
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
			if(_dragAnchor != null) {
				_dragAnchor = null;
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

		// If select is being pressed and there are selected tokens, move the 
		// selected tokens with the mouse	
		if(Input.IsActionPressed("select")) {
			if(_dragAnchor == null) {
				foreach(var token in _selectedTokens) {
					token.Position -= _previousMousePosition - mousePosition;
				}
			} else {

			}
		}

		// If there are selected tokens, set up the brackets to box around them
		// Otherwise, make them invisible
		if(_selectedTokens.Count != 0) {
			_brackets.Visible = true;
			var bound = _selectedTokens.Select(t => t.Bounds).OuterBounds();
			_brackets.SetRect(bound);
		} else {
			_brackets.Visible = false;
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
	public void OnMouseEnterToken(Token token) {
		_tokenHeap.Push(token);
	}
	public void OnMouseExitToken(Token token) {
		_tokenHeap.Remove(token);
	}
	public void OnAnchorEntered(int anchorIdx) {
		_anchorHovering = (AnchorPosition)anchorIdx;
	}
	public void OnAnchorExited(int anchorIdx) {
		_anchorHovering = null;
	}
}
