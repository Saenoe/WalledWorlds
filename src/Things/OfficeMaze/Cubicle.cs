using Godot;
using System;

namespace Aberration;

public partial class Cubicle : Node3D {

	[Export] public float CubicleChance { get; set; } = 0.25f;

	public bool HasCubicle => _cubicle.Visible;

	public RandomNumberGenerator Rng { get; set; }

	private Node3D _cubicle;

	public override void _Ready() {
		_cubicle = GetNode<Node3D>("Cubicle");
		Randomize();
	}

	public void Randomize() {

		bool hasCubicle = Rng.Randf() < CubicleChance;
		if (hasCubicle) {
			_cubicle.ProcessMode = ProcessModeEnum.Inherit;
			_cubicle.Visible = true;

			_cubicle.Transform = Transform3D.Identity
				.Rotated(Vector3.Up, (Rng.Randi() % 4) * MathF.PI / 2.0f);
		} else {
			_cubicle.ProcessMode = ProcessModeEnum.Disabled;
			_cubicle.Visible = false;
		}

	}


}
