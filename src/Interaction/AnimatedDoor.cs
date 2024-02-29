using Godot;

namespace Aberration;

public partial class AnimatedDoor : AnimatableBody3D {

	[Export] public AnimationPlayer AnimationPlayer { get; set; }

	[Export] public string OpenAnimation { get; set; }
	[Export] public string CloseAnimation { get; set; }


	public void Open() {
		AnimationPlayer.Play(OpenAnimation);
	}
	public void Close() {
		AnimationPlayer.Play(CloseAnimation);
	}

}