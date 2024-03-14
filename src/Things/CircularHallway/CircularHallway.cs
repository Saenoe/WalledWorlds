using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aberration;

public partial class CircularHallway : Node3D {

	private PackedScene _pieceScene;

	private float _pieceAngle = 10.0f;
	private float _radius = 10.0f;

	private int _circlePieceCount;
	private int _currentPieceId = 0;
	private int _modPieceId = 0;
	private float _currentAngle = 0.0f;
	private bool _initialized = false;

	private readonly int _cullDistance = 8;

	private readonly Dictionary<int, HallwayPiece> _pieces = new();
	private readonly List<HallwayPiece> _activePieces = new();

	private HallwayPiece _currentInside = null;
	private HallwayPiece _prevInside = null;

	private bool _playerInRoom = false;
	private int _roomPieceId = 0; 

	private RandomNumberGenerator _rng;

	public override void _Ready() {
		_rng = new RandomNumberGenerator();

		_pieceScene = ResourceLoader.Load<PackedScene>("res://scenes/CircularHallway/HallwayPiece.tscn");
		_circlePieceCount = (int)(Math.Round(360.0f / PieceAngle) + 0.5f);

	}

	public override void _Process(double _dt) {
		Camera3D cam = GetViewport().GetCamera3D();

		Vector3 pos = this.GlobalPosition - cam.GlobalPosition;
		Vector2 normPos = new Vector2(pos.X, pos.Z).Normalized();

		float angle = -MathF.Atan2(normPos.Y, normPos.X) + MathF.PI; 
		int newId = (int)(MathF.Floor(Mathf.PosMod(angle + Mathf.DegToRad(_pieceAngle / 2.0f), MathF.Tau) / MathF.Tau * _circlePieceCount) + 0.5f);

		if ((newId != _modPieceId || !_initialized) && !_playerInRoom) {
			
			int a = Mathf.PosMod(_modPieceId - newId, _circlePieceCount);
			int b = Mathf.PosMod(newId - _modPieceId, _circlePieceCount);

			int angleDiff = a < b ? -a : b;

			_modPieceId = newId;
			_currentPieceId += angleDiff;
			UpdatePieces();
		}

	}

	private void CreateOrSpawnPiece(int id) {
		float angle = (id % _circlePieceCount) * Mathf.DegToRad(_pieceAngle);

		if (!_pieces.ContainsKey(id)) {
			HallwayPiece newPiece = _pieceScene.Instantiate<HallwayPiece>();
			Transform3D t = Transform3D.Identity.Translated(Vector3.Right * Radius).Rotated(Vector3.Up, angle);
			newPiece.Transform = t;
			newPiece.Id = id;

			newPiece.LeftDoorEntered += OnLeftRoomEntered;
			newPiece.LeftDoorExited += OnLeftRoomExited;
			
			newPiece.RightDoorEntered += OnRightRoomEntered;
			newPiece.RightDoorExited += OnRightRoomExited;

			newPiece.Rng = _rng;

			_pieces.Add(id, newPiece);
		}
		HallwayPiece piece = _pieces[id];
		
		if (_activePieces.Contains(piece)) return;

		_activePieces.Add(piece);
		AddChild(piece);
	}

	private void UpdatePieces() {
		CullOutOfRange();

		for (int i = _currentPieceId - _cullDistance; i <= _currentPieceId + _cullDistance; ++i) {
			CreateOrSpawnPiece(i);
		}

		_currentInside = _pieces[_currentPieceId];
		if (_currentInside != null) {
			_currentInside.IsPlayerInside = true;

			if (_prevInside != null && _prevInside != _currentInside) {
				_prevInside.IsPlayerInside = false;
			}
		}
		_prevInside = _currentInside;
	}



	private void OnLeftRoomEntered(int id) {
		if (_playerInRoom) return;

		_playerInRoom = true;
		_roomPieceId = id;

		foreach (var piece in _pieces.Values.Where(p => p.Id != id)) {
			piece.LeftHardCull = true;
		}
	}
	private void OnLeftRoomExited(int id) {
		if (!_playerInRoom || id != _roomPieceId) return;

		_playerInRoom = false;

		foreach (var piece in _pieces.Values.Where(p => p.Id != id)) {
			piece.LeftHardCull = false;
		}
	}
	private void OnRightRoomEntered(int id) {
		if (_playerInRoom) return;

		_playerInRoom = true;
		_roomPieceId = id;

		foreach (var piece in _pieces.Values.Where(p => p.Id != id)) {
			piece.RightHardCull = true;
		}
	}
	private void OnRightRoomExited(int id) {
		if (!_playerInRoom || id != _roomPieceId) return;

		_playerInRoom = false;

		foreach (var piece in _pieces.Values.Where(p => p.Id != id)) {
			piece.RightHardCull = false;
		}
	}

	private void CullOutOfRange() {
		_activePieces.RemoveAll(p => {
			bool remove = Math.Abs(_currentPieceId - p.Id) > _cullDistance;

			if (remove) { p.GetParent().RemoveChild(p); }

			return remove;
		});
	}

	[Export]
	public float PieceAngle {
		get => _pieceAngle;
		set {
			_pieceAngle = value;
		}
	}
	[Export]
	public float Radius {
		get => _radius;
		set {
			_radius = value;
		}
	}

}
