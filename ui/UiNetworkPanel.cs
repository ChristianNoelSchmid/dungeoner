using Dungeoner.Ui;
using Godot;

public partial class UiNetworkPanel : Panel
{
    [Export] private UiMain _ui = default!;
    [Export] private UiPermissionsPanel _permissionsPanel = default!;
    private NetworkManager _networkManager = default!;
    private Button _openCheckBox = default!;

    public override void _Ready()
    {
        _networkManager = (NetworkManager)GetNode("/root/Main/NetworkManager");
        _openCheckBox = GetChild<Control>(0).GetChild<Button>(1);
    }

    public void OnCheckBoxClicked(bool isToggled)
    {
        if (isToggled) _networkManager.Open();
        else _networkManager.Close();
    }
    public void OnServerTypeItemClicked(int idx)
    {
        _networkManager.Close();
        _networkManager.IsHost = idx == 0;
        _openCheckBox.ButtonPressed = false;
        if(idx == 1) _permissionsPanel.Hide();
        else _permissionsPanel.Show();

        _ui.EnableSection(UiPanel.Map, _networkManager.IsHost);
        _ui.EnableSection(UiPanel.Tokens, _networkManager.IsHost);
    }
}
