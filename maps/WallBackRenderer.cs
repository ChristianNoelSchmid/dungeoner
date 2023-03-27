using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dungeoner.Importers;
using Godot;

namespace Dungeoner.Maps;

public partial class WallBackRenderer : WallPanelRenderer {

	private Sprite2D _backPanel = default!;
	private Sprite2D _tileShadow = default!;
	private Sprite2D _leftDrywall = default!;
	private Sprite2D _rightDrywall = default!;

	public override void _Ready() {
		_backPanel = (Sprite2D)FindChild("BackPanel");
		_tileShadow = (Sprite2D)FindChild("TileShadow");

		_leftDrywall = (Sprite2D)FindChild("LeftDrywall");
		_rightDrywall = (Sprite2D)FindChild("RightDrywall");
	}
	public override void Setup(Vector2I vertex1, Vector2I vertex2, WallMeta wall) {
		base.Setup(vertex1, vertex2, wall);
		_backPanel.Texture = wall.GetTexture(Position.ToGridPosition());
	}
    public override void UpdateWall(
		IEnumerable<(Vector2I, Vector2I, bool)> edgeNeighbors, 
		IEnumerable<(Vector2I, bool)> tileNeighbors
	) {
		var bottomLeft = edgeNeighbors.First(
			edge => edge.Item1 == _vertex1 && edge.Item2 == _vertex1 + new Vector2I(0, 1)
		).Item3;
		var bottomRight = edgeNeighbors.First(
			edge => edge.Item1 == _vertex2 && edge.Item2 == _vertex2 + new Vector2I(0, 1)
		).Item3;
		var bottom = tileNeighbors.First(tile => tile.Item1 == _vertex1).Item2;

		_leftDrywall.SetProcess(bottomLeft);
		_rightDrywall.SetProcess(bottomRight);
		_tileShadow.SetProcess(bottom);
    }
}
