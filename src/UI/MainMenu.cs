using Godot;
using System;

namespace Aberration;

public partial class MainMenu : Control {

	public void CommenceEpicGaming() {
		GameManager.StartGame();
	}

	public void ExitGame() {
		GetTree().Quit();	
	}

}
