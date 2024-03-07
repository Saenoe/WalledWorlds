using Godot;
using System;

namespace Aberration.Dev;

public partial class FlyCam : Camera3D {

	[Export] public float Speed { get; set; } = 5.0f;
	
	public override void _Input(InputEvent ev) {
		if (ev is InputEventMouseMotion mouseMotion) {
			Rotation = new Vector3 {
				X = Math.Clamp(Rotation.X - mouseMotion.Relative.Y * 0.002f, -1.5f, 1.5f),
				Y = Rotation.Y - mouseMotion.Relative.X * 0.002f,
				Z = 0.0f,
			};
		}
	}

	public override void _PhysicsProcess(double dt) {
		Vector3 forward = GlobalBasis.Z;
		Vector3 right = GlobalBasis.X;

		Vector3 move =
			right * (Input.GetActionStrength("MoveRight") - Input.GetActionStrength("MoveLeft")) +
			forward * (Input.GetActionStrength("MoveBack") - Input.GetActionStrength("MoveForward"));




		GlobalPosition += move * Speed * (float)dt;
	}

}