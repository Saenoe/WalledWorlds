using Godot;
using System;

namespace Aberration;

public partial class OfficeBuildingBandaid : Area3D {

	[Export] public float NewBrightness { get; set; } = 1.5f;
	[Export] public float AdjustSpeed { get; set; } = 0.5f;

	private float _brightness;
	private bool _playerInside = false;

	private float _timeInside;
	

	public override void _Ready() {
		_brightness = GameManager.WorldEnvironment.Environment.AdjustmentBrightness;

		BodyEntered += (body) => {
			if (body is not Player) return;

			_playerInside = true;
		};

		BodyExited += (body) => {
			if (body is not Player) return;
			_playerInside = false;
		};

	}


	public override void _Process(double dt) {
		float h = _timeInside;

		if (_playerInside) {
			_timeInside += (float)dt / AdjustSpeed;
		} else {
			_timeInside -= (float)dt / AdjustSpeed;
		}
		_timeInside = Math.Clamp(_timeInside, 0.0f, 1.0f);

		if (h == _timeInside) return;

		GameManager.WorldEnvironment.Environment.AdjustmentBrightness = Mathf.Lerp(_brightness, NewBrightness, _timeInside);

	}

}
