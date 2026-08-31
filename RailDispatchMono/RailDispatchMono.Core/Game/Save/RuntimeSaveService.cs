using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Simulation;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RailDispatchMono.Core.Game.Save;

public sealed class RuntimeSaveData
{
    public int SchemaVersion { get; set; } = 1;
    public string GameVersion { get; set; } = "0.0.16";
    public int GameDay { get; set; } = 1;
    public double GameTimeSeconds { get; set; }
    public List<TrainSaveData> Trains { get; set; } = new();
}

public sealed class TrainSaveData
{
    public Guid Id { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Speed { get; set; }
    public float DistanceAlongTrack { get; set; }
    public TrackConnections Direction { get; set; }
    public List<VehicleSaveData> Vehicles { get; set; } = new();
}

public sealed class VehicleSaveData
{
    public string Kind { get; set; } = "Wagon";
    public string Type { get; set; } = "Passenger";
    public float MaxSpeed { get; set; }
    public float Mass { get; set; }
    public float Length { get; set; }
    public float MassCoefficient { get; set; }
    public float TechnicalCondition { get; set; }
    public float AccelerationCoefficient { get; set; }
    public float BrakingCoefficient { get; set; }
    public VehicleOrientation Orientation { get; set; }
    public int PassengerCapacity { get; set; }
    public List<Guid> ServiceRoute { get; set; } = new();
    public List<PassengerSaveData> Passengers { get; set; } = new();
}

public sealed class PassengerSaveData
{
    public Guid OriginStationId { get; set; }
    public Guid DestinationStationId { get; set; }
    public PassengerState State { get; set; }
}

/// <summary>Persists the runtime part of a save slot independently of map infrastructure.</summary>
public static class RuntimeSaveService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private const string FileName = "trains.json";

    public static string FilePath => Path.Combine(
        SaveSlotContext.ActiveSlotDirectory ?? throw new InvalidOperationException("No active save slot."),
        FileName);

    public static void Save(TrainManager trainManager, GameClock clock)
    {
        if (trainManager == null) throw new ArgumentNullException(nameof(trainManager));
        if (clock == null) throw new ArgumentNullException(nameof(clock));

        var data = new RuntimeSaveData
        {
            GameDay = clock.GameDay,
            GameTimeSeconds = clock.Seconds
        };

        foreach (Train train in trainManager.Trains)
        {
            var savedTrain = new TrainSaveData
            {
                Id = train.Id,
                X = train.Position.X,
                Y = train.Position.Y,
                Speed = train.Speed,
                DistanceAlongTrack = train.DistanceAlongTrack,
                Direction = train.Direction
            };

            foreach (Vehicle vehicle in train.Composition.Vehicles)
            {
                var parameters = vehicle.Parameters;
                var savedVehicle = new VehicleSaveData
                {
                    Kind = vehicle is Locomotive ? "Locomotive" : "Wagon",
                    Type = vehicle is Locomotive locomotive
                        ? locomotive.Type.ToString()
                        : ((Wagon)vehicle).WagonType.ToString(),
                    MaxSpeed = parameters.MaxSpeed,
                    Mass = parameters.Mass,
                    Length = parameters.Length,
                    MassCoefficient = parameters.MassCoefficient,
                    TechnicalCondition = parameters.TechnicalCondition,
                    AccelerationCoefficient = parameters.AccelerationCoefficient,
                    BrakingCoefficient = parameters.BrakingCoefficient,
                    Orientation = vehicle.Orientation
                };

                if (vehicle is Wagon wagon)
                {
                    savedVehicle.PassengerCapacity = wagon.PassengerCapacity;
                    savedVehicle.ServiceRoute = wagon.ServiceRoute.ToList();
                    foreach (Passenger passenger in wagon.Passengers)
                    {
                        savedVehicle.Passengers.Add(new PassengerSaveData
                        {
                            OriginStationId = passenger.OriginStation.Id,
                            DestinationStationId = passenger.DestinationStation.Id,
                            State = passenger.State
                        });
                    }
                }

                savedTrain.Vehicles.Add(savedVehicle);
            }

            data.Trains.Add(savedTrain);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(data, Options));
    }

    public static void Load(TrainManager trainManager, SignalController signals, BlockController blocks, StationController stations, GameClock clock)
    {
        if (!File.Exists(FilePath)) throw new FileNotFoundException("trains.json is missing.", FilePath);

        var data = JsonSerializer.Deserialize<RuntimeSaveData>(File.ReadAllText(FilePath), Options)
            ?? throw new InvalidDataException("trains.json is empty or invalid.");
        if (data.SchemaVersion != 1)
            throw new InvalidDataException($"Unsupported trains schema version: {data.SchemaVersion}.");

        trainManager.ClearAll();
        trainManager.Update(0f);

        foreach (TrainSaveData savedTrain in data.Trains)
        {
            var vehicles = new List<Vehicle>();
            foreach (VehicleSaveData savedVehicle in savedTrain.Vehicles)
            {
                var parameters = new VehicleParameters(
                    savedVehicle.MaxSpeed,
                    savedVehicle.AccelerationCoefficient,
                    savedVehicle.BrakingCoefficient,
                    savedVehicle.Mass,
                    savedVehicle.Length,
                    savedVehicle.MassCoefficient,
                    savedVehicle.TechnicalCondition);

                Vehicle vehicle;
                if (string.Equals(savedVehicle.Kind, "Locomotive", StringComparison.OrdinalIgnoreCase))
                {
                    var type = Enum.Parse<LocomotiveType>(savedVehicle.Type, true);
                    vehicle = new Locomotive(type, parameters);
                }
                else
                {
                    var type = Enum.Parse<WagonType>(savedVehicle.Type, true);
                    vehicle = new Wagon(parameters, type, savedVehicle.PassengerCapacity, savedVehicle.ServiceRoute);
                }
                vehicle.Orientation = savedVehicle.Orientation;
                vehicles.Add(vehicle);
            }

            var train = new Train(new Vector2(savedTrain.X, savedTrain.Y), savedTrain.Direction, savedTrain.Speed, vehicles);
            train.SetMap(GetMap(trainManager));
            train.SetSignalController(signals);
            train.SetBlockController(blocks);
            train.DistanceAlongTrack = savedTrain.DistanceAlongTrack;
            trainManager.Add(train);
        }

        trainManager.Update(0f);
        clock.SetTime(data.GameDay, data.GameTimeSeconds);
    }

    private static RailDispatchMono.Core.Game.Map.GameMap GetMap(TrainManager manager)
    {
        var field = typeof(TrainManager).GetField("_map", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("TrainManager map field is unavailable.");
        return (RailDispatchMono.Core.Game.Map.GameMap)(field.GetValue(manager)
            ?? throw new InvalidOperationException("TrainManager map is unavailable."));
    }
}
