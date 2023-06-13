using Godot;
using System.Linq;

namespace Dungeoner.Ui;

public partial class UiTopMenu : Control
{
    private Button[] _menuButtons = default!;
	private Button? _hoveringButton = null;

    public override void _Ready()
    {
        _menuButtons = GetChildren()
            .Where(child => child is Button)
            .Select(child => child as Button)
            .ToArray()!;

        for(int i = 0; i < _menuButtons.Length; i += 1)         
		{
			int menuIdx = i;
			var button = _menuButtons[i];
            button.Pressed += () => HandleClick(button);
            button.MouseEntered += () => HandleMouseEnterButton(button);

			button.GetChild<ItemList>(0).ItemClicked += (itemIdx, _atPosition, _mouseBtn) => {
				_hoveringButton?.GetChild<ItemList>(0).Hide();
				_hoveringButton?.GetChild<ItemList>(0).DeselectAll();
				OnMenuItemPressed(menuIdx, (int)itemIdx);
			};
        }
    }

    private void HandleClick(Button button)
    {
        if (_hoveringButton != null)
            _hoveringButton.GetChild<ItemList>(0).Hide();
        else
        {
            button.GetChild<ItemList>(0).Show();
			button.GetChild<ItemList>(0).GrabFocus();
			_hoveringButton = button;
        }
    }

    private void HandleMouseEnterButton(Button button)
    {
        if (_hoveringButton != null)
        {
			_hoveringButton.GetChild<ItemList>(0).Hide();
            button.GetChild<ItemList>(0).Show();
			_hoveringButton = button;
        }
    }

	private void OnMenuItemPressed(int menuIdx, int itemIdx) 
	{
		GD.Print($"Menu button idx: {menuIdx}, item idx {itemIdx}");
	}

	public override void _Process(double _delta) 
	{
		if(Input.IsActionJustPressed("select") && _hoveringButton != null)
		{
			_hoveringButton.GetChild<ItemList>(0).Hide();
			_hoveringButton = null;
		}
	}
}
