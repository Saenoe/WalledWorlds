using System;
using Godot;

namespace Aberration;

// why did i do this
public partial class Crosshair : Node {

	[Export] public ShaderMaterial Material { get; set; }

	[Export] public float ChangeTime { get; set; } = 0.5f;

	[Export] public float IdleInnerDotRadius { get; set; } = 0.022f;
	[Export] public float IdleOuterRingRadius { get; set; } = 0.045f;

	[Export] public float ActiveInnerDotRadius { get; set; } = 0.0f;
	[Export] public float ActiveOuterRingRadius { get; set; } = 0.085f;

	public bool Active { get; set; } = false;

	private float _interpValue = 0.0f;

	public override void _Process(double dt) {

		if (Active) _interpValue = Math.Min(_interpValue + (float)dt, ChangeTime);
		else _interpValue = Math.Max(_interpValue - (float)dt, 0.0f);

		float innerDotRadius = (float)Tween.InterpolateValue(
			IdleInnerDotRadius,
			ActiveInnerDotRadius - IdleInnerDotRadius,
			_interpValue,
			ChangeTime,
			Tween.TransitionType.Cubic,
			Tween.EaseType.InOut
		);
		float outerRingRadius = (float)Tween.InterpolateValue(
			IdleOuterRingRadius,
			ActiveOuterRingRadius - IdleOuterRingRadius,
			_interpValue,
			ChangeTime,
			Tween.TransitionType.Cubic,
			Tween.EaseType.InOut
		);

		Material.SetShaderParameter("inner_dot_radius", innerDotRadius);
		Material.SetShaderParameter("outer_ring_radius", outerRingRadius);
	}

}