
using System;
using System.Collections.Generic;
using Godot;

namespace Dungeoner.Maps;

public partial class PermissionsMap : Node
{
    [Export] private UserMap _userMap = default!;
    [Export] private TokenMap _tokenMap = default!;
    [Export] private NetworkManager _netManager = default!;

    private Dictionary<Guid, HashSet<Guid>> _controlledTokens = new();
    private Dictionary<Guid, Dictionary<Permission, bool>> _userPermissions = new();

    public bool this[Guid userId, Permission permission]
    {
        get
        {
            if(userId == default) return true;
            var permissions = _userPermissions.GetValueOrDefault(userId);
            return permissions?[permission] ?? false;
        }
        set
        {
            if (!_userPermissions.TryGetValue(userId, out var permissions))
            {
                permissions = new Dictionary<Permission, bool>();
                foreach (var newPermission in Enum.GetValues<Permission>())
                {
                    permissions[newPermission] = false;
                }
                _userPermissions[userId] = permissions;
            }
            permissions[permission] = value;
        }
    }

    public bool this[Permission permission]
    {
        get 
        {
            if(_userMap.ClientId == default) return true;
            return this[_userMap.ClientId, permission];
        }
        set => this[_userMap.ClientId, permission] = value;
    }

    public void UpdateUserTokenControl(Guid userId, Guid tokenId, bool canControl)
    {
        if (!_controlledTokens.TryGetValue(userId, out var tokens))
        {
            tokens = new HashSet<Guid>();
            _controlledTokens[userId] = tokens;
        }

        if (canControl) tokens.Add(tokenId);
        else tokens.Remove(tokenId);
    }

    public void UpdateUserTokenControl(Guid tokenId, bool canControl)
        => UpdateUserTokenControl(_userMap.ClientId, tokenId, canControl);

    /// <summary>
    /// Checks if a User can control a particular Token
    /// </summary>
    /// <param name="userId">The ID of the User to check</param>
    /// <param name="tokenId">The ID of the Token to check</param>
    /// <returns>Whether the User can control the particular Token</returns>
    public bool UserCanControlToken(Guid userId, Guid tokenId)
    {
        // If just checking if the host can control tokens as the host,
        // return true
        if (userId == default) return true;

        // First check if the user can move all Tokens by the Permission map
        var permissions = _userPermissions.GetValueOrDefault(userId);
        if (permissions != null && permissions[Permission.MoveTokens]) return true;

        // If not, check if this User can control this particular Token
        var tokens = _controlledTokens.GetValueOrDefault(userId);
        return tokens?.Contains(tokenId) ?? false;
    }
}

public enum Permission
{
    CreateTokens,
    MoveTokens,
    DeleteTokens
}