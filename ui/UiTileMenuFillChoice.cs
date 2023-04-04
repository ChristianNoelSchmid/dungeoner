using Dungeoner.Painters;
using Godot;
using System;

public partial class UiTileMenuFillChoice : ItemList {
	[Export]
	private TilePainter _painter = default!;
	private bool _mouseOver = false;

	public override void _Ready() {
		_painter = (TilePainter)GetNode("/root/Main/TilePainter");
		Select(0, true);
	}
	public void OnChoiceChange(int idx) { }
	public void OnMouseEnter() => _mouseOver = true;
	public void OnMouseExit() {
		_mouseOver = false;
		ReleaseFocus();
	}
}
