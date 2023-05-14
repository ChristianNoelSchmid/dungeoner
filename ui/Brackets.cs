using Dungeoner;
using Godot;

public partial class Brackets : Node2D
{
    [Export]
    Camera2D _mainCamera;

    [Signal]
    public delegate void MouseEnterBracketEventHandler(int position);

    [Signal]
    public delegate void MouseExitBracketEventHandler(int position);

    private TokenSprite[] _anchors;
    private TokenSprite
        _top, _topRight, _right, _bottomRight,
        _bottom, _bottomLeft, _left, _topLeft;

    private ResizeDirection? _mouseOnAnchor;

    public override void _Ready()
    {
        _top = (TokenSprite)FindChild("Top");
        _topRight = (TokenSprite)FindChild("TopRight");
        _right = (TokenSprite)FindChild("Right");
        _bottomRight = (TokenSprite)FindChild("BottomRight");
        _bottom = (TokenSprite)FindChild("Bottom");
        _bottomLeft = (TokenSprite)FindChild("BottomLeft");
        _left = (TokenSprite)FindChild("Left");
        _topLeft = (TokenSprite)FindChild("TopLeft");

        _top.MouseEnter += (_) => OnMouseEnterAnchor(ResizeDirection.Top);
        _topRight.MouseEnter += (_) => OnMouseEnterAnchor(ResizeDirection.TopRight);
        _right.MouseEnter += (_) => OnMouseEnterAnchor(ResizeDirection.Right);
        _bottomRight.MouseEnter += (_) => OnMouseEnterAnchor(ResizeDirection.BottomRight);
        _bottom.MouseEnter += (_) => OnMouseEnterAnchor(ResizeDirection.Bottom);
        _bottomLeft.MouseEnter += (_) => OnMouseEnterAnchor(ResizeDirection.BottomLeft);
        _left.MouseEnter += (_) => OnMouseEnterAnchor(ResizeDirection.Left);
        _topLeft.MouseEnter += (_) => OnMouseEnterAnchor(ResizeDirection.TopLeft);

        _top.MouseExit += (_) => OnMouseExitAnchor(ResizeDirection.Top);
        _topRight.MouseExit += (_) => OnMouseExitAnchor(ResizeDirection.TopRight);
        _right.MouseExit += (_) => OnMouseExitAnchor(ResizeDirection.Right);
        _bottomRight.MouseExit += (_) => OnMouseExitAnchor(ResizeDirection.BottomRight);
        _bottom.MouseExit += (_) => OnMouseExitAnchor(ResizeDirection.Bottom);
        _bottomLeft.MouseExit += (_) => OnMouseExitAnchor(ResizeDirection.BottomLeft);
        _left.MouseExit += (_) => OnMouseExitAnchor(ResizeDirection.Left);
        _topLeft.MouseExit += (_) => OnMouseExitAnchor(ResizeDirection.TopLeft);
    }

    public override void _Process(double delta)
    {
        float scale = Mathf.Clamp((5.5f - _mainCamera.Zoom.X) / 2.0f, 1.0f, 3.0f);

        _top.Scale = _topRight.Scale = _right.Scale =
        _bottomRight.Scale = _bottom.Scale = _bottomLeft.Scale =
        _left.Scale = _topLeft.Scale = new Vector2(scale, scale);
    }

    public void SetRect(Rect2 rect)
    {
        var center = rect.GetCenter();

        _top.GlobalPosition = new(center.X, rect.Position.Y);
        _topRight.GlobalPosition = new(rect.End.X, rect.Position.Y);
        _right.GlobalPosition = new(rect.End.X, center.Y);
        _bottomRight.GlobalPosition = new(rect.End.X, rect.End.Y);
        _bottom.GlobalPosition = new(center.X, rect.End.Y);
        _bottomLeft.GlobalPosition = new(rect.Position.X, rect.End.Y);
        _left.GlobalPosition = new(rect.Position.X, center.Y);
        _topLeft.GlobalPosition = new(rect.Position.X, rect.Position.Y);
    }

    private void OnMouseEnterAnchor(ResizeDirection position)
    {
        EmitSignal(SignalName.MouseEnterBracket, (int)position);
    }
    private void OnMouseExitAnchor(ResizeDirection position)
    {
        EmitSignal(SignalName.MouseExitBracket, (int)position);
    }
}
