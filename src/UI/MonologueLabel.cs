using Godot;

namespace Aberration;

public partial class MonologueLabel : Label {
	public override void _Ready() {
		InternalMonologue.MonologueLabel = this;
	}
}