using System.Collections.Generic;
using System.Linq;
using Dungeoner.Importers;
using Godot;

namespace Dungeoner.Maps;

public partial class WallBackRenderer : WallPanelRenderer {

	private Sprite2D _border = default!;
	private Sprite2D _backPanel = default!;
	private Node2D _shadow = default!;
	private Sprite2D _leftDrywall = default!;
	private Sprite2D _rightDrywall = default!;

	public override void _Ready() {
		_border = (Sprite2D)FindChild("Border");
		_backPanel = (Sprite2D)FindChild("BackPanel");
		_shadow = (Node2D)FindChild("Shadow");

		_leftDrywall = (Sprite2D)FindChild("LeftDrywall");
		_rightDrywall = (Sprite2D)FindChild("RightDrywall");
	}

    public override void UpdateNeighborEdge(Direction direction, bool active) { 
		switch(direction) {
			case Direction.DownLeft: _leftDrywall.Visible = active; break;
			case Direction.DownRight: _rightDrywall.Visible = active; break;
		}
	}

    public override void UpdateNeighborTile(Direction direction, bool active)
    {
		switch(direction) {
			case Direction.Down: _shadow.Visible = active; break;
		}
    }
}
