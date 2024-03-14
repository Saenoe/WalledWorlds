using Godot;

namespace Aberration;

public partial class EndScene : Node3D {

	[Export] 
	public float SkyExposure {
		get => GameManager.WorldEnvironment.Environment.BackgroundEnergyMultiplier;
		set => GameManager.WorldEnvironment.Environment.BackgroundEnergyMultiplier = value;
	}

	public override void _Ready() {

		GetNode<AnimationPlayer>("AnimationPlayer").Play("transition");

	}

	public void FixSky() {
		GameManager.WorldEnvironment.Environment.BackgroundMode = Environment.BGMode.Sky;
		((PanoramaSkyMaterial)GameManager.WorldEnvironment.Environment.Sky.SkyMaterial).Panorama =
			ResourceLoader.Load<Texture2D>("res://assets/textures/sky.png");
		
		GameManager.WorldEnvironment.Environment.FogEnabled = true;
		GameManager.WorldEnvironment.Environment.FogDensity *= 10.0f;
	}

	public void DeletePlayer() {
		GameManager.Player.GetParent().RemoveChild(GameManager.Player);
	}

	public void TheEnd() {
		GameManager.ReturnToMenu();
	}

}
