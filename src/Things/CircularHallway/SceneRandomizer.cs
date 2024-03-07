using System.Collections.Generic;
using Godot;

namespace Aberration;

[GlobalClass]
public partial class SceneRandomizer : Resource {

	[Export]
	public Godot.Collections.Array<PackedScene> Scenes { get; set; }

	[Export]
	public bool LimitedPool { get; set; } = false;

	private bool _initialised = false;
	private readonly Queue<PackedScene> _randomPool = new();
	private uint _randomSeed = 0;

	public PackedScene GetRandom() {
		if (Scenes == null || Scenes.Count == 0) return null;
		if (_initialised && LimitedPool && _randomPool.Count == 0) return null;
		if (!_initialised || (!LimitedPool && _randomPool.Count == 0)) Initialise();

		return _randomPool.Dequeue();
	}

	private void Initialise() {
		_randomPool.EnsureCapacity(Scenes.Count);

		_randomSeed = Util.Hash32((uint)Time.GetTicksUsec());
		for (int i = 0; i < Scenes.Count; ++i) {
			_randomPool.Enqueue(Scenes[(int)(Util.Hash32(_randomSeed) % Scenes.Count)]);
		}
		_initialised = true;
	}


}