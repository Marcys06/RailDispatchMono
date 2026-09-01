using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Simulation;
using RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.Game.Rendering;

public sealed class TrainRenderer
{
    private Texture2D? _pixel;
    private TrainManager? _trainManager;

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void SetTrainManager(TrainManager trainManager)
    {
        _trainManager = trainManager;
    }

    public void Draw(SpriteBatch spriteBatch, TrainManager trainManager)
    {
        if (_pixel is null)
            return;

        foreach (var train in trainManager.Trains)
            DrawTrain(spriteBatch, train);
    }

    private void DrawTrain(SpriteBatch spriteBatch, global::RailDispatchMono.Core.Game.Train.Train train)
    {
        if (_pixel is null)
            return;

        const float vehicleWidth = 0.45f;
        var origin = new Vector2(0.5f, 0.5f);
        var positions = train.GetVehiclePositions();

        for (var i = 0; i < train.Composition.Vehicles.Count; i++)
        {
            var vehicle = train.Composition.Vehicles[i];
            Vector2 position = positions[i];
            float angle = train.GetVehicleTransform(i).Rotation;

            var color = vehicle is Locomotive
                ? Color.Red
                : Color.Blue;

            float worldLength = vehicle.Parameters.Length / SimulationScale.MetersPerGridCell;

            spriteBatch.Draw(
                _pixel,
                position,
                null,
                color,
                angle,
                origin,
                new Vector2(worldLength, vehicleWidth),
                SpriteEffects.None,
                0f);
        }
    }

    public (Train.Train train, int vehicleIndex, Vector2 worldPosition)? GetVehicleAtPosition(
        TrainManager trainManager,
        Vector2 worldPosition,
        float detectionRadius = 0.6f)
    {
        if (trainManager == null)
            return null;

        foreach (var train in trainManager.Trains)
        {
            var positions = train.GetVehiclePositions();
            for (int i = positions.Count - 1; i >= 0; i--)
            {
                float distance = Vector2.Distance(positions[i], worldPosition);
                if (distance < detectionRadius)
                    return (train, i, positions[i]);
            }
        }
        return null;
    }

    public Train.Train? GetTrainAtPosition(
        TrainManager trainManager,
        Vector2 worldPosition,
        float detectionRadius = 0.6f)
    {
        var result = GetVehicleAtPosition(trainManager, worldPosition, detectionRadius);
        return result?.train;
    }
}
