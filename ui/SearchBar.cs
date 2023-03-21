using Godot;
using System;

namespace Dungeoner.Ui;

public partial class SearchBar : LineEdit
{
	[Export]
	private TokenList _tokenList;

	private bool _mouseOver = false;

	public void OnTextChange(string text) {
		_tokenList.QueryList($"**/*{Text}*", $"*{Text}*/**");
	}

	public override void _Process(double delta) {
		if(Input.IsKeyPressed(Key.Escape)) { ReleaseFocus(); }
		if(Input.IsKeyPressed(Key.Enter)) { ReleaseFocus(); }
		if(Input.IsActionJustPressed("select") && !_mouseOver) ReleaseFocus();
	}

	public void OnMouseEnter() => _mouseOver = true;
	public void OnMouseExit() => _mouseOver = false;
}