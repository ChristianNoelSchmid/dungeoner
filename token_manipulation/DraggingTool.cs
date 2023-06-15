using System.Collections.Generic;
using System.Linq;
using Dungeoner.Maps;
using Godot;

namespace Dungeoner.TokenManipulation;

public partial class DraggingTool : Node2D
{
    [Export] private SelectionTool _selectionTool = default!;
    [Export] private Node2D _world = default!;
    private PackedScene _tokenScene = GD.Load<PackedScene>("res://tokens/token.tscn");

    private Dictionary<Token, Token> _dragTokenMapping = new();
    private Dictionary<Token, Vector2> _offsets = new();

    private bool _isDragging = false;
    private bool _flipped = false;
    public bool IsDragging
    {
        get => _isDragging;
        set
        {
            _isDragging = value;
            var mousePosition = GetGlobalMousePosition();
            if (_isDragging)
            {
                foreach(var token in _offsets.Keys) token.GetParent()?.RemoveChild(token);
                _dragTokenMapping.Clear();
                _offsets.Clear();
                _flipped = false;

                foreach (var selectedToken in _selectionTool.SelectedTokens)
                {
                    var dragToken = _tokenScene.Instantiate<Token>();
                    dragToken.Instance = selectedToken.Instance;
                    dragToken.Modulate = new Color(2.0f, 2.0f, 2.0f, 0.75f);
                    if(selectedToken.TokenType == TokenType.Floor) dragToken.ZIndex = selectedToken.ZIndex;
                    dragToken.Teleport(selectedToken.PivotPosition);
                    dragToken.Scale = selectedToken.Scale;

                    _dragTokenMapping[selectedToken] = dragToken;
                    _offsets.Add(dragToken, selectedToken.PivotPosition - mousePosition);

                    selectedToken.GetParent().AddChild(dragToken);
                    dragToken.Direction = selectedToken.Direction;
                }
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        var mousePosition = GetGlobalMousePosition();
        if (IsDragging)
        {
            foreach (var token in _selectionTool.SelectedTokens)
            {
                var dragToken = _dragTokenMapping[token];
                dragToken.Teleport(_offsets[dragToken] + mousePosition);

                if (!Input.IsActionPressed("alt"))
                {
                    var gridPosition = new Vector2(
                        Mathf.Round(dragToken.PivotPosition.X / Constants.GRID_SIZE),
                        Mathf.Round(dragToken.PivotPosition.Y / Constants.GRID_SIZE)
                    );
                    if (Mathf.RoundToInt(dragToken.Scale.X * dragToken.Instance.Part.GridSize!.Value.X) % 2 == 0)
                        gridPosition.X -= 0.5f;
                    if (Mathf.RoundToInt(dragToken.Scale.Y * dragToken.Instance.Part.GridSize!.Value.Y) % 2 == 0)
                        gridPosition.Y -= 0.5f;

                    dragToken.Teleport(new(
                        gridPosition.X * Constants.GRID_SIZE,
                        gridPosition.Y * Constants.GRID_SIZE
                    ));
                }
            }
        }
    }

    public void FlipAll()
    {
        _flipped = !_flipped;
        foreach (var token in _dragTokenMapping.Values)
        {
            if (token.Direction == Direction.Left) token.Direction = Direction.Right;
            else token.Direction = Direction.Left;
        }
    }

    public void CommitPositions()
    {
        foreach (var token in _selectionTool.SelectedTokens)
        {
            var dragToken = _dragTokenMapping[token];
            token.PivotPosition = dragToken.PivotPosition;
            if (_flipped)
            {
                if (token.Direction == Direction.Left) token.Direction = Direction.Right;
                else token.Direction = Direction.Left;
            }
            dragToken.GetParent().RemoveChild(dragToken);
        }
    }
}
