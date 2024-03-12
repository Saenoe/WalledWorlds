using Aberration;
using Godot;
using System;

namespace Aberration; 

public partial class PauseMenu : Control {

	public void OnResumePress() {
		GameManager.StartGame();
	}

	public void OnExitToMenuPress() {
		GameManager.ReturnToMenu();
	}

	public void OnExitGamePress() {
		GetTree().Quit();
	}

}
