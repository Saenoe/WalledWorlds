using Godot;
using System;

namespace Aberration;

public partial class HallwayTeleport : Area3D {

	private Timer _teleportTimer;
	private SeamlessTeleporter _teleporter;
	private AudioStreamPlayer3D _player;
	private VisibleOnScreenNotifier3D _entranceVisibleNotifier;

	public override void _Ready() {
		_teleportTimer = GetNode<Timer>("TeleportTimer");
		_teleporter = GetNode<SeamlessTeleporter>("HallwayTeleport");
		_player = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
		_entranceVisibleNotifier = GetNode<VisibleOnScreenNotifier3D>("EntranceVisibleNotifier");
	
		Connect(SignalName.BodyEntered, Callable.From((Node3D body) => {
			_player.Play();
			_teleportTimer.Timeout += () => {
				if (!_entranceVisibleNotifier.IsOnScreen()) {
					_teleporter.Teleport(body);
					GD.Print("whe");
				} else {
					_entranceVisibleNotifier.Connect(VisibleOnScreenNotifier3D.SignalName.ScreenExited, Callable.From(() => {
						_teleporter.Teleport(body);
					}), (int)ConnectFlags.OneShot);
				}
			};
			_teleportTimer.Start();
		}), (int)ConnectFlags.OneShot);

	}




}
