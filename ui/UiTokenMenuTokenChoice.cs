using Dungeoner.Maps;
using Godot;

namespace Dungeoner.Ui;

public partial class UiTokenMenuTokenChoice : ItemList
{
    [Export]
    private UiTokenList _uiTokenList = default!;

    private bool _mouseOver = false;

    public override void _Ready() => Select(0, true);
    public void OnChoiceChange(int idx)
        => _uiTokenList.PlacingTokenType = idx == 0 ? TokenType.World : TokenType.Floor;
    public void OnMouseEnter() => _mouseOver = true;
    public void OnMouseExit()
    {
        _mouseOver = false;
        ReleaseFocus();
    }
}
