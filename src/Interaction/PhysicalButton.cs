using Godot;

namespace Aberration;

public partial class PhysicalButton : AnimatableBody3D, IInteractible {

	[Signal] public delegate void PressedEventHandler();
	[Signal] public delegate void ReleasedEventHandler();
	[Signal] public delegate void ToggledEventHandler(bool isPressed);

	[Export] public bool IsPressed { get; private set; } = false;

	[Export] public bool Toggle { get; set; } = false;

	public void InteractPress() {
		if (Toggle) {
			IsPressed = !IsPressed;
			
			if (IsPressed) EmitSignal(SignalName.Pressed);
			else EmitSignal(SignalName.Released);

			EmitSignal(SignalName.Toggled, IsPressed);
		} else {
			IsPressed = true;
			EmitSignal(SignalName.Pressed);
		}
	}
	public void InteractRelease() {
		if (Toggle) return;

		EmitSignal(SignalName.Released);
	}

}