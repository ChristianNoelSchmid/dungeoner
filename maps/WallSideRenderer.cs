using System.Collections.Generic;
using System.Linq;
using Dungeoner.Importers;
using Godot;

namespace Dungeoner.Maps;

public partial class WallSideRenderer : WallPanelRenderer {
    private Sprite2D _drywall = default!;
	private Sprite2D _leftShadow = default!;
	private Sprite2D _rightShadow = default!;

	public override void _Ready() {
        _drywall = (Sprite2D)FindChild("Drywall");
		_leftShadow = (Sprite2D)FindChild("LeftShadow");
		_rightShadow = (Sprite2D)FindChild("RightShadow");
	}

    public override void Setup(Vector2I vertex1, Vector2I vertex2, WallMeta wall) {
        base.Setup(vertex1, vertex2, wall);
    }

    public override void UpdateWall(
        IEnumerable<(Vector2I, Vector2I, bool)> edgeNeighbors, 
        IEnumerable<(Vector2I, bool)> tileNeighbors
    ) {
        _leftShadow.SetProcess(tileNeighbors.First(tile => tile.Item1 == _vertex1 - new Vector2I(-1, 0)).Item2);
		_rightShadow.SetProcess(tileNeighbors.First(tile => tile.Item1 == _vertex1).Item2);
    }
}