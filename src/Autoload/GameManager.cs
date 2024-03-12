using System.Text.Json.Serialization;
using Godot;

namespace Aberration;

public partial class GameManager : Node {
	
	public enum GameStateEnum {
		MainMenu,
		InGame,
		Paused,
	}

	public static Player Player { get; set; }


	public static GameStateEnum GameState { get; private set; }= GameStateEnum.MainMenu;

	private static PackedScene _mainGamePackedScene;

	private static Node3D _mainGameScene;
	private static Control _mainMenuScene;
	private static Control _pauseMenuScene;
	private static Control _gameUiScene;

	private static Node _root;
	private static Node _uiRoot;
	private static Node _gameRoot;

	public override void _Ready() {
		_root = this;

		_root.ProcessMode = ProcessModeEnum.Always;

		_uiRoot = new Node {
			Name = "_uiRoot",
			ProcessMode = ProcessModeEnum.Pausable,
		};
		_gameRoot = new Node {
			Name = "_gameRoot",
			ProcessMode = ProcessModeEnum.Pausable,
		};
		AddChild(_gameRoot);
		AddChild(_uiRoot);

		_mainGamePackedScene = ResourceLoader.Load<PackedScene>("res://scenes/Areas/Lv1.tscn");

		_mainMenuScene = ResourceLoader.Load<PackedScene>("res://scenes/UI/MainMenu.tscn").Instantiate<Control>();

		_gameUiScene = ResourceLoader.Load<PackedScene>("res://scenes/UI/GameUI.tscn").Instantiate<Control>();
		_gameUiScene.Visible = false;

		_pauseMenuScene = ResourceLoader.Load<PackedScene>("res://scenes/UI/PauseMenu.tscn").Instantiate<Control>();
		_pauseMenuScene.Visible = false;
		_pauseMenuScene.ProcessMode = ProcessModeEnum.WhenPaused;

		_uiRoot.AddChild(_mainMenuScene);
		_uiRoot.AddChild(_pauseMenuScene);
		_uiRoot.AddChild(_gameUiScene);

		ReturnToMenu();
	}

	public override void _Input(InputEvent ev) {
		if (ev.IsActionPressed("PauseGame")) {
			if (GameState == GameStateEnum.InGame) {
				PauseGame();
			} else
			if (GameState == GameStateEnum.Paused) {
				StartGame();
			}
		}
	}

	public static void SwitchGameScene(Node3D scene, Transform3D exitTransform) {

		Node3D levelEntrance = (Node3D)scene.FindChild("LevelEntrance", true, false);

		Transform3D playerExitTransform = exitTransform.Inverse() * Player.GlobalTransform;

		Player.GetParent().RemoveChild(Player);
		
		_gameRoot.CallDeferred("remove_child", _mainGameScene);//RemoveChild(_mainGameScene);
		_mainGameScene.QueueFree();

		_mainGameScene = scene;
		_gameRoot.AddChild(_mainGameScene);
		_mainGameScene.AddChild(Player);
		Player.GlobalTransform = levelEntrance.GlobalTransform * playerExitTransform;
		Player.InteractingWith = null;

	}

	public static void StartGame() {

		if (GameState == GameStateEnum.Paused) {
			_root.GetTree().Paused = false;
			_pauseMenuScene.Visible = false;
		} else
		if (GameState == GameStateEnum.MainMenu) {
			_mainGameScene.Visible = true;
			_mainGameScene.ProcessMode = ProcessModeEnum.Inherit;
			_mainMenuScene.Visible = false;	
			_gameUiScene.Visible = true;
			
			_gameRoot.AddChild(_mainGameScene);
		}

		Input.MouseMode = Input.MouseModeEnum.Captured;


		GameState = GameStateEnum.InGame;
	}

	public static void PauseGame() {
		if (GameState == GameStateEnum.MainMenu || GameState == GameStateEnum.Paused) return;

		_root.GetTree().Paused = true;
		_pauseMenuScene.Visible = true;

		Input.MouseMode = Input.MouseModeEnum.Visible;

		GameState = GameStateEnum.Paused;
	}

	public static void ReturnToMenu() {
		if (GameState != GameStateEnum.MainMenu) {
			_gameRoot.RemoveChild(_mainGameScene);	
		}
		if (_mainGameScene != null) _mainGameScene.QueueFree();

		_root.GetTree().Paused = false;
		_pauseMenuScene.Visible = false;

		_gameUiScene.Visible = false;
		_mainMenuScene.Visible = true;

		Input.MouseMode = Input.MouseModeEnum.Visible;

		_mainGameScene = _mainGamePackedScene.Instantiate<Node3D>();
		_mainGameScene.Visible = false;
		_mainGameScene.ProcessMode = ProcessModeEnum.Disabled;


		GameState = GameStateEnum.MainMenu;
	}

}