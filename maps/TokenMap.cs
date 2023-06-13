using System;
using System.Collections.Generic;
using System.Linq;
using Dungeoner.Collections;
using Dungeoner.Importers;
using Dungeoner.TokenManipulation;
using Godot;

namespace Dungeoner.Maps;

public partial class TokenMap : Node2D
{
    [Export]
    private TokenImporter _tokenImporter = default!;

    [Export]
    private SelectionTool _selectionTool = default!;

    [Export]
    private PermissionsMap _permissionsMap = default!;

    private Node2D _floor = default!;
    private Node2D _world = default!;

    // Collection of ids mapped to their Tokens
    // Enables quick access of tokens by Guid
    // (ie. client recv id to remove from host)
    private BiMap<Guid, Token> _idTokens = new();

    public override void _Ready() 
    {
        _world = (Node2D)FindChild("World");
        _floor = (Node2D)FindChild("Floor");
    }

    public Token this[Guid guid]
    {
        get => _idTokens[guid];
        set => AddToken(guid, value);
    }

    public Guid this[Token token]
    {
        get => _idTokens[token];
        set => AddToken(value, token);
    }

    public TokensCreatedModel? ToTokensCreatedModel(Guid userId)
    {
        if(!_idTokens.Any()) return null;
        return new TokensCreatedModel(userId, _idTokens.Set2.Select(token => new TokenModel(
            token.Instance.Part.Key,
            token.Id,
            (token.PivotPosition.X, token.PivotPosition.Y),
            _permissionsMap.UserCanControlToken(userId, token.Id)
        )));
    }

    public Guid AddToken(Guid? id, Token token)
    {
        if (!id.HasValue)
            id = Guid.NewGuid();

        _idTokens.Add(id.Value, token);

        if(token.TokenType == TokenType.World) _world.AddChild(token);
        else _floor.AddChild(token);

        _selectionTool.RegisterToken(token);
        token.Id = id.Value;

        return id.Value;
    }

    public void RemoveToken(Guid id)
    {
        _selectionTool.RemoveToken(_idTokens[id]);
        RemoveChild(_idTokens[id]);

        _idTokens.Remove(id);
    }

    public void RemoveToken(Token token)
    {
        _selectionTool.RemoveToken(token);
        RemoveChild(token);

        _idTokens.Remove(token);
    }

    public bool ContainsId(Guid id) => _idTokens.ContainsKey(id);

    public void ScaleToken(Guid id, Vector2 scale) => _idTokens[id].GlobalScale = scale;
}
