using Godot;
using System;

namespace Aberration;

public partial class BridgeThing : Node3D {

	private Area3D _levelExit;

	public override void _Ready() {
		GetNode<Area3D>("BreakTrigger").BodyEntered += (Node3D body) => {
			if (body is not Player) return;

			GetNode<AnimationPlayer>("AnimationPlayer").Play("break");
		};
		_levelExit = GetNode<Area3D>("LevelExit");

	}

	public override void _PhysicsProcess(double _dt) {
		// doing the sane thing and using signals for this
		// would hard freeze the game without any further console output
		// thanks godot!
		if (_levelExit.OverlapsBody(GameManager.Player)) LevelTransition();
	}

	public void LevelTransition() {
		Node3D nextLevel = ResourceLoader.Load<PackedScene>("res://scenes/Areas/Lv2.tscn").Instantiate<Node3D>();

		GameManager.SwitchGameScene(
			nextLevel,
			_levelExit.GlobalTransform
		);
	}

}
