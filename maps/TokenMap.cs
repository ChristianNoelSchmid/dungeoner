using System;
using System.Collections.Generic;
using System.Linq;
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

    // Collection of ids mapped to their Tokens
    // Enables quick access of tokens by Guid
    // (ie. client recv id to remove from host)
    private Dictionary<Guid, Token> _idTokens = new();

    // Collection of Tokens mapped to their ids
    // Enables quick access of Guids by their Token (ie. host actions)
    // (ie. host deleting Token - then sending id to client)
    private Dictionary<Token, Guid> _tokenIds = new();

    public Token this[Guid guid]
    {
        get => _idTokens[guid];
        set => AddToken(guid, value);
    }

    public Guid this[Token token]
    {
        get => _tokenIds[token];
        set => AddToken(value, token);
    }

    public TokensCreatedModel? ToTokensCreatedModel(Guid userId)
    {
        if(!_idTokens.Any()) return null;
        return new TokensCreatedModel(userId, _idTokens.Values.Select(token => new TokenModel(
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
        _tokenIds.Add(token, id.Value);

        AddChild(token);
        _selectionTool.RegisterToken(token);
        token.Id = id.Value;

        return id.Value;
    }

    public void RemoveToken(Guid id)
    {
        _selectionTool.RemoveToken(_idTokens[id]);
        RemoveChild(_idTokens[id]);

        _tokenIds.Remove(_idTokens[id]);
        _idTokens.Remove(id);
    }

    public void RemoveToken(Token token)
    {
        _selectionTool.RemoveToken(token);
        RemoveChild(token);

        _idTokens.Remove(_tokenIds[token]);
        _tokenIds.Remove(token);
    }

    public bool ContainsId(Guid id) => _idTokens.ContainsKey(id);

    public void ScaleToken(Guid id, Vector2 scale) => _idTokens[id].GlobalScale = scale;
}
