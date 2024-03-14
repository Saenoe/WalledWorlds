using Godot;
using System;

namespace Aberration;

public partial class TwistyCorridor : Node3D {

	private Area3D _twistArea;
	private Node3D _twistStart;
	private Node3D _twistEnd;

	private Basis _playerOrigBasis;
	private bool _firstEntry = false;

	private bool _playerInside = false;

	public override void _Ready() {
		_twistArea = GetNode<Area3D>("TwistArea");
		_twistStart = GetNode<Node3D>("TwistStart");
		_twistEnd = GetNode<Node3D>("TwistEnd");

		_twistArea.BodyEntered += (Node3D body) => {
			if (body is not Player) return; 
			_playerInside = true;
			if (!_firstEntry) {
				_firstEntry = true;
				_playerOrigBasis = GameManager.Player.GlobalBasis;
			}
		};
		_twistArea.BodyExited += (Node3D body) => { if (body is Player) _playerInside = false; };
	}

	public override void _PhysicsProcess(double _dt) {
		if (!_playerInside) return;

		Vector3 corridorAxis = _twistStart.GlobalPosition.DirectionTo(_twistEnd.GlobalPosition);

		Plane entryPlane = new(corridorAxis, _twistStart.GlobalPosition);

		float corridorLength = entryPlane.DistanceTo(_twistEnd.GlobalPosition);
		float playerDepth = Math.Clamp(entryPlane.DistanceTo(GameManager.Player.GlobalPosition) / corridorLength, 0.0f, 1.0f);


		float rotation = Mathf.Lerp(_twistStart.GlobalRotation.Z, _twistEnd.GlobalRotation.Z, playerDepth);

		Vector3 gravity = Vector3.Down.Rotated(corridorAxis, rotation);

		PhysicsServer3D.AreaSetParam(GetWorld3D().Space, PhysicsServer3D.AreaParameter.GravityVector, gravity);

		GameManager.Player.GlobalBasis = _playerOrigBasis.Rotated(corridorAxis, rotation);
		GameManager.Player.UpDirection = -gravity;
	}

	public void StartLevelTransition() {
		var player = GetNode<AnimationPlayer>("AnimationPlayer");
		player.AnimationFinished += (_) => DoLevelTransition(); 
		player.Play("close_bars");
		
	}
	public void DoLevelTransition() {
		Node3D nextLevel = ResourceLoader.Load<PackedScene>("res://scenes/Areas/Lv3.tscn").Instantiate<Node3D>();
		GameManager.Player.UpDirection = Vector3.Up;

		PhysicsServer3D.AreaSetParam(GetWorld3D().Space, PhysicsServer3D.AreaParameter.GravityVector, Vector3.Down);

		GameManager.WorldEnvironment.Environment.BackgroundMode = Godot.Environment.BGMode.Color;
		GameManager.WorldEnvironment.Environment.BackgroundColor = new Color(0, 0, 0);
		GameManager.SwitchGameScene(nextLevel, GetNode<Node3D>("ThePanopticon/SpookyDoorEntry").GlobalTransform);
	}

}