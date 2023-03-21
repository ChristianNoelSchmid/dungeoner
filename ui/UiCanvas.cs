using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeoner.Ui;

public partial class UiCanvas : MarginContainer 
{
	private bool _uiFocused = false;
	public bool UiFocused => _uiFocused;

	private List<Control> _allNodes;

    public override void _Ready() {
		_allNodes = new();

		var queue = new List<Control>();
		var set = new HashSet<Control>();

		queue.Add(this);
		while(queue.Count > 0) {
			var control = queue[0];
			queue.RemoveAt(0);

			var children = control.GetChildren().Where(node => node is Control).Select(node => (Control)node);

			_allNodes.AddRange(children);
			queue.AddRange(children);
		}
    }

    public override void _Process(double delta) {
		_uiFocused = _allNodes.Any(node => node.HasFocus());
    }
}
