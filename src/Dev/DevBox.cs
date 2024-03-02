using Godot;

namespace Aberration.Dev;

[Tool]
public partial class DevBox : Node3D {

	[Export] public bool Collision { get; set; } = true;
	
	private Vector3 _dimensions = Vector3.One;


	private BoxShape3D _boxShape = null;
	private BoxMesh _mesh = new();

	public override void _Ready() {
		_mesh.Size = _dimensions;
		AddChild(new MeshInstance3D { Mesh = _mesh });

		if (!Engine.IsEditorHint() && Collision) {
			_boxShape = new BoxShape3D {
				Size = _dimensions,
			};

			StaticBody3D collisionBody = new();
			collisionBody.AddChild(new CollisionShape3D { Shape = _boxShape });
		} 
	}

	[Export] public Vector3 Dimensions {
		get => _dimensions;
		set {
			_dimensions = value;
			if (!IsNodeReady()) return;

			_mesh.Size = _dimensions;
			if (_boxShape != null) _boxShape.Size = _dimensions;
		}
	}

}