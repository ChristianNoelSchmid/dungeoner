using Godot;
using System;

public partial class MainTabBar : Godot.TabBar {
	[Export]
	private Control TokenMenu;
	[Export]
	private Control MapMenu;

	public void OnTabChange(int idx) {
		switch(idx) {
			case 0: 
				TokenMenu.Show();
				MapMenu.Hide();
				break;
			case 1:
				TokenMenu.Hide();
				MapMenu.Show();
				break;
		}
	}
}
