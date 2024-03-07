using System.Collections.Generic;
using Godot;

namespace Aberration;

public partial class InternalMonologue : Node {

	private readonly struct MonologueEntry {
		public string Text { get; init; }
		public double DisplaySeconds { get; init; }
	}

	public static Label MonologueLabel { get; set; } = null;
	private static readonly Timer _displayTimer = new();

	private static readonly Queue<MonologueEntry> _entries = new();

	public override void _Ready() {
		_displayTimer.OneShot = true;
		_displayTimer.Timeout += DisplayNext;
		AddChild(_displayTimer);
	}


	public static void SubmitText(string text, double displaySeconds, bool overrideCurrent = false) {
		if (overrideCurrent) {
			_entries.Clear();
			_displayTimer.Stop();
		}

		_entries.Enqueue(new MonologueEntry {
			Text = text,
			DisplaySeconds = displaySeconds,
		});

		if (_displayTimer.IsStopped()) DisplayNext();
	}

	private static void DisplayNext() {
		if (_entries.Count == 0) {
			ResetLabel();
			return;
		}

		MonologueEntry entry = _entries.Dequeue();
		_displayTimer.WaitTime = entry.DisplaySeconds;
		MonologueLabel.Text = entry.Text;

		_displayTimer.Start();
	}

	private static void ResetLabel() {
		if (MonologueLabel != null) MonologueLabel.Text = string.Empty;
	}


}