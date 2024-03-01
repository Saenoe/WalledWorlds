using System.Diagnostics;
using Godot;

namespace Aberration;

public partial class AnimatedDoor : AnimatableBody3D, IInteractible {

	public enum StateEnum {
		Open,
		Opening,
		Closed,
		Closing,
	}

	[Signal] public delegate void OpeningEventHandler(bool success);
	[Signal] public delegate void OpenedEventHandler();
	[Signal] public delegate void ClosingEventHandler(bool success);
	[Signal] public delegate void ClosedEventHandler();

	[Export] public AnimationPlayer AnimationPlayer { get; set; }

	[Export] public string OpenAnimation { get; set; }
	[Export] public string CloseAnimation { get; set; }
	[Export] public string LockedAnimation { get; set; }

	[Export] public bool Locked { get; set; }

	[Export] public bool StartOpen { get; set; }

	[Export] public bool Interruptible { get; set; } = true;

	public bool InMotion { get => State == StateEnum.Opening || State == StateEnum.Closing; }

	public StateEnum State { get; set; } = StateEnum.Closed;


	public override void _Ready() {
		AnimationPlayer.AnimationFinished += OnAnimationFinished;

		if (StartOpen) {
			AnimationPlayer.Play(OpenAnimation, -1.0, 1.0f, true);
			State = StateEnum.Open;
		}
	}

	public void Open(bool obeyLock = true) {
		if (!Interruptible && InMotion) return;
		
		EmitSignal(SignalName.Opening, Locked && obeyLock);
		if (Locked && obeyLock) {
			AnimationPlayer.Play(LockedAnimation);
			return;
		}

		AnimationPlayer.Play(OpenAnimation);
		State = StateEnum.Opening;
	}
	public void Close() {
		if (!Interruptible && InMotion) return;
		
		EmitSignal(SignalName.Closing);

		AnimationPlayer.Play(CloseAnimation);
		State = StateEnum.Closing;
	}
	public void Toggle(bool obeyLock = true) {
		if (State == StateEnum.Closed || State == StateEnum.Closing) Open();
		else Close();
	}

	public void OnAnimationFinished(StringName animName) {
		if (animName == OpenAnimation) {
			EmitSignal(SignalName.Opened);
			State = StateEnum.Open;
		}
		else if (animName == CloseAnimation) {
			EmitSignal(SignalName.Closed);
			State = StateEnum.Closed;
		}
	}


	public void InteractPress() {
		Toggle();
	}
	public void InteractRelease() {}

}