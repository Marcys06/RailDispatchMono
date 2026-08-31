using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Game.Save;

/// <summary>
/// Writes the current map/infrastructure state to map.json.
/// Loading is intentionally not implemented until 0.0.15a's follow-up work.
/// </summary>
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
        SaveDirectoryPath = rootDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            SaveDirectoryName,
            SaveFolderName);
    }

    public string Save(GameMap map, SignalController signals, StationController stations, DepotController depots)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));
        if (signals == null) throw new ArgumentNullException(nameof(signals));
        if (stations == null) throw new ArgumentNullException(nameof(stations));
        if (depots == null) throw new ArgumentNullException(nameof(depots));

        var data = new MapSaveData
        {
            Map = new MapInfoSaveData
            {
                Width = map.Size.Width,
                Height = map.Size.Height
            }
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
        {
            data.Depots.Add(new DepotSaveData
            {
                Id = depot.Id,
                Name = depot.Name,
                X = depot.Position.X,
                Y = depot.Position.Y
            });
        }

        Directory.CreateDirectory(SaveDirectoryPath);
        string tempPath = MapFilePath + ".tmp";
        string json = JsonSerializer.Serialize(data, _jsonOptions);
        File.WriteAllText(tempPath, json);

        if (File.Exists(MapFilePath))
            File.Replace(tempPath, MapFilePath, null);
        else
            File.Move(tempPath, MapFilePath);

        return MapFilePath;
    }
}
