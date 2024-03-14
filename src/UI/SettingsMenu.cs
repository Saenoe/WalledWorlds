using Aberration;
using Godot;
using System;

public partial class SettingsMenu : Control {

	[Export] public float MinMouseSensitivity { get; set; } = 0.0005f;
	[Export] public float MaxMouseSensitivity { get; set; } = 0.01f;

	public override void _Ready() {
		UpdateMenu();
	}

	public void ChangeMouseSensitivity(float value) {
		GameManager.MouseSensitivity = Mathf.Lerp(MinMouseSensitivity, MaxMouseSensitivity, value); 
		GameManager.SaveSettings();
	}

	private void UpdateVsync(bool enabled) {
		DisplayServer.WindowSetVsyncMode(enabled ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
		GameManager.SaveSettings();
	}

	private void SetFullscreen(bool enabled) {
		DisplayServer.WindowSetMode(enabled ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
		GameManager.SaveSettings();
	}

	private void UpdateMenu() {
		GetNode<Slider>("MouseSensitivitySlider").SetValueNoSignal(
			(GameManager.MouseSensitivity - MinMouseSensitivity) / (MaxMouseSensitivity - MinMouseSensitivity)
		);
		GetNode<Slider>("VolumeSlider").SetValueNoSignal(
			Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master")))
		);

		GetNode<CheckBox>("VSyncCheckbox").ButtonPressed = DisplayServer.WindowGetVsyncMode() != DisplayServer.VSyncMode.Disabled;
		GetNode<CheckBox>("FullscreenCheckbox").ButtonPressed = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;

	}

	private void SetVolume(float value) {
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), Mathf.LinearToDb(value));
		GameManager.SaveSettings();
	}

	public override void _Notification(int what) {
		if (what != NotificationVisibilityChanged) return;

		UpdateMenu();
	}

}
