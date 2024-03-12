using Godot;

namespace Aberration;

public partial class EndScene : Node3D {

	public override void _Ready() {

		GetNode<AnimationPlayer>("AnimationPlayer").Play("transition");

	}

}
