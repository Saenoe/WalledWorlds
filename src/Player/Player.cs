using System;
using System.ComponentModel;
using Godot;

namespace Aberration;

public partial class Player : CharacterBody3D {

	public enum FootstepTypeEnum {
		Grass,
		Wood,
		Concrete,
		Carpet,
		Void,
	}

	// todo move this somewhere sane
	private const float MouseSensitivity = 0.002f;

	[Export] public float WalkSpeed { get; set; } = 2.0f;
	[Export] public float SprintMultiplier { get; set; } = 2.0f;
	[Export] public float InteractRange { get; set; } = 2.0f;

	[Export] public float Fov { get; set; } = 60.0f;
	[Export] public float ZoomTime { get; set; } = 0.1f;
	[Export] public float ZoomAmount { get; set; } = 20.0f;

	[Export] public bool DebugMode { get; set; } = false;

	public Camera3D Camera { get; private set; }
	private RayCast3D _interactRay;
	private Crosshair _crosshair;

	private AnimationPlayer _animationPlayer;

	public IInteractible InteractingWith { get; set; }= null;

	private FootstepTypeEnum _currentFootstep = FootstepTypeEnum.Grass;

	private AudioStreamPlayer3D _footstepPlayer;

	private bool _wasOnFloor = false;

	private float _zoomLevel = 0.0f;

	public override void _Ready() {
		if (DebugMode) Input.MouseMode = Input.MouseModeEnum.Captured;

		Camera = GetNode<Camera3D>("Camera");

		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_footstepPlayer = GetNode<AudioStreamPlayer3D>("FootstepPlayer");
		_footstepPlayer.Stream = ResourceLoader.Load<AudioStream>($"res://assets/sounds/feet/{_currentFootstep}.tres");


		_interactRay = new RayCast3D {
			TargetPosition = Vector3.Forward * InteractRange,
			CollisionMask = 0x04, // interaction layer
			DebugShapeThickness = 1,
		};
		Camera.AddChild(_interactRay);

		_crosshair = Camera.GetNode<Crosshair>("Crosshair");

		// todo also move this somewhere sane
		GameManager.Player = this;
	}

	public override void _Input(InputEvent ev) {
		if (ev is InputEventMouseMotion mouseMotion) {
			Camera.Rotation = new Vector3 {
				X = Math.Clamp(Camera.Rotation.X - mouseMotion.Relative.Y * MouseSensitivity, -1.5f, 1.5f),
				Y = Camera.Rotation.Y - mouseMotion.Relative.X * MouseSensitivity,
				Z = 0.0f,
			};
		}
	}
	
	public override void _Process(double dt) {

		_zoomLevel = Math.Clamp(_zoomLevel + (Input.IsActionPressed("Zoom") ? (float)dt / ZoomTime : -(float)dt / ZoomTime), 0.0f, 1.0f);
		Camera.Fov = Fov - _zoomLevel * ZoomAmount;
	}

	public override void _PhysicsProcess(double dt) {

		PhysicsDirectBodyState3D state = PhysicsServer3D.BodyGetDirectState(this.GetRid());

		Vector3 gravityDirection = state.TotalGravity.Normalized();

		Vector3 forward = NormalAligned(Camera.GlobalBasis.Z, -gravityDirection).Normalized();
		Vector3 right = NormalAligned(Camera.GlobalBasis.X, -gravityDirection).Normalized();

		Vector3 move =
			right * (Input.GetActionStrength("MoveRight") - Input.GetActionStrength("MoveLeft")) +
			forward * (Input.GetActionStrength("MoveBack") - Input.GetActionStrength("MoveForward"));

		float l = move.Length();
		if (l > 1.0) move /= l;

		move *= WalkSpeed;
		if (Input.IsActionPressed("Sprint")) move *= SprintMultiplier;

		if (IsOnFloor()) {
			Velocity = move;

			float h = Velocity.Length();

			_animationPlayer.SpeedScale = h / WalkSpeed;
			
			if (!Mathf.IsZeroApprox(h) && !_animationPlayer.IsPlaying()) {
				_animationPlayer.Play("walk");
			} else if (Mathf.IsZeroApprox(h) && _animationPlayer.IsPlaying()) {
				_animationPlayer.Stop(true);
			}

			if (!_wasOnFloor) PlayFootstep();

		} else {
			if (_animationPlayer.IsPlaying()) _animationPlayer.Stop(true);
			Velocity += state.TotalGravity * (float)dt;
		}


		
		if (!_animationPlayer.IsPlaying()) {
			Camera.Position = Camera.Position.MoveToward(new Vector3(0.0f, 1.6f, 0.0f), (float)dt * 0.5f);
		}

		_wasOnFloor = IsOnFloor();

		MoveAndSlide();

		var collision = GetLastSlideCollision();
		if (collision != null) {
			Node collider = (Node)collision.GetColliderShape();
			var newFootstep = (FootstepTypeEnum)(int)collider.GetMeta("FootstepType", (int)FootstepTypeEnum.Grass);
			if (newFootstep != _currentFootstep) {
				_currentFootstep = newFootstep;
				_footstepPlayer.Stream = ResourceLoader.Load<AudioStream>($"res://assets/sounds/feet/{_currentFootstep}.tres");
			}
		}

		// vile
		if (_interactRay.GetCollider() is IInteractible interactible) {
			_crosshair.Active = true;
			
			if (Input.IsActionJustPressed("Interact")) {

				if (InteractingWith != null && InteractingWith != interactible) {
					InteractingWith.InteractRelease();
				}

				InteractingWith = interactible;
				interactible.InteractPress();
			} else
			if (Input.IsActionJustReleased("Interact")) {
				InteractingWith = null;
				interactible.InteractRelease();
			}
		} else {
			_crosshair.Active = false;

			if (InteractingWith != null) {
				InteractingWith.InteractRelease();
				InteractingWith = null;
			}
		}
	}

	public void PlayFootstep() {
		_footstepPlayer.Play();
	}

	// for singals
	public void SetWalkSpeed(float speed) => WalkSpeed = speed;

	private static Vector3 NormalAligned(Vector3 vec, Vector3 normal) {
		float d = vec.Dot(normal);

		return vec - normal * d;
	}

}
