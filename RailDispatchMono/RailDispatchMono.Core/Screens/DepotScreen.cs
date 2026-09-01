using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.RollingStock;
using RailDispatchMono.Core.Game.Train;
using RailDispatchMono.Core.ScreenManagers;
using RailDispatchMono.Core.UI.Myra;
using System;

namespace RailDispatchMono.Core.Screens;

public sealed class DepotScreen : GameScreen
{
    private readonly TrainManager _trainManager;
    private readonly SignalController _signalController;
    private readonly BlockController _blockController;
    private readonly Depot _depot;
    private readonly MyraDepotView _view;
    private readonly MyraUIManager _myra;

    public TrainComposition Composition { get; } = new();
    public LocomotiveDefinition? SelectedLocomotive { get; private set; }
    public string StatusMessage { get; private set; } = string.Empty;

    public DepotScreen(
        TrainManager trainManager,
        SignalController signalController,
        BlockController blockController,
        Depot depot,
        MyraUIManager myra)
    {
        _trainManager = trainManager ?? throw new ArgumentNullException(nameof(trainManager));
        _signalController = signalController ?? throw new ArgumentNullException(nameof(signalController));
        _blockController = blockController ?? throw new ArgumentNullException(nameof(blockController));
        _depot = depot ?? throw new ArgumentNullException(nameof(depot));
        _myra = myra ?? throw new ArgumentNullException(nameof(myra));
        _view = new MyraDepotView(this);
    }

    public override void LoadContent()
    {
        base.LoadContent();
        _myra.SetRoot(_view.Root);
    }

    public override void UnloadContent()
    {
        _myra.Clear();
        base.UnloadContent();
    }

    public void SelectLocomotive(LocomotiveDefinition definition)
    {
        SelectedLocomotive = definition;
        Composition.SetLocomotive(definition);
        StatusMessage = $"Wybrano: {definition.DisplayName}";
        _view.Refresh();
    }

    public void AddWagon(WagonDefinition definition)
    {
        Composition.AddWagon(definition);
        StatusMessage = $"Dodano: {definition.DisplayName}";
        _view.Refresh();
    }

    public void RemoveWagon(int index)
    {
        if (Composition.RemoveWagon(index))
            StatusMessage = "Wagon usunięty.";
        _view.Refresh();
    }

    public void ClearWagons()
    {
        for (int i = Composition.Vehicles.Count - 1; i >= 0; i--)
            if (Composition.Vehicles[i] is Wagon)
                Composition.RemoveWagon(i);
        StatusMessage = "Wagony usunięte.";
        _view.Refresh();
    }

    public void CreateTrain()
    {
        if (!Composition.CanMove)
        {
            StatusMessage = "Wybierz lokomotywę.";
            _view.Refresh();
            return;
        }

        if (!TryFindSpawn(out MapPosition spawnCell, out TrackConnections direction))
        {
            StatusMessage = "Brak wolnego toru przy depocie.";
            _view.Refresh();
            return;
        }

        var train = _trainManager.CreateTrainFromComposition(Composition, spawnCell, direction, 0f);
        train.SetSignalController(_signalController);
        train.SetBlockController(_blockController);
        StatusMessage = $"Utworzono skład {train.Id.ToString()[..8]}.";
        Close();
    }

    public void Close()
    {
        ExitScreen();
    }

    private bool TryFindSpawn(out MapPosition spawnCell, out TrackConnections direction)
    {
        var candidates = new[]
        {
            (new MapPosition(_depot.Position.X + 1, _depot.Position.Y), TrackConnections.East),
            (new MapPosition(_depot.Position.X - 1, _depot.Position.Y), TrackConnections.West),
            (new MapPosition(_depot.Position.X, _depot.Position.Y + 1), TrackConnections.South),
            (new MapPosition(_depot.Position.X, _depot.Position.Y - 1), TrackConnections.North)
        };

        foreach (var candidate in candidates)
        {
            if (_trainManager.Map.TryGetTrack(candidate.Item1, out TrackCell? track) && track != null && !_trainManager.IsCellOccupied(candidate.Item1))
            {
                spawnCell = candidate.Item1;
                direction = candidate.Item2;
                return true;
            }
        }

        spawnCell = default;
        direction = TrackConnections.East;
        return false;
    }
}
