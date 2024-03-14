using Godot;

namespace Aberration;

public partial class SpookyHouse : Node {

	private Timer _shouldLeaveTimer;
	private VisibleOnScreenNotifier3D _onScreenNotifier;


	public override void _Ready() {
		_shouldLeaveTimer = GetNode<Timer>("ShouldLeaveTimer");
		_onScreenNotifier = GetNode<VisibleOnScreenNotifier3D>("OnScreenNotifier");

		_shouldLeaveTimer.Timeout += () => {
			var doorbell = GetNode<AnimatedDoor>("Doorbell");

			if (!_onScreenNotifier.IsOnScreen()) {
				doorbell.ForceOpen();
			} else {
				_onScreenNotifier.Connect(
					VisibleOnScreenNotifier3D.SignalName.ScreenExited,
					Callable.From(() => doorbell.ForceOpen()),
					(int)ConnectFlags.OneShot
				);
			}

			_shouldLeaveTimer.GetParent().RemoveChild(_shouldLeaveTimer);
		};
	}

	public void BeginSpooks() {
		if (_shouldLeaveTimer.IsStopped()) _shouldLeaveTimer.Start();
	}

} 