using System;
using System.Collections.Generic;
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

	// Collection of ids mapped to their Tokens
	// Enables quick access of tokens by Guid 
	// (ie. client recv id to remove from host)
	private Dictionary<Guid, Token> _idTokens = new();

	// Collection of Tokens mapped to their ids
	// Enables quick access of Guids by their Token (ie. host actions)
	// (ie. host deleting Token - then sending id to client)
	private Dictionary<Token, Guid> _tokenIds = new();

	public Guid AddToken(Guid? id, Token token) {
		if(!id.HasValue) id = Guid.NewGuid();

		_idTokens.Add(id.Value, token);
		_tokenIds.Add(token, id.Value);

		AddChild(token);
		_selectionTool.RegisterToken(token);

		return id.Value;
	}

	public void RemoveToken(Guid id) {
		_selectionTool.RemoveToken(_idTokens[id]); 
		RemoveChild(_idTokens[id]);

		_tokenIds.Remove(_idTokens[id]);
		_idTokens.Remove(id);
	}
	public void RemoveToken(Token token) {
		_selectionTool.RemoveToken(token); 
		RemoveChild(token);

		_idTokens.Remove(_tokenIds[token]);
		_tokenIds.Remove(token);
	}
}