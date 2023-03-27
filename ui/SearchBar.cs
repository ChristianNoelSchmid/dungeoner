using Godot;
using System;

namespace Dungeoner.Ui;

public partial class SearchBar : LineEdit
{
	[Export]
	private UiList _list;

	private bool _mouseOver = false;

	public void OnTextChange(string text) {
		_list.QueryList($"**/*{Text}*", $"*{Text}*/**");
	}

	public override void _Process(double delta) {
		if(Input.IsKeyPressed(Key.Escape)) { ReleaseFocus(); }
		if(Input.IsKeyPressed(Key.Enter)) { ReleaseFocus(); }
		if(Input.IsActionJustPressed("select") && !_mouseOver) ReleaseFocus();
	}

	public void OnMouseEnter() => _mouseOver = true;
	public void OnMouseExit() => _mouseOver = false;
}