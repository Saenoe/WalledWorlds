using Godot;
using System;

namespace Abberation;

public partial class Player : CharacterBody3D {

	// todo move this somewhere sane
	private const float MouseSensitivity = 0.002f;

	[Export]
	public float WalkSpeed { get; set; } = 10.0f;

	private Camera3D _camera;

	public override void _Ready() {

		Input.MouseMode = Input.MouseModeEnum.Captured;

		_camera = GetNode<Camera3D>("Camera");
	}

	public override void _Input(InputEvent ev) {
		if (ev is InputEventMouseMotion mouseMotion) {
			_camera.Rotation += new Vector3 {
				X = -mouseMotion.Relative.Y * MouseSensitivity,
				Y = -mouseMotion.Relative.X * MouseSensitivity,
				Z = 0.0f,
			};
		}
	}

	public override void _PhysicsProcess(double dt) {

		PhysicsDirectBodyState3D state = PhysicsServer3D.BodyGetDirectState(this.GetRid());

		Vector3 gravityDirection = state.TotalGravity.Normalized();

		Vector3 forward = NormalAligned(_camera.Basis.Z, -gravityDirection);
		Vector3 right = NormalAligned(_camera.Basis.X, -gravityDirection);

		Vector3 move =
			right * (Input.GetActionStrength("MoveRight") - Input.GetActionStrength("MoveLeft")) +
			forward * (Input.GetActionStrength("MoveBack") - Input.GetActionStrength("MoveForward"));

		float l = move.Length();
		if (l > 1.0) move /= l;

		move *= WalkSpeed;

		Velocity = move;

		MoveAndSlide();

	}

	private static Vector3 NormalAligned(Vector3 vec, Vector3 normal) {
		float d = vec.Dot(normal);

		return vec - normal * d;
	}

}
