using System.Collections.Generic;
using System.Linq;
using Dungeoner;
using Godot;

public partial class Token : Node2D
{	
	[Signal]
	public delegate void MouseEnterEventHandler(Token token);
	[Signal]
	public delegate void MouseExitEventHandler(Token token);

	private TokenSprite _hoveringSprite;
	private HashSet<TokenSprite> _tokenSprites;
	public bool InRect(Rect2 rect) => _tokenSprites.All(s => s.InRect(rect));

	public Rect2 Bounds => _tokenSprites.Select(s => s.SpriteBounds).OuterBounds();

	public override void _Ready() 
	{
		_tokenSprites = FindChildren("TokenSprite*").Select(child => (TokenSprite)child).ToHashSet();
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
