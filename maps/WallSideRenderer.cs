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

    public override void UpdateWallMeta(WallMeta meta, Vector2I gridPosition) { 
		_drywall.Modulate = meta.DrywallColor; 
	}

    public override void UpdateNeighborEdge(Direction direction, bool active) { }

    public override void UpdateNeighborTile(Direction direction, bool active) {
		switch(direction) {
			case Direction.Right: _rightShadow.Visible = active; break;
			case Direction.Left: _leftShadow.Visible = active; break;
		}
    }
}
