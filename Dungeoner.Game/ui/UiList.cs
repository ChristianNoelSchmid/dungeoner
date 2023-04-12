using Dungeoner.Collections;
using Godot;
using System;

public abstract partial class UiList : ItemList
{
	public abstract void QueryList(params string[] globs);
}
