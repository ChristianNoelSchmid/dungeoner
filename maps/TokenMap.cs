using System.Collections.Generic;
using System.Linq;
using Dungeoner.GodotExtensions;
using Dungeoner.Importers;
using Dungeoner.TokenManipulation;
using Dungeoner.Maps;
using Godot;

namespace Dungeoner.Maps;

public partial class TokenMap : Node2D
{	
	[Export]
	private TokenImporter _tokenImporter = default!;
	[Export]
	private SelectionTool _selectionTool = default!;

	private Dictionary<Vector2I, Tile> _tiles = new();

	public override void _Draw() {
		foreach((var tilePosition, var tile) in _tiles) {
			DrawTexture(tile.Texture, (tilePosition * Constants.GRID_SIZE) - Constants.GRID_VECTOR / 2);
		}
    }

	public void AddToken(Token token) {
		AddChild(token);
		_selectionTool.RegisterToken(token);
	}
	public void RemoveToken(Token token) {
		_selectionTool.RemoveToken(token); 
		RemoveChild(token);
	}
}