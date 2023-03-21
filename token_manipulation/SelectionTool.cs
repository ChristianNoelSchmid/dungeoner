using System;
using System.Collections.Generic;
using System.Linq;
using Dungeoner.Collections;
using Dungeoner.Ui;
using Godot;

namespace Dungeoner.TokenManipulation;

public partial class SelectionTool : Node2D
{	
	[Export]
	private World _world;
	[Export]
	private UiCanvas _uiCanvas;
	[Export]
	private DraggingTool _draggingTool;
	[Export]
	private ResizingTool _resizingTool;

	private HashSet<Token> _tokens = new();
	private HashSet<Token> _selectedTokens = new();
	public IEnumerable<Token> SelectedTokens => _selectedTokens;
	private Heap<Token> _tokenHeap = new((a, b) => a.Position.Y.CompareTo(b.Position.Y));
	private Vector2? _mouseDragStart;
	private ResizeDirection? _resizeDirection;

	public override void _Process(double delta) {
		var mousePosition = GetGlobalMousePosition();

		// If the left mouse is pressed down...
		if(Input.IsActionJustPressed("select")) {
			if(_uiCanvas.UiFocused) {
				_selectedTokens.Clear();
				return;
			} 

			// If there's a resize bracket the mouse is currently hovering over,
			// begin resizing
			if(_resizeDirection != null) {
				_mouseDragStart = mousePosition;
				_resizingTool.StartResizing(
					_selectedTokens, 
					_selectedTokens.Select(t => t.Bounds).OuterBounds(), 
					_resizeDirection.Value
				);
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

					// Activate the DraggingTool, to begin dragging the selected sprites
					_draggingTool.IsDragging = true;

				// If the mouse is currently not over any Token, clear the selected token collection
				// and store the mouse position as drag start
				} else {
					if(!Input.IsActionPressed("shift")) _selectedTokens.Clear();
					_mouseDragStart = mousePosition;
				}	
			}
		}
		// If the left mouse is released, and there is currently a select box being dragged...
		if(Input.IsActionJustReleased("select")) {
			_draggingTool.IsDragging = false;
			_resizingTool.StopResizing();

			if(_mouseDragStart != null) {
				// Create bounds over the rect and select any Tokens that lie within those bounds
				Rect2 dragBounds = new Rect2(_mouseDragStart.Value, mousePosition - _mouseDragStart.Value).Abs();
				foreach(var token in _tokens) {
					if(dragBounds.Encloses(token.Bounds)) _selectedTokens.Add(token);
				}
			}
			// Then clear the select box
			_mouseDragStart = null;
		}
		if(Input.IsActionJustPressed("delete")) {
			foreach(var token in _selectedTokens) {
				RemoveToken(token);
				_world.RemoveToken(token);	
			}
		}

		QueueRedraw();
	}

    public override void _Draw() {
		if(_mouseDragStart != null && !_resizingTool.IsResizing) {
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

	public void SelectTokens(IEnumerable<Token> tokens) {
		_selectedTokens.Clear();
		foreach(var token in tokens) _selectedTokens.Add(token);
		_draggingTool.IsDragging = Input.IsActionPressed("select");
		_resizingTool.StopResizing();
	}
	public void RegisterToken(Token token) {
		_tokens.Add(token);
		token.MouseEnter += OnMouseEnterToken;
		token.MouseExit += OnMouseExitToken;
	}
	public void RemoveToken(Token token) {
		_tokens.Remove(token);
		_selectedTokens.Remove(token);
	}
	public void OnMouseEnterToken(Token token) => _tokenHeap.Push(token);
	public void OnMouseExitToken(Token token) =>_tokenHeap.Remove(token);
	public void OnResizeBoxEnter(int anchorIdx) => _resizeDirection = (ResizeDirection)anchorIdx;
	public void OnResizeBoxExit(int anchorIdx) => _resizeDirection = null;
}
