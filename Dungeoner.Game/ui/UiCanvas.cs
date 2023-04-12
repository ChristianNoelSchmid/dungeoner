using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeoner.Ui;

public partial class UiCanvas : MarginContainer 
{
	public bool UiFocused => _hoveredControls.Count > 0 && _hoveredControls.Any(c => c.HasFocus());

	private List<Control> _allControls = default!;
	private HashSet<Control> _hoveredControls = default!;

    public override void _Ready() {
		_allControls = new();
		_hoveredControls = new();

		var queue = new List<Control>();
		var set = new HashSet<Control>();

		queue.Add(this);
		while(queue.Count > 0) {
			var control = queue[0];
			queue.RemoveAt(0);

			var children = control.GetChildren().Where(node => node is Control).Select(node => (Control)node);

			_allControls.AddRange(children);
			queue.AddRange(children);
		}

		foreach(var control in _allControls) {
			control.MouseEntered += () => _hoveredControls.Add(control);
			control.MouseExited += () => _hoveredControls.Remove(control);
		}
    }
}
