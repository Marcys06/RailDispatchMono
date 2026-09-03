using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Passengers;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Simulation;
using RailDispatchMono.Core.Game.Train;
using TrainModel = RailDispatchMono.Core.Game.Train.Train;
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
    public string GameVersion { get; set; } = "0.1.5pre";
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
    public bool IsReversed { get; set; }
    public List<VehicleSaveData> Vehicles { get; set; } = new();
}

public sealed class VehicleSaveData
{
    public string Kind { get; set; } = "Wagon";
    public string Type { get; set; } = "Passenger";
    public string ShortName { get; set; } = "";
    public float MaxSpeed { get; set; }
    public float Mass { get; set; }
    public float Length { get; set; }
    public float MassTons { get; set; }
    public float LengthMeters { get; set; }
    public float Acceleration { get; set; }
    public float Braking { get; set; }
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

public static class RuntimeSaveService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string FilePath => Path.Combine(
        SaveSlotContext.ActiveSlotDirectory ?? throw new InvalidOperationException("No active save slot."),
        "trains.json");

    public static void Save(TrainManager trainManager, GameClock clock)
    {
        if (trainManager == null) throw new ArgumentNullException(nameof(trainManager));
        if (clock == null) throw new ArgumentNullException(nameof(clock));

        var data = new RuntimeSaveData { GameDay = clock.GameDay, GameTimeSeconds = clock.Seconds };
        var passengers = new List<PassengerSaveData>();

        foreach (TrainModel train in trainManager.Trains)
        {
            var savedTrain = new TrainSaveData
            {
                Id = train.Id,
                X = train.Position.X,
                Y = train.Position.Y,
                Speed = train.Speed,
                DistanceAlongTrack = train.DistanceAlongTrack,
                Direction = train.Direction,
                IsReversed = train.IsReversed
            };

            foreach (Vehicle vehicle in train.Composition.Vehicles)
            {
                var p = vehicle.Parameters;
                var savedVehicle = new VehicleSaveData
                {
                    Kind = vehicle is Locomotive ? "Locomotive" : "Wagon",
                    Type = vehicle is Locomotive l ? l.Type.ToString() : ((Wagon)vehicle).WagonType.ToString(),
                    ShortName = vehicle is Locomotive locomotive
                        ? locomotive.ShortName
                        : ((Wagon)vehicle).ShortName,
                    MaxSpeed = p.MaxSpeed,
                    Mass = p.Mass,
                    Length = p.Length,
                    MassTons = p.MassTons,
                    LengthMeters = p.LengthMeters,
                    Acceleration = p.Acceleration,
                    Braking = p.Braking,
                    MassCoefficient = p.MassCoefficient,
                    TechnicalCondition = p.TechnicalCondition,
                    AccelerationCoefficient = p.AccelerationCoefficient,
                    BrakingCoefficient = p.BrakingCoefficient,
                    Orientation = vehicle.Orientation
                };

                if (vehicle is Wagon wagon)
                {
                    savedVehicle.PassengerCapacity = wagon.PassengerCapacity;
                    savedVehicle.ServiceRoute = wagon.ServiceRoute.ToList();
                    foreach (Passenger passenger in wagon.Passengers)
                    {
                        var snapshot = new PassengerSaveData
                        {
                            OriginStationId = passenger.OriginStation.Id,
                            DestinationStationId = passenger.DestinationStation.Id,
                            State = passenger.State
                        };
                        savedVehicle.Passengers.Add(snapshot);
                        passengers.Add(snapshot);
                    }
                }
                savedTrain.Vehicles.Add(savedVehicle);
            }
            data.Trains.Add(savedTrain);
        }

        Directory.CreateDirectory(SaveSlotContext.ActiveSlotDirectory!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(data, Options));
        File.WriteAllText(Path.Combine(SaveSlotContext.ActiveSlotDirectory!, "passengers.json"),
            JsonSerializer.Serialize(new { schemaVersion = 1, passengers }, Options));
    }

    public static void Load(TrainManager trainManager, SignalController signals, BlockController blocks, StationController stations, GameClock clock)
    {
        if (!File.Exists(FilePath)) throw new FileNotFoundException("trains.json is missing.", FilePath);
        var data = JsonSerializer.Deserialize<RuntimeSaveData>(File.ReadAllText(FilePath), Options)
            ?? throw new InvalidDataException("trains.json is empty or invalid.");
        if (data.SchemaVersion != 1) throw new InvalidDataException($"Unsupported trains schema version: {data.SchemaVersion}.");

        trainManager.ClearAll();
        trainManager.Update(0f);

        foreach (TrainSaveData savedTrain in data.Trains)
        {
            var vehicles = new List<Vehicle>();
            foreach (VehicleSaveData savedVehicle in savedTrain.Vehicles)
            {
                VehicleParameters p;
                if (savedVehicle.MassTons > 0f && savedVehicle.LengthMeters > 0f)
                {
                    p = VehicleParameters.CreatePhysical(
                        savedVehicle.MaxSpeed * 3.6f,
                        savedVehicle.Acceleration,
                        savedVehicle.Braking,
                        savedVehicle.MassTons,
                        savedVehicle.LengthMeters,
                        savedVehicle.Length,
                        savedVehicle.MassCoefficient,
                        savedVehicle.TechnicalCondition);
                }
                else
                {
                    p = new VehicleParameters(
                        savedVehicle.MaxSpeed,
                        savedVehicle.AccelerationCoefficient,
                        savedVehicle.BrakingCoefficient,
                        savedVehicle.Mass,
                        savedVehicle.Length,
                        savedVehicle.MassCoefficient,
                        savedVehicle.TechnicalCondition);
                }

                Vehicle vehicle = string.Equals(savedVehicle.Kind, "Locomotive", StringComparison.OrdinalIgnoreCase)
                    ? new Locomotive(
                        Enum.Parse<LocomotiveType>(savedVehicle.Type, true),
                        p,
                        savedVehicle.ShortName)
                    : new Wagon(
                        p,
                        savedVehicle.ShortName,
                        Enum.Parse<WagonType>(savedVehicle.Type, true),
                        savedVehicle.PassengerCapacity,
                        savedVehicle.ServiceRoute);
                vehicle.Orientation = savedVehicle.Orientation;
                vehicles.Add(vehicle);
            }

            var train = new TrainModel(new Vector2(savedTrain.X, savedTrain.Y), savedTrain.Direction, savedTrain.Speed, vehicles);
            train.SetMap(trainManager.Map);
            train.SetSignalController(signals);
            train.SetBlockController(blocks);
            train.DistanceAlongTrack = savedTrain.DistanceAlongTrack;
            train.RestoreTravelDirection(savedTrain.IsReversed);
            trainManager.Add(train);
        }

        trainManager.Update(0f);

        foreach (TrainSaveData savedTrain in data.Trains)
        {
            TrainModel? train = trainManager.Trains.FirstOrDefault(x => x.Position == new Vector2(savedTrain.X, savedTrain.Y));
            if (train == null) continue;
            for (int i = 0; i < savedTrain.Vehicles.Count && i < train.Composition.Vehicles.Count; i++)
            {
                if (train.Composition.Vehicles[i] is not Wagon wagon) continue;
                foreach (PassengerSaveData savedPassenger in savedTrain.Vehicles[i].Passengers)
                {
                    if (savedPassenger.State != PassengerState.OnBoard) continue;
                    Station? origin = stations.Stations.FirstOrDefault(s => s.Id == savedPassenger.OriginStationId);
                    Station? destination = stations.Stations.FirstOrDefault(s => s.Id == savedPassenger.DestinationStationId);
                    if (origin == null || destination == null) continue;
                    var passenger = new Passenger(origin, destination);
                    wagon.TryBoard(passenger, train.Id);
                }
            }
        }

        clock.SetTime(data.GameDay, data.GameTimeSeconds);
    }
}
