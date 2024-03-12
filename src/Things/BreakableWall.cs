using Godot;

namespace Aberration;

public partial class BreakableWall : Node3D {

	[Export]
	public AudioStream BreakSound { get; set; } 

	private PackedScene _behindWallScene;

	public override void _Ready() {
		_behindWallScene = ResourceLoader.Load<PackedScene>("res://scenes/OfficeMaze/OfficeMaze.tscn");
	}

	public void Break() {

		var player = new AudioStreamPlayer3D {
			Stream = BreakSound,
			AttenuationFilterCutoffHz = 10000,
			Autoplay = true,
		};

		AddChild(player);

		GetNode<Switcheroo>("Switcheroo").Switch();
		GetNode<Switcheroo>("CollisionSwitcheroo").Switch();

		Node3D fuck = _behindWallScene.Instantiate<Node3D>();
		AddChild(fuck);

	}


}
