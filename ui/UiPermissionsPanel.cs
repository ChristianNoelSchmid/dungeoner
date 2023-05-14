using Dungeoner.Maps;
using Godot;
using System;

public partial class UiPermissionsPanel : Panel
{
    private NetworkManager _netManager = default!;
    private PermissionsMap _permissionsMap = default!;
    private UserMap _userMap = default!;

    public override void _Ready()
    {
        _netManager = (NetworkManager)GetNode("/root/Main/NetworkManager");
        _permissionsMap = (PermissionsMap)GetNode("/root/Main/PermissionsMap");
        _userMap = (UserMap)GetNode("/root/Main/UserMap");
    }

    public void CreateChecked(bool isChecked)
    {
        foreach (var user in _userMap.Users)
        {
            _permissionsMap[user.Id, Permission.CreateTokens] = isChecked;
            _netManager.SendTo(user, new UpdateUserPermissionsModel(new[] { (Permission.CreateTokens, isChecked) }), true);
        }
    }
    public void MoveChecked(bool isChecked)
    {
        foreach (var user in _userMap.Users)
        {
            _permissionsMap[user.Id, Permission.MoveTokens] = isChecked;
            _netManager.SendTo(user, new UpdateUserPermissionsModel(new[] { (Permission.MoveTokens, isChecked) }), true);
        }
    }
    public void DeleteChecked(bool isChecked)
    {
		foreach(var user in _userMap.Users) 
		{
			_permissionsMap[user.Id, Permission.DeleteTokens] = isChecked;
			_netManager.SendTo(user, new UpdateUserPermissionsModel(new[] { (Permission.DeleteTokens, isChecked) }), true);
		}
    }
}
