using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Dungeoner.TokenManipulation;

public partial class ResizingTool : Node2D
{
    [Export]
    private SelectionTool _selectionTool = default!;
    [Export]
    private Brackets _brackets = default!;

    private bool _isResizing = false;
    public bool IsResizing => _isResizing;
    private Dictionary<Token, (Vector2, Vector2)> _tokenStartPosAndScale = new();
    private Rect2 _start;
    private Vector2 _mouseDragStart;
    private ResizeDirection _resizeDirection;

    public void StartResizing(IEnumerable<Token> tokens, Rect2 start, ResizeDirection anchor)
    {
        _start = start;
        _tokenStartPosAndScale = new();
        _resizeDirection = anchor;
        _isResizing = true;

        foreach (var token in tokens)
        {
            _tokenStartPosAndScale.Add(token, (
                (token.PivotPosition - _start.Position) / _start.Size,
                token.GlobalScale
            ));
        }
    }

    public void StopResizing() => _isResizing = false;

    public override void _Process(double delta)
    {
        var mousePosition = GetGlobalMousePosition();
        if (_isResizing)
        {
            Rect2 bounds = _selectionTool.SelectedTokens.Select(t => t.Bounds).OuterBounds();
            Rect2 newRect = _resizeDirection switch
            {
                ResizeDirection.Top => new(bounds.Position.X, mousePosition.Y, bounds.Size.X, bounds.End.Y - mousePosition.Y),
                ResizeDirection.TopRight => new(bounds.Position.X, mousePosition.Y, mousePosition.X - bounds.Position.X, bounds.End.Y - mousePosition.Y),
                ResizeDirection.Right => new(bounds.Position.X, bounds.Position.Y, mousePosition.X - bounds.Position.X, bounds.Size.Y),
                ResizeDirection.BottomRight => new(bounds.Position.X, bounds.Position.Y, mousePosition.X - bounds.Position.X, mousePosition.Y - bounds.Position.Y),
                ResizeDirection.Bottom => new(bounds.Position.X, bounds.Position.Y, bounds.Size.X, mousePosition.Y - bounds.Position.Y),
                ResizeDirection.BottomLeft => new(mousePosition.X, bounds.Position.Y, bounds.End.X - mousePosition.X, mousePosition.Y - bounds.Position.Y),
                ResizeDirection.Left => new(mousePosition.X, bounds.Position.Y, bounds.End.X - mousePosition.X, bounds.Size.Y),
                _ => new(mousePosition.X, mousePosition.Y, bounds.End.X - mousePosition.X, bounds.End.Y - mousePosition.Y)
            };

            // If the aspect-ratio wants to be kept
            if (Input.IsActionPressed("shift"))
            {
                newRect = CropToAspectRatio(newRect);
            }
            foreach ((var token, (var positionRatio, var scale)) in _tokenStartPosAndScale)
            {
                token.Teleport(new(
                    Mathf.Lerp(newRect.Position.X, newRect.End.X, positionRatio.X),
                    Mathf.Lerp(newRect.Position.Y, newRect.End.Y, positionRatio.Y)
                ));
                token.Scale = scale * newRect.Size / _start.Size;
            }
        }
        if (_selectionTool.SelectedTokens.Any() && !_selectionTool.JustPlacing)
        {
            // Show the brackets if they're currently invisible
            if (!_brackets.IsProcessing())
            {
                _brackets.Visible = true;
                _brackets.SetProcess(true);
            }

            var bound = _selectionTool.SelectedTokens.Select(t => t.Bounds).OuterBounds();
            _brackets.SetRect(bound);
        }
        else
        {
            _brackets.Visible = false;
            _brackets.SetProcess(false);
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_isResizing)
        {
            foreach (var token in _tokenStartPosAndScale.Keys)
            {
                var gridSize = token.ScaledGridSize;
                Vector2 gridPosition = token.PivotPosition;
                if (gridSize.X % 2 == 0)
                {
                    float offsetX = gridPosition.X % Constants.GRID_SIZE;
                    gridPosition.X -= offsetX;
                    if (offsetX > Constants.GRID_SIZE / 2) gridPosition.X += Constants.GRID_SIZE / 2;
                    else gridPosition.X -= Constants.GRID_SIZE / 2;
                }
                else
                {
                    gridPosition.X = Mathf.RoundToInt(gridPosition.X / Constants.GRID_SIZE) * Constants.GRID_SIZE;
                }
                if (gridSize.Y % 2 == 0)
                {
                    float offsetY = gridPosition.Y % Constants.GRID_SIZE;
                    gridPosition.Y -= offsetY;
                    if (offsetY > Constants.GRID_SIZE / 2) gridPosition.Y += Constants.GRID_SIZE / 2;
                    else gridPosition.Y -= Constants.GRID_SIZE / 2;
                }
                else
                {
                    gridPosition.Y = Mathf.RoundToInt(gridPosition.Y / Constants.GRID_SIZE) * Constants.GRID_SIZE;
                }

                var rectSize = new Vector2(
                    Mathf.RoundToInt(token.Instance.Part.GridSize!.Value.X * token.Scale.X),
                    Mathf.RoundToInt(token.Instance.Part.GridSize!.Value.Y * token.Scale.Y)
                ) * Constants.GRID_SIZE;

                var rect = new Rect2(gridPosition - rectSize / 2.0f, rectSize);
                DrawRect(rect, new Color(1.0f, 1.0f, 1.0f), false);
            }
        }
    }

    private Rect2 CropToAspectRatio(Rect2 newRect)
    {
        // The aspect ratio of the original Rect
        float startAspect = _start.Size.X / _start.Size.Y;
        // The aspect ratio of the new Rect
        float newAspect = newRect.Size.X / newRect.Size.Y;

        Vector2 center = newRect.GetCenter();

        float aspectWidth = newRect.Size.Y * startAspect;
        float aspectHeight = newRect.Size.X / startAspect;

        // If the bracket is vertical direction, set the Rect to size the height, and center the width
        if (_resizeDirection == ResizeDirection.Top || _resizeDirection == ResizeDirection.Bottom)
        {
            newRect = new(center.X - aspectWidth / 2, newRect.Position.Y, aspectWidth, newRect.Size.Y);
            // If the bracket is horizontal direction, set the Rect2 the size the width, and center the height
        }
        else if (_resizeDirection == ResizeDirection.Left || _resizeDirection == ResizeDirection.Right)
        {
            newRect = new(newRect.Position.X, center.Y - aspectHeight / 2, newRect.Size.X, aspectHeight);
        }
        // Otherwise, determine the scale factor by comparing the aspect ratio of the new Rect2 to the original
        else
        {
            var oldRect = newRect;
            float scaleFactor = startAspect < newAspect ? _start.Size.Y / newRect.Size.Y : _start.Size.X / newRect.Size.X;
            newRect.Size = _start.Size / scaleFactor;

            // Resizing will be based on the top-left corner of the Rect2, so shift it right and down if required
            switch (_resizeDirection)
            {
                case ResizeDirection.TopLeft:
                case ResizeDirection.Left:
                case ResizeDirection.BottomLeft:
                    newRect.Position += new Vector2(oldRect.End.X - newRect.End.X, 0);
                    break;
            }
            switch (_resizeDirection)
            {
                case ResizeDirection.Top:
                case ResizeDirection.TopLeft:
                case ResizeDirection.TopRight:
                    newRect.Position += new Vector2(0, oldRect.End.Y - newRect.End.Y);
                    break;
            }
        }

        return newRect;
    }
}
