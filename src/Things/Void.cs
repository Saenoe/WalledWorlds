using Godot;
using System;

namespace Aberration;

public partial class Void : Node3D {

	private Node3D _theGuy;
	private Node3D _theCoolHouse;
	private AudioStreamPlayer _clickOnSound;
	private AudioStreamPlayer _clickOffSound;

	private Node3D _nextScene;

	public void InitiateVoidening() {
		GameManager.WorldEnvironment.Environment.BackgroundMode = Godot.Environment.BGMode.Color;

		_theGuy = ResourceLoader.Load<PackedScene>("res://scenes/TheGuy.tscn").Instantiate<Node3D>();
		_clickOnSound = GetNode<AudioStreamPlayer>("ClickOnSound");
		_clickOffSound = GetNode<AudioStreamPlayer>("ClickOffSound");

		SceneTreeTimer timer = GetTree().CreateTimer(10.0);
		timer.Timeout += () => {
			var poolBottomOnScreenNotifier = GetNode<VisibleOnScreenNotifier3D>("PoolBottomOnScreenNotifier");
			if (!poolBottomOnScreenNotifier.IsOnScreen()) {
				SummonTheGuy();
			} else {
				poolBottomOnScreenNotifier.Connect(
					VisibleOnScreenNotifier3D.SignalName.ScreenExited,
					Callable.From(SummonTheGuy),
					(int)ConnectFlags.OneShot
				);
			}
			
		};
	}

	public void SummonTheGuy() {
		_nextScene = ResourceLoader.Load<PackedScene>("res://scenes/Areas/Lv4.tscn").Instantiate<Node3D>();

		GetParent().GetNode("ThePanopticon").GetNode<Node3D>("PoolBottom").Rotate(Vector3.Right, MathF.PI);
		

		AddChild(_theGuy);
		PositionInfront(_theGuy, 50.0f);
		_clickOnSound.Play();

		
		_theGuy.GetNode<Area3D>("CloseArea").Connect(Area3D.SignalName.BodyEntered, Callable.From((Node3D _) => {
			_theGuy.GetNode<AnimationPlayer>("AnimationPlayer").Play("fall_over_lmao");
			DeleteTheGuy();
		}), (int)ConnectFlags.OneShot);
		
	}

	public void DeleteTheGuy() {
		SceneTreeTimer timer = GetTree().CreateTimer(10.0);
		timer.Timeout += () => {
			RemoveChild(_theGuy);
			_theGuy.QueueFree();
			_clickOffSound.Play();

			SummonTheCoolHouse();
		};
	}

	public void SummonTheCoolHouse() {
		_theCoolHouse = ResourceLoader.Load<PackedScene>("res://scenes/VoidHouse.tscn").Instantiate<Node3D>();

		SceneTreeTimer timer = GetTree().CreateTimer(2.0);
		timer.Timeout += () => {
			AddChild(_theCoolHouse);
			PositionInfront(_theCoolHouse, 50.0f);
			_clickOnSound.Play();

			_theCoolHouse.GetNode<PhysicalButton>("TheDoor").Pressed += () => {

				GameManager.SwitchGameScene(_nextScene, _theCoolHouse.GetNode<Node3D>("LevelExit").GlobalTransform);

			};
		};
	}

	private static void PositionInfront(Node3D node, float distance) {
		Vector3 cameraDir = GameManager.Player.Camera.GlobalBasis.Z;
		cameraDir.Y = 0.0f;
		cameraDir = cameraDir.Normalized();
		
		node.GlobalPosition = GameManager.Player.GlobalPosition + new Vector3(-cameraDir.X, 0, -cameraDir.Z) * distance;
		node.GlobalRotation = new Vector3(0.0f, GameManager.Player.Camera.GlobalRotation.Y, 0.0f);
		GameManager.Player.Camera.GlobalRotation *= new Vector3(0, 1, 1);
	}

	public void SplishySplashy() {
		GetNode<AudioStreamPlayer>("SplishySplashy").Play();
	}

}
