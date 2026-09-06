using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Save;
using RailDispatchMono.Core.Game.Rendering;
using RailDispatchMono.Core.Game.Simulation;
using RailDispatchMono.Core.Game.Train;
using RailDispatchMono.Core.Screens;
using RailDispatchMono.Core.Screens.UI;
using RailDispatchMono.Core.ScreenManagers;
using RailDispatchMono.Core.UI.Myra;
using System;
using System.Linq;
using System.Reflection;

namespace RailDispatchMono.Core;

public sealed class RailDispatchMonoGame : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly MyraUIManager _myraUI;
    private ScreenManager _screenManager;
    private MainMenuScreen? _mainMenu;
    private GameplayScreen? _gameplay;
    private MyraGameplayView? _gameplayView;
    private double _gameplayUiRefreshTimer;
    private KeyboardState _previousKeyboard;

    public static bool IsMobile => false;
    public static bool IsDesktop => true;

    public RailDispatchMonoGame()
    {
        DebugManager.LogInfo("RailDispatchMonoGame constructor started.");
        DebugManager.LogInfo($"Runtime: {Environment.OSVersion} | .NET: {Environment.Version}");
        _graphics = new GraphicsDeviceManager(this);
        _myraUI = new MyraUIManager();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 900;
        _graphics.SynchronizeWithVerticalRetrace = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowClientSizeChanged;
    }

    public MyraUIManager MyraUI => _myraUI;

    private void OnWindowClientSizeChanged(object? sender, EventArgs e)
        => DebugManager.LogInfo($"Window resized: {Window.ClientBounds.Width}x{Window.ClientBounds.Height}");

    protected override void Initialize()
    {
        _myraUI.Initialize(this);
        _screenManager = new ScreenManager(this) { TraceEnabled = false };
        Components.Add(_screenManager);
        _mainMenu = new MainMenuScreen(StartGameplay);
        _screenManager.AddScreen(_mainMenu, null);
        base.Initialize();
    }

    private void StartGameplay(string request)
    {
        bool loadExisting = !request.StartsWith("NEW:", StringComparison.Ordinal);
        string slotDirectory = request.StartsWith("NEW:", StringComparison.Ordinal) ? request[4..] : request;
        SaveSlotService.Activate(slotDirectory);
        if (_mainMenu != null)
        {
            _screenManager.RemoveScreen(_mainMenu);
            _mainMenu = null;
        }
        _gameplay = new GameplayScreen(GraphicsDevice, _screenManager, loadExisting);
        _screenManager.AddScreen(_gameplay, null);
        if (loadExisting) _gameplay.LoadSavedGame();
        _gameplayView = new MyraGameplayView(
            speed => _myraUI.QueueAction(() => GameClock.Current?.SetSpeed(speed)),
            train => _myraUI.QueueAction(() => FocusTrain(train)),
            station => _myraUI.QueueAction(() => FocusStation(station)),
            mode => _myraUI.QueueAction(() => SetBuildMode(mode)),
            () => _myraUI.QueueAction(ToggleRouteEditMode),
            train => _myraUI.QueueAction(() => RailwayDispatcher.ForceProceed(train)),
            train => _myraUI.QueueAction(() => RailwayDispatcher.NotifyReleased(train)),
            train => _myraUI.QueueAction(() => ToggleTrainAutomation(train)));
        _myraUI.SetRoot(_gameplayView.Root);
        _gameplayUiRefreshTimer = 0d;
    }

    private void FocusTrain(Train train)
    {
        Camera? camera = GetGameplayField<Camera>("_camera");
        if (camera != null)
            camera.Position = train.Position - new Vector2(GraphicsDevice.Viewport.Width / (2f * camera.Zoom), GraphicsDevice.Viewport.Height / (2f * camera.Zoom));
    }

    private void FocusStation(Station station)
    {
        Camera? camera = GetGameplayField<Camera>("_camera");
        if (camera != null)
        {
            Vector2 center = new(station.Position.X + station.Width / 2f, station.Position.Y + station.Height / 2f);
            camera.Position = center - new Vector2(GraphicsDevice.Viewport.Width / (2f * camera.Zoom), GraphicsDevice.Viewport.Height / (2f * camera.Zoom));
        }
    }

    private void SetBuildMode(TrackBuildMode mode)
    {
        TrackBuilder? builder = GetGameplayField<TrackBuilder>("_builder");
        if (builder != null) builder.Mode = mode;
    }

    private void ToggleRouteEditMode()
    {
        InputManager? input = GetGameplayField<InputManager>("_inputManager");
        if (input == null) return;
        FieldInfo? modeField = typeof(InputManager).GetField("_wagonRouteEditMode", BindingFlags.Instance | BindingFlags.NonPublic);
        modeField?.SetValue(input, !(bool)(modeField.GetValue(input) ?? false));
    }

    private void ToggleTrainAutomation(Train train)
    {
        var schedule = train.LocomotiveSchedule;
        if (schedule == null) return;
        schedule.Enabled = !schedule.Enabled;
        if (!schedule.Enabled) RailwayDispatcher.ClearForceProceed(train);
    }

    private void OpenLocomotiveScheduleEditor()
    {
        if (_gameplayView == null || _gameplay == null || _myraUI.Desktop.Root != _gameplayView.Root) return;
        FieldInfo? selectedField = typeof(MyraGameplayView).GetField("_selectedTrain", BindingFlags.Instance | BindingFlags.NonPublic);
        if (selectedField?.GetValue(_gameplayView) is not Train train || train.Composition.Locomotive == null) return;
        var stations = GetGameplayField<TrainManager>("_trainManager")?.StationController;
        if (stations == null) return;
        var editor = new MyraLocomotiveScheduleView(train, stations, () => _myraUI.QueueAction(() => _myraUI.SetRoot(_gameplayView.Root)));
        _myraUI.SetRoot(editor.Root);
    }

    private void OpenRailwayDiagnostics()
    {
        if (_gameplayView == null || _gameplay == null || _myraUI.Desktop.Root != _gameplayView.Root) return;
        var manager = GetGameplayField<TrainManager>("_trainManager");
        var signals = GetGameplayField<SignalController>("_signalController");
        if (manager == null || signals == null) return;
        var view = new MyraRailwayDiagnosticsView(manager, signals, () => _myraUI.QueueAction(() => _myraUI.SetRoot(_gameplayView.Root)));
        _myraUI.SetRoot(view.Root);
    }

    private void OpenTrackInfrastructureEditor()
    {
        if (_gameplayView == null || _gameplay == null || _myraUI.Desktop.Root != _gameplayView.Root) return;
        Camera? camera = GetGameplayField<Camera>("_camera");
        GameMap? map = GetGameplayField<GameMap>("_map");
        TrackBuilder? builder = GetGameplayField<TrackBuilder>("_builder");
        if (camera == null || map == null || builder == null) return;
        MouseState mouse = Mouse.GetState();
        Vector2 world = camera.ScreenToWorld(new Vector2(mouse.X, mouse.Y));
        var position = new MapPosition((int)MathF.Floor(world.X), (int)MathF.Floor(world.Y));
        var editor = new MyraTrackInfrastructureView(map, builder, position, () => _myraUI.QueueAction(() => _myraUI.SetRoot(_gameplayView.Root)));
        _myraUI.SetRoot(editor.Root);
    }

    private void RefreshTimetableHud()
    {
        if (_gameplayView == null || _gameplay == null) return;
        var selectedField = typeof(MyraGameplayView).GetField("_selectedTrain", BindingFlags.Instance | BindingFlags.NonPublic);
        if (selectedField?.GetValue(_gameplayView) is not Train train) return;
        var detailField = typeof(MyraGameplayView).GetField("_selectionDetails", BindingFlags.Instance | BindingFlags.NonPublic);
        object? label = detailField?.GetValue(_gameplayView);
        if (label == null) return;
        var textProperty = label.GetType().GetProperty("Text");
        if (textProperty == null) return;

        string text = $"Vmax składu: {train.MaxSpeed * 3.6f:0.0} km/h\nCel prędkości: {train.EffectiveTargetSpeed * 3.6f:0.0} km/h\nKierunek: {train.Direction}  •  {RailwayDispatcher.Current?.GetStatus(train) ?? "DISPATCHER NIEDOSTĘPNY"}";
        var schedule = train.LocomotiveSchedule;
        var runtime = train.LocomotiveScheduleRuntime;
        if (schedule?.Enabled == true && schedule.Points.Count > 0)
        {
            if (runtime != null && runtime.CurrentPointIndex >= 0 && runtime.CurrentPointIndex < schedule.Points.Count)
            {
                int current = runtime.CurrentPointIndex;
                int next = (current + 1) % schedule.Points.Count;
                var currentPoint = schedule.Points[current];
                var nextPoint = schedule.Points[next];
                var stations = GetGameplayField<TrainManager>("_trainManager")?.StationController.Stations;
                string currentName = stations?.FirstOrDefault(s => s.Id == currentPoint.StationId)?.Name ?? "—";
                string nextName = stations?.FirstOrDefault(s => s.Id == nextPoint.StationId)?.Name ?? "—";
                int eta = runtime.GetExpectedArrival(schedule, next);
                text += $"\nRozkład: {schedule.Name} • stan: {runtime.State}\nPunkt: {current + 1}/{schedule.Points.Count} • {currentName}\nNastępny: {nextName}\nETA: {FormatClock(eta)} • odjazd: {FormatClock(runtime.GetExpectedDeparture(schedule, current))}\nOpóźnienie: {FormatDelay(runtime.DelaySeconds)} • propagowane: {FormatDelay(runtime.PropagatedDelaySeconds)}";
            }
            else text += $"\nRozkład: {schedule.Name} • oczekiwanie na pierwszy punkt";
        }
        textProperty.SetValue(label, text);
    }

    private static string FormatDelay(int seconds) => seconds == 0 ? "0 s" : $"{(seconds > 0 ? "+" : "−")}{Math.Abs(seconds) / 60}m {Math.Abs(seconds) % 60:D2}s";
    private static string FormatClock(int seconds)
    {
        int normalized = ((seconds % 86400) + 86400) % 86400;
        return $"{normalized / 3600:D2}:{normalized / 60 % 60:D2}";
    }

    private T? GetGameplayField<T>(string fieldName) where T : class
    {
        if (_gameplay == null) return null;
        FieldInfo? field = typeof(GameplayScreen).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(_gameplay) as T;
    }

    protected override void LoadContent() => _myraUI.Initialize(this);

    protected override void Update(GameTime gameTime)
    {
        _myraUI.Update(gameTime);
        KeyboardState keyboard = Keyboard.GetState();
        if (_gameplayView != null && _myraUI.Desktop.Root == _gameplayView.Root)
        {
            if (keyboard.IsKeyDown(Keys.F6) && _previousKeyboard.IsKeyUp(Keys.F6))
            {
                var selectedField = typeof(MyraGameplayView).GetField("_selectedTrain", BindingFlags.Instance | BindingFlags.NonPublic);
                if (selectedField?.GetValue(_gameplayView) is Train train) RailwayDispatcher.ForceProceed(train);
            }
            if (keyboard.IsKeyDown(Keys.F8) && _previousKeyboard.IsKeyUp(Keys.F8)) OpenTrackInfrastructureEditor();
            if (keyboard.IsKeyDown(Keys.F9) && _previousKeyboard.IsKeyUp(Keys.F9)) OpenLocomotiveScheduleEditor();
            if (keyboard.IsKeyDown(Keys.F10) && _previousKeyboard.IsKeyUp(Keys.F10)) OpenRailwayDiagnostics();
            _gameplayUiRefreshTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_gameplayUiRefreshTimer >= 0.5d)
            {
                _gameplayUiRefreshTimer = 0d;
                _gameplayView.Refresh();
                RefreshTimetableHud();
            }
        }
        _previousKeyboard = keyboard;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        _myraUI.Render();
    }
}
