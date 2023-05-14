
using System;
using System.Net;
using Dungeoner.Maps;
using Dungeoner.Server.Events.NetworkEventHandlers;
using Dungeoner.TokenManipulation;
using Dungeoner.Ui;
using Godot;

[NetworkEventModel("UpdateUserPermissions")]
public record UpdateUserPermissionsModel((Permission, bool)[] Permissions) : NetworkEventModel;

[NetworkEvent("UpdateUserPermissions")]
public partial class UpdateUserPermissionsEventHandler : NetworkEventHandler<UpdateUserPermissionsModel>
{
    [Export] private UserMap _userMap = default!;
    [Export] private PermissionsMap _permissionsMap = default!;
    [Export] private UiMain _ui = default!;
    [Export] private SelectionTool _selectionTool = default!;

    protected override void OnClientEventProcess(UpdateUserPermissionsModel netEvent, ClientCallback callback)
    {
        foreach ((var permission, bool value) in netEvent.Permissions)
        {
            _permissionsMap[_userMap.ClientId, permission] = value;

            if(permission == Permission.CreateTokens) 
                _ui.EnableSection(UiPanel.Tokens, value);
            else if(permission == Permission.MoveTokens && !value) 
                _selectionTool.SelectTokens(Array.Empty<Token>());
        }
    }

    protected override void OnHostEventProcess(UpdateUserPermissionsModel netEvent, IPEndPoint sender, HostCallback callback) { }
}
