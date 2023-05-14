using Godot;
using System;

public partial class TokenDragger : Node2D
{
    // The Token Node2D 
    private Node2D _parent = default!;
    // Whether the mouse is currently over the Token's sprite
    private bool _mouseOver = false;

    // A singleton representing the current node being dragged
    private static Node2D _targeted = null;

    // A singleton representing
    private static Vector2 _offsetFromMouse;

    public override void _Ready() => _parent = GetParent<Node2D>();

    public override void _Process(double delta)
    {
        var mousePosition = GetGlobalMousePosition();
        if (_mouseOver && Input.IsActionJustPressed("select"))
        {
            if (_targeted == null || _parent.Position.Y > _targeted.Position.Y)
            {
                _targeted = _parent;
                _offsetFromMouse = mousePosition - _parent.Position;
            }
        }
        else if (!Input.IsActionPressed("select"))
        {
            _targeted = null;
        }
        if (_targeted == _parent)
        {
            _parent.Position = mousePosition - _offsetFromMouse;
        }
    }
    public void OnSpriteMouseEnter(TokenSprite _) => _mouseOver = true;
    public void OnSpriteMouseExit(TokenSprite _) => _mouseOver = false;
}
