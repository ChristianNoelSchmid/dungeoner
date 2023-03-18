using Godot;

public partial class TokenSprite : Sprite2D
{
	[Signal]
	public delegate void MouseEnterEventHandler(TokenSprite tokenSprite);

	[Signal]
	public delegate void MouseExitEventHandler(TokenSprite tokenSprite);

	public Rect2 SpriteBounds => new(
		GlobalPosition.X + (Offset.X * GlobalScale.X),
		GlobalPosition.Y + (Offset.Y * GlobalScale.Y),
		_spriteHalfSize.X * 2,
		_spriteHalfSize.Y * 2
	);

	private Image _spriteImg;
	private Vector2I _spriteHalfSize;
	private bool _mouseOver = false;

	public bool InRect(Rect2 bounds) => bounds.Encloses(SpriteBounds);

	public override void _Ready()
	{
		_spriteImg = Texture.GetImage();
		_spriteHalfSize = new Vector2I(
			(int)(_spriteImg.GetWidth() * GlobalScale.X) / 2, 
			(int)(_spriteImg.GetHeight() * GlobalScale.Y) / 2
		);
	}

	public override void _Process(double delta)
	{
		// Get the mouse position relative to the Node
		var mousePosition = GetGlobalMousePosition();

		// If the mouse is within bounds of the sprite image determine if it is 
		// currently hovering over a pixel
		if(SpriteBounds.HasPoint(mousePosition)) {
			var pixelPos = new Vector2I (
				(int)((mousePosition.X - GlobalPosition.X) / GlobalScale.X - Offset.X),
				(int)((mousePosition.Y - GlobalPosition.Y) / GlobalScale.Y - Offset.Y)
			);

			pixelPos = pixelPos.Clamp(Vector2I.Zero, new (_spriteImg.GetWidth() , _spriteImg.GetHeight()));

			if(_spriteImg.GetPixelv(pixelPos).A > 0.0) 
			{
				if(!_mouseOver) 
				{
					_mouseOver = true;
					EmitSignal(SignalName.MouseEnter, this);
				}
				return;
			} 
		}
		if(_mouseOver) 
		{
			_mouseOver = false;
			EmitSignal(SignalName.MouseExit, this);
		}
	}
}
