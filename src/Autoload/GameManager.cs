using Godot;

namespace Aberration;

public partial class GameManager : Node {
	
	public enum GameStateEnum {
		MainMenu,
		InGame,
		Paused,
	}

	public static float MouseSensitivity { get; set; } = 0.002f;

	public static Player Player { get; set; }


	public static GameStateEnum GameState { get; private set; }= GameStateEnum.MainMenu;

	[Export]
	public Environment DefaualtEnvironment { get; set; }
	
	public static WorldEnvironment WorldEnvironment { get; set; }

	public static SceneRandomizer InsideRoomRandomizer { get; private set; }
	public static SceneRandomizer OutsideRoomRandomizer { get; private set; }
	public static SceneRandomizer SharedRoomRandomizer { get; private set; }
	public static PackedScene HallwayExitScene { get; set; }

	private static Environment _defaultEnvironment;

	private static PackedScene _mainGamePackedScene;

	private static Node3D _mainGameScene;
	private static Control _mainMenuScene;
	private static Control _pauseMenuScene;
	private static Control _gameUiScene;

	private static Node _root;
	private static Node _uiRoot;
	private static Node _gameRoot;

	public override void _Ready() {
		LoadSettings();

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

		_defaultEnvironment = DefaualtEnvironment;

		_mainGamePackedScene = ResourceLoader.Load<PackedScene>("res://scenes/Areas/Lv1.tscn");

		_mainMenuScene = ResourceLoader.Load<PackedScene>("res://scenes/UI/MainMenu.tscn").Instantiate<Control>();

		_gameUiScene = ResourceLoader.Load<PackedScene>("res://scenes/UI/GameUI.tscn").Instantiate<Control>();
		_gameUiScene.Visible = false;

		_pauseMenuScene = ResourceLoader.Load<PackedScene>("res://scenes/UI/PauseMenu.tscn").Instantiate<Control>();
		_pauseMenuScene.Visible = false;
		_pauseMenuScene.ProcessMode = ProcessModeEnum.WhenPaused;

		_uiRoot.AddChild(_gameUiScene);
		_uiRoot.AddChild(_pauseMenuScene);


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
			_uiRoot.RemoveChild(_mainMenuScene);
			_gameUiScene.Visible = true;
			

			WorldEnvironment = new WorldEnvironment {
				Environment = (Environment)_defaultEnvironment.Duplicate(),
			};
			_gameRoot.AddChild(WorldEnvironment);

			SharedRoomRandomizer = (SceneRandomizer)ResourceLoader.Load("res://scenes/CircularHallway/Rooms/SharedRooms.tres").Duplicate();
			InsideRoomRandomizer = (SceneRandomizer)ResourceLoader.Load("res://scenes/CircularHallway/Rooms/InsideRooms.tres").Duplicate();
			OutsideRoomRandomizer = (SceneRandomizer)ResourceLoader.Load("res://scenes/CircularHallway/Rooms/OutsideRooms.tres").Duplicate();
			HallwayExitScene = ResourceLoader.Load<PackedScene>("res://scenes/CircularHallway/Rooms/BridgeThing.tscn");

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
			_gameRoot.RemoveChild(WorldEnvironment);
			WorldEnvironment.QueueFree();
		}
		if (_mainGameScene != null) _mainGameScene.QueueFree();

		_root.GetTree().Paused = false;
		_pauseMenuScene.Visible = false;

		_gameUiScene.Visible = false;
		_uiRoot.AddChild(_mainMenuScene);

		Input.MouseMode = Input.MouseModeEnum.Visible;

		_mainGameScene = _mainGamePackedScene.Instantiate<Node3D>();
		_mainGameScene.Visible = false;
		_mainGameScene.ProcessMode = ProcessModeEnum.Disabled;


		GameState = GameStateEnum.MainMenu;
	}

	public static void SaveSettings() {
		ConfigFile config = new();
		config.SetValue("controls", "mouse_sensitivity", MouseSensitivity);
		config.SetValue("video", "vsync", (int)DisplayServer.WindowGetVsyncMode());
		config.SetValue("video", "fullscreen", (int)DisplayServer.WindowGetMode());
		config.SetValue("audio", "volume", 
			Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master")))
		);

		config.Save("user://game.cfg");
	}

	public static void LoadSettings() {
		ConfigFile config = new();
		Error code = config.Load("user://game.cfg");

		if (code != Error.Ok) {
			SaveSettings();
			return;
		}

		MouseSensitivity = (float)config.GetValue("controls", "mouse_sensitivity");
		DisplayServer.WindowSetVsyncMode((DisplayServer.VSyncMode)(int)config.GetValue("video", "vsync"));
		DisplayServer.WindowSetMode((DisplayServer.WindowMode)(int)config.GetValue("video", "fullscreen"));
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), Mathf.LinearToDb((float)config.GetValue("audio", "volume")));
	}


}