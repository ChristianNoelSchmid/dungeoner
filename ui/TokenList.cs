using Dungeoner.Importers;
using Dungeoner.Maps;
using Dungeoner.TokenManipulation;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeoner;

public partial class TokenList : UiList {
	private  TokenImporter _importer = default!;
	private TokenMap _tokenMap = default!;
	private SelectionTool _selectionTool = default!;
	private UserMap _userMap = default!;
	private NetworkManager _netManager = default!;
	private bool _mouseOver = false;
	private bool _draggingOffList = false;
	List<TokenMeta> _listImgMetas = new();

	public override void _Ready() {
		_importer = (TokenImporter)GetNode("/root/Main/Importers/TokenImporter");
		_tokenMap = (TokenMap)GetNode("/root/Main/TokenMap");
		_selectionTool = (SelectionTool)GetNode("/root/Main/SelectionTool");
		_userMap = (UserMap)GetNode("/root/Main/UserMap");
		_netManager = (NetworkManager)GetNode("/root/Main/NetworkManager");
	}

 	public override void _Process(double delta) {
		if(Input.IsKeyPressed(Key.Escape)) { ReleaseFocus(); }
		if(Input.IsActionJustPressed("select") && !_mouseOver) ReleaseFocus();
		if(Input.IsActionJustPressed("select") && _mouseOver) {
			_draggingOffList = true;
		}

		if(Input.IsActionJustReleased("select")) _draggingOffList = false;
	}

	public override void QueryList(params string[] globs) {
		Clear();
		List<TokenMeta> metas = new();
		foreach(var glob in globs) {
			metas.AddRange(_importer.GetAllMatchingMetas(glob));
		}
		_listImgMetas.Clear();
		foreach(var meta in metas.Distinct()) {
			_listImgMetas.Add(meta);
			AddIconItem(_importer.GetTexture(meta));
		}
	}

	public void OnMouseEnter() => _mouseOver = true;
	public void OnMouseExit() {
		_mouseOver = false;
		var mousePosition = _tokenMap.GetGlobalMousePosition();

		if(_draggingOffList) {
			var tokens = new List<Token>();
			foreach(int idx in GetSelectedItems()) {
				var id = Guid.NewGuid();

				var token = _importer.GetToken(_listImgMetas[idx]);

				token.MapPosition = mousePosition;
				token.GlobalPosition = mousePosition;
				tokens.Add(token);

				if(_netManager.IsHost) {
					_netManager.SendToAll(
						new TokenCreatedModel(_listImgMetas[idx].Key, id, token.GlobalPosition.X, token.GlobalPosition.Y), 
						true
					);
				}

				_tokenMap[id] = token;
			}
			_selectionTool.SelectTokens(tokens); 
		}
		_draggingOffList = false;
	}
}
