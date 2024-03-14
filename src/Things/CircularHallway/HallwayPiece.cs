using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace Aberration;

public partial class HallwayPiece : Node3D {

	[Signal] public delegate void LeftDoorEnteredEventHandler(int id);
	[Signal] public delegate void LeftDoorExitedEventHandler(int id);

	[Signal] public delegate void RightDoorEnteredEventHandler(int id);
	[Signal] public delegate void RightDoorExitedEventHandler(int id);


	//[Export] public SceneRandomizer LeftRooms { get; set; }
	//[Export] public SceneRandomizer RightRooms { get; set; }

	[Export(PropertyHint.Range, "0.0,1.0,0.05")]
	public float LockChance { get; set; } = 0.5f;

	[Export]
	public int Id { get; set; } = 0; 

	[Export]
	public RandomNumberGenerator Rng { get; set; } = null;

	public bool IsPlayerInside { get; set; } = false;

	private Node3D _leftDoorOrigin;
	private AnimatedDoor _leftDoor;
	private VisibleOnScreenNotifier3D _leftDoorVisibleNotifier;
	private PackedScene _leftRoomScene = null;
	private Node3D _leftRoom = null;
	private readonly Dictionary<ShaderMaterial, ShaderMaterial> _leftRoomMaterials = new();
	private readonly Dictionary<Mesh, Mesh> _leftRoomMeshes = new();
	private bool _leftRoomCulled = true;
	private bool _inLeftRoom = false;
	private bool _leftHardCull = false;

	private Node3D _rightDoorOrigin;
	private AnimatedDoor _rightDoor;
	private VisibleOnScreenNotifier3D _rightDoorVisibleNotifier;
	private PackedScene _rightRoomScene = null;
	private Node3D _rightRoom = null;
	private readonly Dictionary<ShaderMaterial, ShaderMaterial> _rightRoomMaterials = new();
	private readonly Dictionary<Mesh, Mesh> _rightRoomMeshes = new();
	private bool _rightRoomCulled = true;
	private bool _inRightRoom = false;
	private bool _rightHardCull = false;


	private uint _initialSeed;

	public override void _Ready() {

		_leftDoorOrigin = GetNode<Node3D>("LeftDoorOrigin");
		_leftDoor = _leftDoorOrigin.GetNode<AnimatedDoor>("LeftDoor/Door");
		_leftDoorVisibleNotifier = _leftDoorOrigin.GetNode<VisibleOnScreenNotifier3D>("LeftDoorVisibleNotifier");
		_leftDoor.Opening += CreateOrShowLeftRoom;
		_leftDoor.Closed += OnLeftDoorClosed;
		this.LeftDoorEntered += OnLeftDoorEntered;
		this.LeftDoorExited += OnLeftDoorExited;

		_rightDoorOrigin = GetNode<Node3D>("RightDoorOrigin");
		_rightDoor = _rightDoorOrigin.GetNode<AnimatedDoor>("RightDoor/Door");
		_rightDoorVisibleNotifier = _rightDoorOrigin.GetNode<VisibleOnScreenNotifier3D>("RightDoorVisibleNotifier");
		_rightDoor.Opening += CreateOrShowRightRoom;
		_rightDoor.Closed += OnRightDoorClosed;
		this.RightDoorEntered += OnRightDoorEntered;
		this.RightDoorExited += OnRightDoorExited;


		_initialSeed = Util.Hash32((uint)Time.GetTicksUsec());

		Randomize();
	}

	public override void _Process(double dt) {
		HandleRoomEnterExit();
	}


	public bool LeftHardCull {
		get => _leftHardCull;
		set {
			_leftHardCull = value;
			if (_leftDoorOrigin == null) return;
			if (_leftHardCull) {
				_leftDoorOrigin.Visible = false;
				_leftDoorOrigin.ProcessMode = ProcessModeEnum.Disabled;
			} else {
				_leftDoorOrigin.Visible = true;
				_leftDoorOrigin.ProcessMode = ProcessModeEnum.Inherit;
			}
		}
	}


	public bool RightHardCull {
		get => _rightHardCull;
		set {
			_rightHardCull = value;
			if (_rightDoorOrigin == null) return;
			if (_rightHardCull) {
				_rightDoorOrigin.Visible = false;
				_rightDoorOrigin.ProcessMode = ProcessModeEnum.Disabled;
			} else {
				_rightDoorOrigin.Visible = true;
				_rightDoorOrigin.ProcessMode = ProcessModeEnum.Inherit;
			}
		}
	}


	private void CreateOrShowLeftRoom() {
		if (_leftRoom == null || _leftRoomScene == null) {
			
			if (Rng.Randf() < 0.25f && !GameManager.SharedRoomRandomizer.LimitedPoolEmpty) {
				_leftRoomScene = GameManager.SharedRoomRandomizer.GetRandom();
			} else {
				_leftRoomScene = GameManager.InsideRoomRandomizer.GetRandom();
			}

			_leftRoom = _leftRoomScene.Instantiate<Node3D>();

			GetRoomMaterials(_leftRoom, _leftRoomMaterials, _leftRoomMeshes);
			Vector3[] points = GetCullWindowPoints(_leftDoorVisibleNotifier);
			UpdateShaderParameters(_leftRoomMaterials.Values, "window_quad", points);
			UpdateShaderParameters(_leftRoomMaterials.Values, "enable_cull", _leftRoomCulled);

			_leftRoom.ProcessMode = ProcessModeEnum.Disabled;
			_leftDoorOrigin.AddChild(_leftRoom);
		}
		_leftRoom.Visible = true;
	}
	public void HideLeftRoom() {
		if (_leftRoom == null) return;
		_leftRoom.Visible = false;
	}
	private void OnLeftDoorClosed() {
		if (!CameraInLeftRoom()) {
			HideLeftRoom();
		}
	}
	public bool CameraInLeftRoom() {
		return new Plane(_leftDoorOrigin.GlobalBasis.Z, _leftDoorOrigin.GlobalPosition)
			.DistanceTo(GetViewport().GetCamera3D().GlobalPosition) < 0.0f;
	}

	public void CreateOrShowRightRoom() {
		if (_rightRoom == null || _rightRoomScene == null) {

			if (GameManager.InsideRoomRandomizer.LimitedPoolEmpty && GameManager.OutsideRoomRandomizer.LimitedPoolEmpty && GameManager.SharedRoomRandomizer.LimitedPoolEmpty) {
				if (Rng.Randf() > 0.5f && GameManager.HallwayExitScene != null) {
					_rightRoomScene = GameManager.HallwayExitScene;
					GameManager.HallwayExitScene = null;
					
				}
			}
			
			if (_rightRoomScene == null) {
				if (Rng.Randf() < 0.25f && !GameManager.SharedRoomRandomizer.LimitedPoolEmpty) {
					_rightRoomScene = GameManager.SharedRoomRandomizer.GetRandom();
				} else {
					_rightRoomScene = GameManager.OutsideRoomRandomizer.GetRandom();
				}
			}
			
			_rightRoom = _rightRoomScene.Instantiate<Node3D>();

			GetRoomMaterials(_rightRoom, _rightRoomMaterials, _rightRoomMeshes);
			Vector3[] points = GetCullWindowPoints(_rightDoorVisibleNotifier);
			UpdateShaderParameters(_rightRoomMaterials.Values, "window_quad", points);
			UpdateShaderParameters(_rightRoomMaterials.Values, "enable_cull", _rightRoomCulled);

			_rightRoom.ProcessMode = ProcessModeEnum.Disabled;
			_rightDoorOrigin.AddChild(_rightRoom);
		}
		_rightRoom.Visible = true;
	}
	public void HideRightRoom() {
		if (_rightRoom == null) return;
		_rightRoom.Visible = false;
	}
	private void OnRightDoorClosed() {
		if (!CameraInRightRoom()) {
			HideRightRoom();
		}
	}
	public bool CameraInRightRoom() {
		return new Plane(_rightDoorOrigin.GlobalBasis.Z, _rightDoorOrigin.GlobalPosition)
			.DistanceTo(GetViewport().GetCamera3D().GlobalPosition) < 0.0f;
	}

	private void HandleRoomEnterExit() {

		bool _cameraInLeftRoom = CameraInLeftRoom();

		if (!_inLeftRoom && _cameraInLeftRoom && IsPlayerInside) {
			_inLeftRoom = true;
			EmitSignal(SignalName.LeftDoorEntered, Id);
		} else
		if (_inLeftRoom && !_cameraInLeftRoom && IsPlayerInside) {
			_inLeftRoom = false;
			EmitSignal(SignalName.LeftDoorExited, Id);
		}

		bool _cameraInRightRoom = CameraInRightRoom();

		if (!_inRightRoom && _cameraInRightRoom && IsPlayerInside) {
			_inRightRoom = true;
			EmitSignal(SignalName.RightDoorEntered, Id);
		} else
		if (_inRightRoom && !_cameraInRightRoom && IsPlayerInside) {
			_inRightRoom = false;
			EmitSignal(SignalName.RightDoorExited, Id);
		}
	}

	private void OnLeftDoorEntered(int _id) {
		UpdateShaderParameters(_leftRoomMaterials.Values, "enable_cull", false);
		if (!LeftHardCull) _leftRoom.ProcessMode = ProcessModeEnum.Inherit;
	}
	private void OnLeftDoorExited(int _id) {
		UpdateShaderParameters(_leftRoomMaterials.Values, "enable_cull", true);
		if (!LeftHardCull) _leftRoom.ProcessMode = ProcessModeEnum.Disabled;
	}

	private void OnRightDoorEntered(int _id) {
		UpdateShaderParameters(_rightRoomMaterials.Values, "enable_cull", false);
		if (!RightHardCull) _rightRoom.ProcessMode = ProcessModeEnum.Inherit;
	}
	private void OnRightDoorExited(int _id) {
		UpdateShaderParameters(_rightRoomMaterials.Values, "enable_cull", true);
		if (!RightHardCull) _rightRoom.ProcessMode = ProcessModeEnum.Disabled;
	}

	private static void UpdateShaderParameters(IEnumerable<ShaderMaterial> materials, string param, Variant value) {
		foreach (var mat in materials) {
			mat.SetShaderParameter(param, value);
		}
	}

	private static Vector3[] GetCullWindowPoints(VisibleOnScreenNotifier3D window) {
		Aabb aabb = window.Aabb;

		Vector3 p0 = new(aabb.Position.X, aabb.End.Y, 0.0f);
		Vector3 p1 = new(aabb.End.X, aabb.End.Y, 0.0f);
		Vector3 p2 = new(aabb.End.X, aabb.Position.Y, 0.0f);
		Vector3 p3 = new(aabb.Position.X, aabb.Position.Y, 0.0f);

		return new Vector3[] {
			window.GlobalTransform * p0,
			window.GlobalTransform * p1,
			window.GlobalTransform * p2,
			window.GlobalTransform * p3
		};
	}

	private static void GetRoomMaterials(Node room, Dictionary<ShaderMaterial, ShaderMaterial> materials, Dictionary<Mesh, Mesh> meshes) {

		if (room is MeshInstance3D mi) {
			
			if (!meshes.ContainsKey(mi.Mesh)) {
				meshes.Add(mi.Mesh, (Mesh)mi.Mesh.Duplicate());
			}
			mi.Mesh = meshes[mi.Mesh];

			for (int i = 0; i < mi.Mesh.GetSurfaceCount(); ++i) {
				Material mat = mi.Mesh.SurfaceGetMaterial(i);
				if (mat is ShaderMaterial shmat) {
					if (!materials.ContainsKey(shmat)) {
						materials.Add(shmat, (ShaderMaterial)shmat.Duplicate());
					}
					mi.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
					mi.Mesh.SurfaceSetMaterial(i, materials[shmat]);
				}
			}
		}

		foreach (Node meshInstance in room.GetChildren()) {
			GetRoomMaterials(meshInstance, materials, meshes);
		}
	}

	private void Randomize() {
		if (Rng == null) Rng = new RandomNumberGenerator {
			Seed = Util.Hash32(_initialSeed),
		};

		_leftDoor.Locked = Rng.Randf() < LockChance;
		_rightDoor.Locked = Rng.Randf() < LockChance;
	}

}