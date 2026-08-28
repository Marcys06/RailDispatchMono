using System;
using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Game.Train;

public sealed class Train
{
    public Guid Id { get; }

    public TrainComposition Composition { get; }

    public float DistanceAlongTrack { get; set; }

    public float Speed { get; set; }

    public TrackConnections Direction { get; set; }

    private GameMap? _map;

    public Train()
    {
        Id =
            Guid.NewGuid();

        Composition =
            new TrainComposition();

        DistanceAlongTrack =
           10.5f;

        Speed =
            2.0f;

        Direction =
            TrackConnections.West;
    }

    public bool CanMove =>
        Composition.CanMove;

    public float Length =>
        Composition.Length;

    public void SetMap(
        GameMap map)
    {
        _map = map;
    }

    public void Update(
        float deltaTime)
    {
        if (!CanMove ||
            _map is null ||
            Speed <= 0f)
        {
            return;
        }

        var movement =
            Speed *
            deltaTime;

        if (Direction ==
            TrackConnections.East)
        {
            MoveEast(movement);
            return;
        }

        if (Direction ==
            TrackConnections.West)
        {
            MoveWest(movement);
            return;
        }
    }

    private void MoveEast(
        float distance)
    {
        var remaining =
            distance;

        while (remaining > 0f)
        {
            var currentX =
                (int)MathF.Floor(
                    DistanceAlongTrack);

            var mapPosition =
                new MapPosition(
                    currentX,
                    2);

            if (!_map!.TryGetTrack(
                    mapPosition,
                    out var track) ||
                track is null)
            {
                return;
            }

            if (!track.HasConnection(
                    TrackConnections.East))
            {
                return;
            }

            var distanceToNextCell =
                (currentX + 1f) -
                DistanceAlongTrack;

            var step =
                MathF.Min(
                    remaining,
                    distanceToNextCell);

            DistanceAlongTrack +=
                step;

            remaining -=
                step;
        }
    }

    private void MoveWest(
    float distance)
    {
        var remaining =
            distance;

        while (remaining > 0f)
        {
            var currentX =
                (int)MathF.Floor(
                    DistanceAlongTrack);

            var mapPosition =
                new MapPosition(
                    currentX,
                    2);

            if (!_map!.TryGetTrack(
                    mapPosition,
                    out var track) ||
                track is null)
            {
                return;
            }

            if (!track.HasConnection(
                    TrackConnections.West))
            {
                return;
            }

            var distanceToPreviousCell =
                DistanceAlongTrack -
                currentX;

            if (distanceToPreviousCell <= 0f)
            {
                DistanceAlongTrack =
                    currentX - 0.001f;

                continue;
            }

            var step =
                MathF.Min(
                    remaining,
                    distanceToPreviousCell);

            DistanceAlongTrack -=
                step;

            remaining -=
                step;
        }
    }

    public Vector2 GetHeadPosition()
    {
        return new Vector2(
            DistanceAlongTrack,
            2.5f);
    }

    public float GetVehicleDistance(
        int vehicleIndex)
    {
        if (vehicleIndex < 0 ||
            vehicleIndex >= Composition.Vehicles.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(vehicleIndex));
        }

        var distance =
            DistanceAlongTrack;

        for (var i = 0;
             i < vehicleIndex;
             i++)
        {
            distance -=
                Composition.Vehicles[i]
                    .Parameters.Length;
        }

        return distance;
    }
}