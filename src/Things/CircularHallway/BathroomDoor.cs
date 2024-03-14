using Godot;
using System;

namespace Aberration;

public partial class BathroomDoor : AnimatedDoor {

	[Export] public float LockChance { get; set; } = 0.2f;

	public override void _Ready() {

		RandomNumberGenerator rng = new();
		Locked = rng.Randf() < LockChance;
		
		base._Ready();
	}

}
