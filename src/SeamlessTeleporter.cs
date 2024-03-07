using Godot;
using System;

namespace Aberration;

public partial class SeamlessTeleporter : Node3D {

	[Signal] public delegate void TeleportedEventHandler(Node3D node);

	[Export] public Node3D ExitNode { get; set; }
	[Export] public float PositionScale { get; set; } = 1.0f;


	public void Teleport(Node3D node) {
		Transform3D t =  this.GlobalTransform.Inverse() * node.GlobalTransform;

		t.Origin *= PositionScale;
		t = ExitNode.GlobalTransform * t;

		node.GlobalTransform = t;

		if (node is Player player) player.WalkSpeed *= PositionScale;

		EmitSignal(SignalName.Teleported, node);
	}


}
