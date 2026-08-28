using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.Game.Rendering;

public sealed class TrainRenderer
{
    private Texture2D? _pixel;

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Draw(SpriteBatch spriteBatch, TrainManager trainManager)
    {
        if (_pixel is null)
            return;

        foreach (var train in trainManager.Trains)
        {
            DrawTrain(spriteBatch, train);
        }
    }

    private void DrawTrain(SpriteBatch spriteBatch, global::RailDispatchMono.Core.Game.Train.Train train)
    {
        if (_pixel is null)
            return;

        const float vehicleWidth = 0.45f;
        var origin = new Vector2(0.5f, 0.5f);

        for (var i = 0; i < train.Composition.Vehicles.Count; i++)
        {
            var vehicle = train.Composition.Vehicles[i];

            // Get exact world-space position and rotation angle along the trajectory
            var transform = train.GetVehicleTransform(i);
            Vector2 position = transform.Position;
            float angle = transform.Rotation;

            var color = vehicle is Locomotive
                ? Color.Red
                : Color.Blue;

            spriteBatch.Draw(
                _pixel,
                position,
                null,
                color,
                angle,
                origin,
                new Vector2(vehicle.Parameters.Length, vehicleWidth),
                SpriteEffects.None,
                0f);
        }
    }
}