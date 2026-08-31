using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Simulation;
using RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.Game.Save;

/// <summary>Infrastructure persistence service. In 0.0.16 it writes inside the active save slot.</summary>
public sealed class MapSaveService
{
    private const string SaveDirectoryName = "RailDispatchMono";
    private const string SaveFolderName = "Saves";
    private const string FileName = "map.json";
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string SaveDirectoryPath { get; }
    public string MapFilePath => Path.Combine(SaveDirectoryPath, FileName);

    public MapSaveService(string? rootDirectory = null)
    {
        SaveDirectoryPath = rootDirectory
            ?? SaveSlotContext.ActiveSlotDirectory
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), SaveDirectoryName, SaveFolderName);
    }

    public string Save(GameMap map, SignalController signals, StationController stations, DepotController depots)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));
        if (signals == null) throw new ArgumentNullException(nameof(signals));
        if (stations == null) throw new ArgumentNullException(nameof(stations));
        if (depots == null) throw new ArgumentNullException(nameof(depots));

        var data = new MapSaveData
        {
            GameVersion = "0.0.16",
            Map = new MapInfoSaveData { Width = map.Size.Width, Height = map.Size.Height }
        };

        foreach (var entry in map.GetAllTracks().OrderBy(x => x.Key.Y).ThenBy(x => x.Key.X))
        {
            var track = entry.Value;
            data.Tracks.Add(new TrackSaveData
            {
                X = track.Position.X,
                Y = track.Position.Y,
                Geometry = track.Geometry,
                Connections = track.Connections,
                SwitchPosition = track.CurrentSwitchPosition,
                CommonStem = track.CommonStem,
                StraightConnection = track.StraightConnection,
                DivergingConnection = track.DivergingConnection
            });
        }

        foreach (var signal in signals.GetAllSignals().OrderBy(s => s.Position.Y).ThenBy(s => s.Position.X))
        {
            data.Signals.Add(new SignalSaveData
            {
                Id = signal.Id,
                Name = signal.Name,
                X = signal.Position.X,
                Y = signal.Position.Y,
                Direction = signal.Direction,
                Aspect = signal.Aspect,
                AvailableAspects = signal.AvailableAspects.ToList(),
                IsLocked = signal.IsLocked
            });
        }

        foreach (var station in stations.Stations.OrderBy(s => s.Position.Y).ThenBy(s => s.Position.X))
        {
            data.Stations.Add(new StationSaveData
            {
                Id = station.Id,
                Name = station.Name,
                X = station.Position.X,
                Y = station.Position.Y,
                Width = station.Width,
                Height = station.Height,
                StopRadius = station.StopRadius,
                DwellTimeSeconds = station.DwellTimeSeconds,
                PassengerServiceEnabled = station.PassengerServiceEnabled,
                PassengerGenerationEnabled = station.PassengerGenerationEnabled,
                PassengerGenerationIntervalSeconds = station.PassengerGenerationIntervalSeconds,
                PassengerGenerationBatchSize = station.PassengerGenerationBatchSize,
                PassengerWaitingCapacity = station.PassengerWaitingCapacity
            });
        }

        foreach (var depot in depots.Depots.OrderBy(d => d.Position.Y).ThenBy(d => d.Position.X))
            data.Depots.Add(new DepotSaveData { Id = depot.Id, Name = depot.Name, X = depot.Position.X, Y = depot.Position.Y });

        Directory.CreateDirectory(SaveDirectoryPath);
        string tempPath = MapFilePath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(data, _jsonOptions));
        if (File.Exists(MapFilePath)) File.Replace(tempPath, MapFilePath, null);
        else File.Move(tempPath, MapFilePath);

        if (SaveSlotContext.ActiveSlotDirectory != null)
        {
            if (TrainManager.Current != null && GameClock.Current != null)
                RuntimeSaveService.Save(TrainManager.Current, GameClock.Current);
            SaveEmptyDocumentIfMissing("schedules.json");
            SaveEmptyDocumentIfMissing("passengers.json");
            SaveEmptyDocumentIfMissing("economy.json");
            SaveSlotService.Touch();
        }

        return MapFilePath;
    }

    private static void SaveEmptyDocumentIfMissing(string fileName)
    {
        string path = Path.Combine(SaveSlotContext.ActiveSlotDirectory!, fileName);
        if (!File.Exists(path)) File.WriteAllText(path, "{\n  \"schemaVersion\": 1\n}\n");
    }

    public bool Exists => File.Exists(MapFilePath);

    public void Load(GameMap map, SignalController signals, StationController stations, DepotController depots)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));
        if (signals == null) throw new ArgumentNullException(nameof(signals));
        if (stations == null) throw new ArgumentNullException(nameof(stations));
        if (depots == null) throw new ArgumentNullException(nameof(depots));
        if (!File.Exists(MapFilePath)) throw new FileNotFoundException("Map save not found.", MapFilePath);

        var data = JsonSerializer.Deserialize<MapSaveData>(File.ReadAllText(MapFilePath), _jsonOptions)
            ?? throw new InvalidDataException("map.json is empty or invalid.");
        if (data.SchemaVersion != 1) throw new InvalidDataException($"Unsupported map schema version: {data.SchemaVersion}.");
        if (data.Map.Width != map.Size.Width || data.Map.Height != map.Size.Height)
            throw new InvalidDataException($"Map size {data.Map.Width}x{data.Map.Height} does not match runtime map {map.Size.Width}x{map.Size.Height}.");

        map.Clear();
        signals.Clear();
        stations.Clear();
        depots.Clear();

        foreach (var saved in data.Tracks)
        {
            var position = new MapPosition(saved.X, saved.Y);
            if (!map.IsInside(position)) throw new InvalidDataException($"Track outside map: ({saved.X},{saved.Y}).");
            var track = new TrackCell(position, saved.Geometry, saved.Connections);
            if (saved.Geometry == TrackGeometry.Junction)
                track.ConfigureJunction(saved.CommonStem, saved.StraightConnection, saved.DivergingConnection);
            track.SetSwitchPosition(saved.SwitchPosition);
            map.AddTrack(track);
        }

        foreach (var saved in data.Signals)
        {
            var signal = new Signal(saved.Id, new MapPosition(saved.X, saved.Y), saved.Direction, saved.AvailableAspects);
            signal.Name = saved.Name;
            if (saved.AvailableAspects.Contains(saved.Aspect)) signal.SetAspect(saved.Aspect);
            signal.IsLocked = saved.IsLocked;
            signals.AddSignal(signal);
        }

        foreach (var saved in data.Stations)
        {
            var station = new Station(saved.Id, saved.Name, new MapPosition(saved.X, saved.Y), saved.Width, saved.Height)
            {
                StopRadius = saved.StopRadius,
                DwellTimeSeconds = saved.DwellTimeSeconds,
                PassengerServiceEnabled = saved.PassengerServiceEnabled,
                PassengerGenerationEnabled = saved.PassengerGenerationEnabled,
                PassengerGenerationIntervalSeconds = saved.PassengerGenerationIntervalSeconds,
                PassengerGenerationBatchSize = saved.PassengerGenerationBatchSize,
                PassengerWaitingCapacity = saved.PassengerWaitingCapacity
            };
            stations.AddStation(station);
        }

        foreach (var saved in data.Depots)
            depots.AddDepot(new Depot(saved.Id, saved.Name, new MapPosition(saved.X, saved.Y)));

        if (SaveSlotContext.ActiveSlotDirectory != null && TrainManager.Current != null && GameClock.Current != null)
        {
            string trainPath = RuntimeSaveService.FilePath;
            if (File.Exists(trainPath))
            {
                var blocks = TrainManager.Current.BlockController;
                if (blocks != null)
                    RuntimeSaveService.Load(TrainManager.Current, signals, blocks, stations, GameClock.Current);
            }
        }
    }
}
