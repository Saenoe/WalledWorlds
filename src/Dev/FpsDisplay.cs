using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Aberration.Dev;

public partial class FpsDisplay : Node {

	private readonly List<double> _frameTimes = new();

	public override void _Ready() {
		Timer timer = new Timer {
			Autostart = true,
			OneShot = false,
			WaitTime = 1.0,
		};

		timer.Timeout += () => {
			InternalMonologue.SubmitText((1.0 /_frameTimes.Average()).ToString("0.0"), 1.0, true);
			_frameTimes.Clear();
		};

		AddChild(timer);
		
	}


	public override void _Process(double dt) {
		_frameTimes.Add(dt);
	}

}