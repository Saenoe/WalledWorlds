using Godot;
using System;
using System.Collections.Generic;

namespace Aberration;

public partial class OfficeMaze : Node3D {

	private PackedScene _cubicleScene;
	private PackedScene _floorScene;

	private Node3D _cubiclesRoot;
	private readonly List<Cubicle> _cubicles = new();

	public override void _Ready() {
		_cubiclesRoot = GetNode<Node3D>("CubiclesRoot");

		_cubicleScene = ResourceLoader.Load<PackedScene>("res://scenes/OfficeMaze/Cubicle.tscn");
		_floorScene = ResourceLoader.Load<PackedScene>("res://scenes/OfficeMaze/Floor.tscn");

	
		RandomNumberGenerator rng = new();
		for (int y = 0; y < 17; ++y) {
			for (int x = 0; x < 26; ++x) {

				Cubicle cubicle = _cubicleScene.Instantiate<Cubicle>();
				cubicle.Position = new Vector3(-x, 0, -y) * 2;
				cubicle.Rng = rng;

				_cubiclesRoot.AddChild(cubicle);
				_cubicles.Add(cubicle);
				
			}
		}
	}

	public void EngageCoolDoorSequence() {
		var exitOnScreenNotifier = GetNode<VisibleOnScreenNotifier3D>("ExitOnScreenNotifier");
		var exitSwitcheroo = GetNode<Switcheroo>("ExitSwitcheroo");
		if (!exitOnScreenNotifier.IsOnScreen()) {
			exitSwitcheroo.Switch();
		} else {
			exitOnScreenNotifier.Connect(
				VisibleOnScreenNotifier3D.SignalName.ScreenExited,
				Callable.From(exitSwitcheroo.Switch),
				(int)ConnectFlags.OneShot
			);
				
		}

		var entranceOnScreenNotifier = GetNode<VisibleOnScreenNotifier3D>("EntranceOnScreenNotifier");
		var entranceSwitcheroo = GetNode<Switcheroo>("EntranceSwitcheroo");
		if (!entranceOnScreenNotifier.IsOnScreen() && !CameraBehind(entranceOnScreenNotifier)) {
			entranceSwitcheroo.Switch();
		} else {
			entranceOnScreenNotifier.ScreenExited += TryCloseEntranceDoor;
		} 
	}

	public void TryCloseEntranceDoor() {
		var entranceOnScreenNotifier = GetNode<VisibleOnScreenNotifier3D>("EntranceOnScreenNotifier");
		if (CameraBehind(entranceOnScreenNotifier)) return;

		GetNode<Switcheroo>("EntranceSwitcheroo").Switch();

		entranceOnScreenNotifier.ScreenExited -= TryCloseEntranceDoor;
	}

	private bool CameraBehind(Node3D node) {
		Camera3D camera = GetViewport().GetCamera3D();

		return new Plane(node.GlobalBasis.Z, node.GlobalPosition).DistanceTo(camera.GlobalPosition) > 0;
	}

	public void EngageDisappearMode() {
		foreach (var cubicle in _cubicles) {
			if (!cubicle.HasCubicle) _cubiclesRoot.RemoveChild(cubicle);
		}
		_cubicles.RemoveAll(c => !c.HasCubicle);
		if (_cubicles.Count == 0) EngageCoolDoorSequence();

		foreach (var cubicle in _cubicles) {
			cubicle.CubicleChance = 0.0f;
			cubicle.GetNode<VisibleOnScreenNotifier3D>("OnScreenNotifier").ScreenExited += () => {
				

				_cubicles.Remove(cubicle);
				_cubiclesRoot.RemoveChild(cubicle);
				if (_cubicles.Count == 0) {
					EngageCoolDoorSequence();
				}
			};
		}


		GetNode<Node3D>("LightCone").Visible = false;
		GetNode<AudioStreamPlayer3D>("LightCone/AudioStreamPlayer3D").Play();
		GetNode<AudioStreamPlayer3D>("LightCone/AudioStreamPlayer3D2").Play();
	}
}
