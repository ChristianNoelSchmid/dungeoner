using System;
using System.Collections.Generic;
using System.Linq;
using Dungeoner;
using Godot;

public partial class Token : Node2D
{	
	private const float TOKEN_MOVE_SPEED = 15.0f;

	[Signal]
	public delegate void MouseEnterEventHandler(Token token);
	[Signal]
	public delegate void MouseExitEventHandler(Token token);

	public Guid Id { get; set; }
	public Vector2 MapPosition { get; set; }
	public bool MapLocked { get; set; } = true;

	private TokenSprite? _hoveringSprite;
	private HashSet<TokenSprite> _tokenSprites = default!;
	public bool InRect(Rect2 rect) => _tokenSprites.All(s => s.InRect(rect));

	public Rect2 Bounds => _tokenSprites.Select(s => s.SpriteBounds).OuterBounds();

	public override void _Ready() {
		_tokenSprites = FindChildren("TokenSprite*").Select(child => (TokenSprite)child).ToHashSet();
	}

	public override void _Process(double delta) {
		// The Token should always be in the process of moving towards
		// its MapPosition. This gives a transition movement effect
		// when the MapPosition is changed.
		if(Position != MapPosition) {
			Vector2 direction = (MapPosition - Position).Normalized();
			Position += direction * Mathf.Min(Position.DistanceTo(MapPosition), TOKEN_MOVE_SPEED);
		}
	}

	public void OnTokenSpriteMouseEnter(TokenSprite tokenSprite) 
	{
		_hoveringSprite = tokenSprite;	
		EmitSignal(SignalName.MouseEnter, this);
	}
	public void OnTokenSpriteMouseExit(TokenSprite tokenSprite) 
	{
		if(_hoveringSprite == tokenSprite) _hoveringSprite = null;
		EmitSignal(SignalName.MouseExit, this);
	}
}
