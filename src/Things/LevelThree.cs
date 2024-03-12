using Godot;
using System;

namespace Aberration;

public partial class LevelThree : Node3D {

	private TwistyCorridor _corridor;

	public override void _Ready() {
		_corridor = GetNode<TwistyCorridor>("TwistyCorridor");
	}

	public override void _PhysicsProcess(double _dt) {
		//if (!_corridor.PlayerInside) return;
		//GlobalRotation = Vector3.Zero.Rotated(_corridor.CorridorDirection, _corridor.CurrentRotation);
		//GD.Print(GlobalRotation);
	}

}
