using System.Collections.Generic;
using Godot;

namespace Dungeoner;

public class TokenResizer {
    private Dictionary<Token, (Vector2, Vector2)> _tokenStartPosAndScale = new();
    private Rect2 _start;
    private ResizeDirection _bracketPosition;

    public TokenResizer(IEnumerable<Token> tokens, Rect2 start, ResizeDirection anchor) {
        _start = start;
        _tokenStartPosAndScale = new();
        _bracketPosition = anchor;

        foreach(var token in tokens) {
            _tokenStartPosAndScale.Add(token, (
                (token.Position - _start.Position) / _start.Size, 
                token.GlobalScale
            ));
            GD.Print(_tokenStartPosAndScale[token]);
        }
    }

    public void Resize(Rect2 newRect, bool keepAspect) {
        // If the aspect-ratio wants to be kept
        if(keepAspect)
        {
            newRect = CropToAspectRatio(newRect);
        }
        foreach ((var token, (var positionRatio, var scale)) in _tokenStartPosAndScale) {
            token.Position = new(
                Mathf.Lerp(newRect.Position.X, newRect.End.X, positionRatio.X),
                Mathf.Lerp(newRect.Position.Y, newRect.End.Y, positionRatio.Y)
            );
            token.Scale = scale * newRect.Size / _start.Size;
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
        if (_bracketPosition == ResizeDirection.Top || _bracketPosition == ResizeDirection.Bottom) {
            newRect = new(center.X - aspectWidth / 2, newRect.Position.Y, aspectWidth, newRect.Size.Y);
        // If the bracket is horizontal direction, set the Rect2 the size the width, and center the height
        } else if (_bracketPosition == ResizeDirection.Left || _bracketPosition == ResizeDirection.Right) {
            newRect = new(newRect.Position.X, center.Y - aspectHeight / 2, newRect.Size.X, aspectHeight);
        }
        // Otherwise, determine the scale factor by comparing the aspect ratio of the new Rect2 to the original
        else
        {
            var oldRect = newRect;
            float scaleFactor = startAspect < newAspect ? _start.Size.Y / newRect.Size.Y : _start.Size.X / newRect.Size.X;
            newRect.Size = _start.Size / scaleFactor;

            // Resizing will be based on the top-left corner of the Rect2, so shift it right and down if required
            switch (_bracketPosition)
            {
                case ResizeDirection.TopLeft:
                case ResizeDirection.Left:
                case ResizeDirection.BottomLeft:
                    newRect.Position += new Vector2(oldRect.End.X - newRect.End.X, 0);
                    break;
            }
            switch (_bracketPosition)
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