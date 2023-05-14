using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeoner.Ui;

public partial class UiMain : MarginContainer
{
    [Export] private Control _mainPanel = default!;
    [Export] private TabBar _mainTabBar = default!;
    [Export] private Control[] _mainPanels = default!;
    [Export] private Control _mapMenu = default!;

    private List<bool> _enabledPanels = default!;

    public bool IsFocused => _hoveredControls.Count > 0 && _hoveredControls.Any(c => c.HasFocus());

    private List<Control> _allControls = default!;
    private HashSet<Control> _hoveredControls = default!;

    public override void _Ready()
    {
        _allControls = new();
        _hoveredControls = new();

        var queue = new List<Control>();
        var set = new HashSet<Control>();

        _enabledPanels = Enumerable.Repeat(true, _mainPanels.Length).ToList();

        queue.Add(this);
        while (queue.Count > 0)
        {
            var control = queue[0];
            queue.RemoveAt(0);

            var children = control.GetChildren().Where(node => node is Control).Select(node => (Control)node);

            _allControls.AddRange(children);
            queue.AddRange(children);
        }

        foreach (var control in _allControls)
        {
            control.MouseEntered += () => _hoveredControls.Add(control);
            control.MouseExited += () => _hoveredControls.Remove(control);
        }
    }

    public void EnableSection(UiPanel section, bool isEnabled)
    {
        _enabledPanels[(int)section] = isEnabled;
        _mainPanel.Visible = _enabledPanels.Any(enb => enb);

        if (_mainPanel.Visible && _mainTabBar.CurrentTab == (int)section)
            _mainTabBar.CurrentTab = _enabledPanels.FindIndex(enb => enb);

        for (int i = 0; i < _enabledPanels.Count; i += 1)
        {
            _mainTabBar.SetTabHidden(i, !_enabledPanels[i]);
        }
    }
}

public enum UiPanel
{
    Tokens,
    Map
}