using Godot;
using System;
using System.Linq;

namespace Aberration;

public partial class WallBreaker9000 : Node {

	[Export]
	public Godot.Collections.Array<BreakableWall> Walls { get; set; }

	public void CommenceBreakage(Node3D body) {
		if (body is not Player) return;
		Player player = (Player)body;

		Vector3 faceDirection = player.Camera.GlobalBasis.Z;
		faceDirection.Y = 0.0f;
		faceDirection = faceDirection.Normalized();

		Plane cameraPlane = new(
			-faceDirection,
			player.Camera.GlobalPosition
		);

		BreakableWall mostEpicestWall = null;
		float mostEpicestDistance = float.MaxValue;
		foreach (var wall in Walls) {

			float distance = cameraPlane.DistanceTo(wall.GlobalPosition);

			if (distance < mostEpicestDistance) {
				mostEpicestDistance = distance;
				mostEpicestWall = wall;
			}
		}

		mostEpicestWall.Break();


	}

}
