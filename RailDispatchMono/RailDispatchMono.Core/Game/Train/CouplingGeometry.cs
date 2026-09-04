using Microsoft.Xna.Framework;
using System;

namespace RailDispatchMono.Core.Game.Train;

public static class CouplingGeometry
{
    public const float DefaultCouplingDistance = 0.12f;
    public const float DefaultAlignmentDot = 0.94f;

    public static Vector2 GetEndpoint(Train train, int vehicleIndex, VehicleEnd end)
    {
        var transform = train.GetVehicleTransform(vehicleIndex);
        Vector2 forward = new(MathF.Cos(transform.Rotation), MathF.Sin(transform.Rotation));
        float halfLength = train.Composition.Vehicles[vehicleIndex].Parameters.Length * 0.5f;

        return transform.Position + (end == VehicleEnd.Front ? forward : -forward) * halfLength;
    }

    public static Vector2 GetOutwardDirection(Train train, int vehicleIndex, VehicleEnd end)
    {
        var transform = train.GetVehicleTransform(vehicleIndex);
        Vector2 forward = new(MathF.Cos(transform.Rotation), MathF.Sin(transform.Rotation));
        return end == VehicleEnd.Front ? forward : -forward;
    }

    public static bool AreFacing(Train firstTrain, int firstIndex, VehicleEnd firstEnd,
        Train secondTrain, int secondIndex, VehicleEnd secondEnd,
        float minimumDot = DefaultAlignmentDot)
    {
        Vector2 firstToSecond = GetEndpoint(secondTrain, secondIndex, secondEnd) -
                                GetEndpoint(firstTrain, firstIndex, firstEnd);
        if (firstToSecond.LengthSquared() < 0.000001f)
            return true;

        firstToSecond.Normalize();
        Vector2 secondToFirst = -firstToSecond;
        return Vector2.Dot(GetOutwardDirection(firstTrain, firstIndex, firstEnd), firstToSecond) >= minimumDot &&
               Vector2.Dot(GetOutwardDirection(secondTrain, secondIndex, secondEnd), secondToFirst) >= minimumDot;
    }
}
