using Dungeoner;
using Godot;

public partial class Brackets : Node2D
{
	[Signal]
	public delegate void BracketMouseClickedEventHandler(int position);

	[Signal]
	public delegate void BracketMouseReleasedEventHandler(int position);

	private TokenSprite[] _anchors;
	private TokenSprite 
		_top, _topRight, _right, _bottomRight, 
		_bottom, _bottomLeft, _left, _topLeft;
	
	private AnchorPosition? _mouseOnAnchor;

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

		_top.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.Top);
		_topRight.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.TopRight);
		_right.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.Right);
		_bottomRight.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.BottomRight);
		_bottom.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.Bottom);
		_bottomLeft.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.BottomLeft);
		_left.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.Left);
		_topLeft.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.TopLeft);

		_top.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.Top);
		_topRight.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.TopRight);
		_right.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.Right);
		_bottomRight.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.BottomRight);
		_bottom.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.Bottom);
		_bottomLeft.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.BottomLeft);
		_left.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.Left);
		_topLeft.MouseEnter += (_) => OnMouseEnterAnchor(AnchorPosition.TopLeft);

		_top.MouseExit += (_) => OnMouseExitAnchor(AnchorPosition.Top);
		_topRight.MouseExit += (_) => OnMouseExitAnchor(AnchorPosition.TopRight);
		_right.MouseExit += (_) => OnMouseExitAnchor(AnchorPosition.Right);
		_bottomRight.MouseExit += (_) => OnMouseExitAnchor(AnchorPosition.BottomRight);
		_bottom.MouseExit += (_) => OnMouseExitAnchor(AnchorPosition.Bottom);
		_bottomLeft.MouseExit += (_) => OnMouseExitAnchor(AnchorPosition.BottomLeft);
		_left.MouseExit += (_) => OnMouseExitAnchor(AnchorPosition.Left);
		_topLeft.MouseExit += (_) => OnMouseExitAnchor(AnchorPosition.TopLeft);
	}

	public override void _Process(double delta) {
		if(_mouseOnAnchor != null) {
			if(Input.IsActionJustPressed("select")) {
				EmitSignal(SignalName.BracketMouseClicked, (int)_mouseOnAnchor.Value);
			} else if(Input.IsActionJustReleased("select")) {
				EmitSignal(SignalName.BracketMouseClicked, (int)_mouseOnAnchor.Value);
			}
		}
	}

	public void SetRect(Rect2 rect) {
		var center = rect.GetCenter();

		_top.Position = new(center.X, rect.Position.Y);
		_topRight.Position = new(rect.End.X, rect.Position.Y);
		_right.Position = new(rect.End.X, center.Y);
		_bottomRight.Position = new(rect.End.X, rect.End.Y);
		_bottom.Position = new(center.X, rect.End.Y);
		_bottomLeft.Position = new(rect.Position.X, rect.End.Y);
		_left.Position = new(rect.Position.X, center.Y);
		_topLeft.Position = new(rect.Position.X, rect.Position.Y);
	}

	private void OnMouseEnterAnchor(AnchorPosition position) => _mouseOnAnchor = position;
	private void OnMouseExitAnchor(AnchorPosition position) => _mouseOnAnchor = null;
}
