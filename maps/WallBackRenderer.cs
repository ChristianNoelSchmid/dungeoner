using System.Collections.Generic;
using System.Linq;
using Dungeoner.Importers;
using Godot;

namespace Dungeoner.Maps;

public partial class WallBackRenderer : WallPanelRenderer {

	private Sprite2D _border = default!;
	private Sprite2D _backPanel = default!;
	private Node2D _shadow = default!;
	private Sprite2D _topDrywall = default!;
	private Sprite2D _leftDrywall = default!;
	private Sprite2D _rightDrywall = default!;

	int _leftCounter = 0;
	int _rightCounter = 0;

	public override void _Ready() {
		_border = (Sprite2D)FindChild("Border");
		_backPanel = (Sprite2D)FindChild("BackPanel");
		_shadow = (Node2D)FindChild("Shadow");

		_topDrywall = (Sprite2D)FindChild("TopDrywall");
		_leftDrywall = (Sprite2D)FindChild("LeftDrywall");
		_rightDrywall = (Sprite2D)FindChild("RightDrywall");
	}

    public override void UpdateWallMeta(WallMeta meta, Vector2I gridPosition) {
		_backPanel.Texture = meta.GetTexture(gridPosition);
		_leftDrywall.Modulate = _rightDrywall.Modulate = _topDrywall.Modulate = meta.DrywallColor;
    }

    public override void UpdateNeighborEdge(Direction direction, bool active) { 
		if(direction == Direction.DownLeft) _leftDrywall.Visible = active;
		if(direction == Direction.DownRight) _rightDrywall.Visible = active;

		UpdateTileShadowing();
	}

    public override void UpdateNeighborTile(Direction direction, bool active) {
		switch(direction) {
			case Direction.Down: _shadow.Visible = active; break;
		}
    }

	private void UpdateTileShadowing() {
		Vector2 position = new(0.5f, 0.0f);
		Vector2 scale = new(27, 1);

		if(_leftDrywall.Visible) {
			position.X += 1.0f;
			scale.X -= 2.0f;
		}
		if(_rightDrywall.Visible) {
			scale.X -= 2.0f;
			position.X -= 1.0f;
		}

		_shadow.Position = position;
		_shadow.Scale = scale;
	}
}
