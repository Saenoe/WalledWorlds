using System;
using Godot;
namespace Aberration;

public partial class SlowdownArea : Area3D {

	public enum ExitBehaviourEnum {
		Reset,
		ApplyNew,
		ApplyClosest,
		DoNothing,
	}

	[Export] public float NewSpeed { get; set; } = 2.0f;

	[Export] public ExitBehaviourEnum ExitBehaviour { get; set; } = ExitBehaviourEnum.ApplyNew;

	[Export] public Curve Curve { get; set; }

	private float _playerOrigSpeed;
	private float _entryDistance;
	private Player _playerInside = null;

	public override void _Ready() {
		BodyEntered += OnBodyEnter;
		BodyExited += OnBodyExit;
	}

	public override void _PhysicsProcess(double _dt) {
		if (_playerInside == null) return;


		_playerInside.WalkSpeed = InterpolateSpeed();
		GD.Print(_playerInside.WalkSpeed);
	}

	public void OnBodyEnter(Node3D node) {
		if (node is Player player) {
			_playerInside = player;
			_playerOrigSpeed = player.WalkSpeed;
			_entryDistance = GlobalPosition.DistanceTo(player.GlobalPosition);
		}
	}

	public void OnBodyExit(Node3D node) {
		if (node == _playerInside) {
			switch (ExitBehaviour) {
				case ExitBehaviourEnum.Reset: _playerInside.WalkSpeed = _playerOrigSpeed; break;
				case ExitBehaviourEnum.ApplyNew: _playerInside.WalkSpeed = NewSpeed; break;
				case ExitBehaviourEnum.ApplyClosest: _playerInside.WalkSpeed = MathF.Round(InterpolateSpeed()); break;
				case ExitBehaviourEnum.DoNothing: break;
			}

			_playerInside = null;
		}
	}

	private float InterpolateSpeed() {
		float nd = 1.0f - Math.Min(GlobalPosition.DistanceTo(_playerInside.GlobalPosition) / _entryDistance, 1.0f);

		float t = Math.Clamp(Curve.Sample(nd), 0.0f, 1.0f);
		return Mathf.Lerp(NewSpeed, _playerOrigSpeed, t);
	}

}
