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

    private TokenInstance _instance;
    public TokenInstance Instance
    {
        get => _instance;
        set
        {
            _instance = value;
            var texture = _instance.Texture;

            var sprite = GetChild<Sprite2D>(0);
            sprite.Texture = texture;
            sprite.Offset = new Vector2(0.0f, -Instance.Part.SortHeight!.Value);
            sprite.Position = new Vector2(0.0f, Instance.Part.SortHeight!.Value) - Instance.Part.Pivot;
        }
    }

    public Guid Id { get; set; }

    private Vector2 _pivotPosition;
    public Vector2 PivotPosition
    {
        get => _pivotPosition; // - new Vector2(0.0f, _instance.Part.SortHeight) + _instance.Part.Pivot * GlobalScale;
        set => _pivotPosition = value; // + new Vector2(0.0f, _instance.Part.SortHeight) - _instance.Part.Pivot * GlobalScale;
    }

    public void Teleport(Vector2 position)
    {
        PivotPosition = position;
        GlobalPosition = _pivotPosition;
    }

    private TokenSprite? _hoveringSprite;
    private HashSet<TokenSprite> _tokenSprites = default!;
    public bool InRect(Rect2 rect) => _tokenSprites.All(s => s.InRect(rect));

    public Rect2 Bounds => _tokenSprites.Select(s => s.SpriteBounds).OuterBounds();

    public Vector2I ScaledGridSize => new(
        Mathf.RoundToInt(Instance.Part.GridSize!.Value.X * GlobalScale.X),
        Mathf.RoundToInt(Instance.Part.GridSize.Value.Y * GlobalScale.Y)
    );

    private Direction _direction = Direction.Right;
    public Direction Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            foreach (var sprite in _tokenSprites)
            {
                sprite.FlipH = _direction == Direction.Left;
            }
        }
    }

    public override void _Ready()
    {
        _tokenSprites = FindChildren("TokenSprite*").Select(child => (TokenSprite)child).ToHashSet();
    }

    int counter = 10;
    public override void _Process(double delta)
    {
        // The Token should always be in the process of moving towards
        // its MapPosition. This gives a transition movement effect
        // when the MapPosition is changed.
        if (GlobalPosition != _pivotPosition)
        {
            Vector2 direction = (GlobalPosition - _pivotPosition).Normalized();
            GlobalPosition -= direction * Mathf.Min(GlobalPosition.DistanceTo(_pivotPosition), TOKEN_MOVE_SPEED);
        }
    }

    public void OnTokenSpriteMouseEnter(TokenSprite tokenSprite)
    {
        _hoveringSprite = tokenSprite;
        EmitSignal(SignalName.MouseEnter, this);
    }
    public void OnTokenSpriteMouseExit(TokenSprite tokenSprite)
    {
        if (_hoveringSprite == tokenSprite) _hoveringSprite = null;
        EmitSignal(SignalName.MouseExit, this);
    }
}
