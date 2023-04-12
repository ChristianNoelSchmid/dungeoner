using Dungeoner.Importers;
using Dungeoner.Maps;
using Dungeoner.TokenManipulation;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Dungeoner;

public partial class TokenList : UiList {
	private  TokenImporter _importer;
	private TokenMap _world;
	private SelectionTool _selectionTool;
	private bool _mouseOver = false;
	private bool _draggingOffList = false;
	List<TokenMeta> _listImgMetas = new();

	public override void _Ready() {
		_importer = (TokenImporter)GetNode("/root/Main/Importers/TokenImporter");
		_world = (TokenMap)GetNode("/root/Main/World");
		_selectionTool = (SelectionTool)GetNode("/root/Main/SelectionTool");
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
		if(_draggingOffList) {
			var tokens = new List<Token>();
			foreach(int idx in GetSelectedItems()) {
				var token = _importer.GetToken(_listImgMetas[idx]);
				token.GlobalPosition = _world.GetGlobalMousePosition();
				_world.AddToken(null, token);
				tokens.Add(token);
			}
			_selectionTool.SelectTokens(tokens); 
		}
		_draggingOffList = false;
	}
}
