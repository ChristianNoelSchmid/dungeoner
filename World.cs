using System.Collections.Generic;
using Dungeoner.TokenManipulation;
using Godot;

namespace Dungeoner;

public partial class World : Node2D
{
	[Export]
	private TokenImporter _tokenImporter;
	[Export]
	private SelectionTool _selectionTool;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//var metas = _tokenImporter.GetAllMatchingMetas("**/*");
		/*foreach(var meta in metas) {
			var token = _tokenImporter.GetToken(meta);
			_selectionTool.RegisterToken(token);
			AddChild(token);
		}*/
	}

	public void AddToken(Token token) {
		AddChild(token);
		_selectionTool.RegisterToken(token);
	}

	public void RemoveToken(Token token) => RemoveChild(token);
}
