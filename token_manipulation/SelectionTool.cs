using System.Collections.Generic;
using System.Linq;
using Dungeoner.Collections;
using Dungeoner.Maps;
using Dungeoner.Ui;
using Godot;

namespace Dungeoner.TokenManipulation;

public partial class SelectionTool : Node2D
{
    [Export] private TokenMap _tokenMap = default!;
    [Export] private UiMain _ui = default!;
    [Export] private DraggingTool _draggingTool = default!;
    [Export] private ResizingTool _resizingTool = default!;
    [Export] private NetworkManager _netManager = default!;
    [Export] private UserMap _userMap = default!;
    [Export] private PermissionsMap _permissionsMap = default!;

    private HashSet<Token> _tokens = new();
    private HashSet<Token> _selectedTokens = new();
    public IEnumerable<Token> SelectedTokens => _selectedTokens;

    public bool Activated { get; set; } = true;

    // Used to trigger specific functionality when the items selected
    // by this tool are in process of being placed for the first time
    // (ie. they were just dragged from the TokenList UI)
    public bool JustPlacing { get; set; } = false;

    private Heap<Token> _tokenHeap = new((a, b) => a.Position.Y.CompareTo(b.Position.Y));
    private Vector2? _selectBoxStart;
    private ResizeDirection? _resizeDirection;

    public override void _Process(double delta)
    {
        if (Activated)
        {
            var mousePosition = GetGlobalMousePosition();

            // If the left mouse is pressed down...
            if (Input.IsActionJustPressed("select"))
            {
                // If the UI is focused when clicked, deselect all selected tokens
                if (_ui.IsFocused)
                {
                    _selectedTokens.Clear();
                }

                // If there's a resize bracket the mouse is currently hovering over,
                // begin resizing
                else if (_resizeDirection != null)
                {
                    _selectBoxStart = mousePosition;
                    _resizingTool.StartResizing(
                        _selectedTokens,
                        _selectedTokens.Select(t => t.Bounds).OuterBounds(),
                        _resizeDirection.Value
                    );
                }
                
                // If the mouse is currently over a Token, handle
                // its selection
                else if (_tokenHeap.Peek(out var token))
                {
                    Token? selectedToken = null;

                    // If shift is also being pressed, add/remove the Token
                    // to/from the selected collection
                    if (Input.IsActionPressed("shift"))
                    {
                        if (_selectedTokens.Contains(token)) _selectedTokens.Remove(token);
                        else selectedToken = token;
                    }
                    // Otherwise, if the token is not part of the collection, clear the collection
                    // and add the single Token to the collection
                    else if (!_selectedTokens.Contains(token))
                    {
                        _selectedTokens.Clear();
                        selectedToken = token;
                    }

                    // Before selecting the token, ensure the user has control over it
                    if (selectedToken != null && _permissionsMap.UserCanControlToken(_userMap.ClientId, selectedToken.Id))
                        _selectedTokens.Add(selectedToken);

                    // Activate the DraggingTool, to begin dragging the selected sprites
                    _draggingTool.IsDragging = true;
                }
                // If the mouse is currently not over any Token, clear the selected token collection
                // and store the mouse position as drag start
                else
                {
                    // Unselect all Tokens, unless SHIFT is being pressed
                    if (!Input.IsActionPressed("shift")) _selectedTokens.Clear();
                    _selectBoxStart = mousePosition;
                }
            }
            // If the left mouse is released, and there is currently a select box being dragged...
            if (Input.IsActionJustReleased("select"))
            {
                if (_draggingTool.IsDragging)
                {
                    _draggingTool.IsDragging = false;
                    _draggingTool.CommitPositions();

                    if (JustPlacing)
                    {
                        foreach (var token in _selectedTokens)
                        {
                            token.Teleport(token.PivotPosition);
                            token.Visible = true;

                            var newToken = new TokensCreatedModel(
                                _userMap.ClientId,
                                new TokenModel[] {
                                    new TokenModel(
                                        token.Instance.Part.Key,
                                        token.Id,
                                        (token.PivotPosition.X, token.PivotPosition.Y),
                                        false
                                    )
                                }
                            );

                            if (_netManager.IsHost) _netManager.SendToAll(newToken, true);
                            else _netManager.SendToHost(newToken, true);
                        }
                        JustPlacing = false;
                    }
                    else if(SelectedTokens.Any()) SendTokenTransformMessage();
                }

                if (_resizingTool.IsResizing)
                {
                    _resizingTool.StopResizing();
                    SendTokenTransformMessage();
                }

                if (_selectBoxStart != null)
                {
                    // Create bounds over the rect and select any Tokens that lie within those bounds
                    Rect2 dragBounds = new Rect2(_selectBoxStart.Value, mousePosition - _selectBoxStart.Value).Abs();
                    foreach (var token in _tokens)
                    {
                        if (dragBounds.Encloses(token.Bounds))
                        {
                            // Ensure that the client is either the host, or has
                            // control over the given Token.
                            if (_permissionsMap.UserCanControlToken(_userMap.ClientId, token.Id))
                            {
                                _selectedTokens.Add(token);
                            }
                        }
                    }
                }
                // Then clear the select box
                _selectBoxStart = null;
            }
            if (Input.IsActionJustPressed("delete"))
            {
                foreach (var token in _selectedTokens)
                {
                    if (
                        _permissionsMap.UserCanControlToken(_userMap.ClientId, token.Id) && 
                        _permissionsMap[Permission.DeleteTokens]
                    )  {
                        _tokenMap.RemoveToken(token);
                        var model = new TokensDeletedModel(_userMap.ClientId, new [] { token.Id });
                        if(_netManager.IsHost) _netManager.SendToAll(model, true);
                        else _netManager.SendToHost(model, true);
                    }
                }
            }
            if (Input.IsActionJustPressed("right-click") && _draggingTool.IsDragging)
                _draggingTool.FlipAll();

            QueueRedraw();
        }
    }

    private void SendTokenTransformMessage()
    {
        var tokenTransforms = _selectedTokens.Select(
            token => new TokenTransform(
                token.Id,
                token.PivotPosition.X,
                token.PivotPosition.Y,
                token.Scale.X,
                token.Scale.Y,
                token.Direction
            )
        ).ToArray();

        if (_netManager.IsHost) _netManager.SendToAll(new TokensTransformedModel(default, tokenTransforms), true);
        else _netManager.SendToHost(new TokensTransformedModel(_userMap.ClientId, tokenTransforms), true);
    }

    public override void _Draw()
    {
        if (_selectBoxStart != null && !_resizingTool.IsResizing)
        {
            Rect2 dragBounds = new Rect2(_selectBoxStart.Value, GetGlobalMousePosition() - _selectBoxStart.Value).Abs();
            DrawRect(dragBounds, new Color(1.0f, 1.0f, 1.0f, 0.25f), filled: true);
            foreach (var token in _tokens)
            {
                if (dragBounds.Encloses(token.Bounds))
                {
                    DrawRect(token.Bounds, new Color(0.75f, 0.75f, 0.75f), filled: false, width: -1);
                }
            }
        }
        foreach (var token in _selectedTokens)
        {
            if (!JustPlacing)
            {
                DrawRect(token.Bounds, new Color(0.75f, 0.75f, 0.75f), filled: false, width: -1);
            }
        }
    }

    public void SelectTokens(IEnumerable<Token> tokens)
    {
        _selectedTokens.Clear();
        foreach (var token in tokens) _selectedTokens.Add(token);
        _draggingTool.IsDragging = Input.IsActionPressed("select");
        _resizingTool.StopResizing();
    }
    public void RegisterToken(Token token)
    {
        _tokens.Add(token);
        token.MouseEnter += OnMouseEnterToken;
        token.MouseExit += OnMouseExitToken;
    }
    public void RemoveToken(Token token)
    {
        _tokens.Remove(token);
        _selectedTokens.Remove(token);
    }
    public void OnMouseEnterToken(Token token) 
    {
        GD.Print(token.Name);
        _tokenHeap.Push(token);
    }
    public void OnMouseExitToken(Token token) => _tokenHeap.Remove(token);
    public void OnResizeBoxEnter(int anchorIdx) => _resizeDirection = (ResizeDirection)anchorIdx;
    public void OnResizeBoxExit(int anchorIdx) => _resizeDirection = null;
}
