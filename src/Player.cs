using Godot;

namespace Aberration;

public partial class Player : CharacterBody3D {

	// todo move this somewhere sane
	private const float MouseSensitivity = 0.002f;

	[Export] public float WalkSpeed { get; set; } = 2.0f;
	[Export] public float InteractRange { get; set; } = 2.0f;


	private Camera3D _camera;
	private RayCast3D _interactRay;
	private Crosshair _crosshair;

	private IInteractible _interactingWith = null;

	public override void _Ready() {

		Input.MouseMode = Input.MouseModeEnum.Captured;

		_camera = GetNode<Camera3D>("Camera");

		_interactRay = new RayCast3D {
			TargetPosition = Vector3.Forward * InteractRange,
		};
		_camera.AddChild(_interactRay);

		_crosshair = _camera.GetNode<Crosshair>("Crosshair");
	}

	public override void _Input(InputEvent ev) {
		if (ev is InputEventMouseMotion mouseMotion) {
			_camera.Rotation += new Vector3 {
				X = -mouseMotion.Relative.Y * MouseSensitivity,
				Y = -mouseMotion.Relative.X * MouseSensitivity,
				Z = 0.0f,
			};
		}
	}

	public override void _PhysicsProcess(double dt) {

		PhysicsDirectBodyState3D state = PhysicsServer3D.BodyGetDirectState(this.GetRid());

		Vector3 gravityDirection = state.TotalGravity.Normalized();

		Vector3 forward = NormalAligned(_camera.Basis.Z, -gravityDirection);
		Vector3 right = NormalAligned(_camera.Basis.X, -gravityDirection);

		Vector3 move =
			right * (Input.GetActionStrength("MoveRight") - Input.GetActionStrength("MoveLeft")) +
			forward * (Input.GetActionStrength("MoveBack") - Input.GetActionStrength("MoveForward"));

		float l = move.Length();
		if (l > 1.0) move /= l;

		move *= WalkSpeed;

		Velocity = move;

		MoveAndSlide();

		// vile
		if (_interactRay.GetCollider() is IInteractible interactible) {
			_crosshair.Active = true;
			
			if (Input.IsActionJustPressed("Interact")) {

				if (_interactingWith != null && _interactingWith != interactible) {
					_interactingWith.InteractRelease();
				}

				_interactingWith = interactible;
				interactible.InteractPress();
			} else
			if (Input.IsActionJustReleased("Interact")) {
				_interactingWith = null;
				interactible.InteractRelease();
			}
		} else {
			_crosshair.Active = false;

			if (_interactingWith != null) {
				_interactingWith.InteractRelease();
				_interactingWith = null;
			}
		}

	}

	private static Vector3 NormalAligned(Vector3 vec, Vector3 normal) {
		float d = vec.Dot(normal);

		return vec - normal * d;
	}

}
