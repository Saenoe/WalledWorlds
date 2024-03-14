using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Godot;

namespace Aberration;

[GlobalClass]
public partial class SceneRandomizer : Resource {

	[Export] public Godot.Collections.Array<PackedScene> LimitedPool { get; set; } = null;
	[Export] public Godot.Collections.Array<PackedScene> BasicPool { get; set; } = null;

	[Export] public float LimitedPullChance { get; set; } = 0.5f;

	public bool LimitedPoolEmpty => _limitedPoolIndex >= LimitedPool.Count;

	private List<PackedScene> _shuffledLimitedPool;
	private int _limitedPoolIndex = 0;

	private List<PackedScene> _shuffledBasicPool;
	private int _basicPoolIndex = 0;

	private bool _initialized = false;
	private RandomNumberGenerator _rng;

	public PackedScene GetRandom() {
		if (LimitedPool == null && BasicPool == null) return null;

		if (!_initialized) Initialize();
			

		bool useLimited = ((_rng.Randf() < LimitedPullChance) || BasicPool == null) && LimitedPool != null && _limitedPoolIndex < _shuffledLimitedPool.Count;
		if (useLimited) {
			return _shuffledLimitedPool[_limitedPoolIndex++];
		}

		if (BasicPool == null) return null;

		if (_basicPoolIndex >= _shuffledBasicPool.Count) {
			ShufflePool(_shuffledBasicPool);
			_basicPoolIndex = 0;
		}

		return _shuffledBasicPool[_basicPoolIndex++];
	}

	public void Initialize() {

		_rng = new RandomNumberGenerator {
			Seed = Util.Hash32((uint)Time.GetTicksMsec()),
		};

		if (LimitedPool != null) {
			_shuffledLimitedPool = new List<PackedScene>(LimitedPool);
			ShufflePool(_shuffledLimitedPool);
		}

		if (BasicPool != null) {
			_shuffledBasicPool = new List<PackedScene>(BasicPool);
			ShufflePool(_shuffledBasicPool);
		}
	
		_initialized = true;
	}



	private void ShufflePool(IList pool) {
		int n = pool.Count;
		while (n >= 1) {
			int k = _rng.RandiRange(0, --n);
			(pool[n], pool[k]) = (pool[k], pool[n]);
		}
	}

}