using Godot;

namespace Aberration;

public partial class AnimatedDoor : AnimatableBody3D, IInteractible {

	public enum StateEnum {
		Open,
		Opening,
		Closed,
		Closing,
	}

	[Signal] public delegate void TriedToOpenEventHandler();
	[Signal] public delegate void OpeningEventHandler();
	[Signal] public delegate void OpenedEventHandler();
	[Signal] public delegate void ClosingEventHandler();
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
		
		if (Locked && obeyLock && State == StateEnum.Closed) {
			if (LockedAnimation == null || LockedAnimation == string.Empty) return;
			AnimationPlayer.Play(LockedAnimation);
			EmitSignal(SignalName.TriedToOpen);
			return;
		}
		
		if (OpenAnimation == null || OpenAnimation == string.Empty) return;

		EmitSignal(SignalName.Opening);
		AnimationPlayer.Play(OpenAnimation);
		State = StateEnum.Opening;
	}
	public void ForceOpen() => Open(false);

	public void Close() {
		if (!Interruptible && InMotion) return;
		if (CloseAnimation == null || CloseAnimation == string.Empty) return;

		EmitSignal(SignalName.Closing);
		if (CloseAnimation == OpenAnimation) {
			AnimationPlayer.Play(OpenAnimation, -1.0, -1.0f, true);
		} else {
			AnimationPlayer.Play(CloseAnimation);
		}
		State = StateEnum.Closing;
	}

	public void Toggle(bool obeyLock = true) {
		if (State == StateEnum.Closed || State == StateEnum.Closing) Open(obeyLock);
		else Close();
	}

	public void Lock() => Locked = true;
	public void Unlock() => Locked = false;
	public void ToggleLock() => Locked = !Locked;

	private void OnAnimationFinished(StringName animName) {
		if (State == StateEnum.Opening) {
			EmitSignal(SignalName.Opened);
			State = StateEnum.Open;
		}
		else if (State == StateEnum.Closing) {
			EmitSignal(SignalName.Closed);
			State = StateEnum.Closed;
		}
	}


	public void InteractPress() {
		Toggle();
	}
	public void InteractRelease() {}

}