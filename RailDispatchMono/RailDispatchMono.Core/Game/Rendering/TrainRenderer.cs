using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.Game.Rendering;

public sealed class TrainRenderer
{
    private Texture2D? _pixel;

    public void LoadContent(
        GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(
            graphicsDevice,
            1,
            1);

        _pixel.SetData(
            new[] { Color.White });
    }

    public void Draw(
        SpriteBatch spriteBatch,
        TrainManager trainManager)
    {
        if (_pixel is null)
        {
            return;
        }

        foreach (var train in trainManager.Trains)
        {
            DrawTrain(
                spriteBatch,
                train);
        }
    }

    private void DrawTrain(
        SpriteBatch spriteBatch,
        RailDispatchMono.Core.Game.Train.Train train)
    {
        const float vehicleLength = 0.7f;
        const float vehicleWidth = 0.45f;
        const float spacing = 0.1f;

        var x =
            train.Position.X;

        var centerY =
            train.Position.Y;

        foreach (var vehicle in train.Composition.Vehicles)
        {
            var color =
                vehicle is Locomotive
                    ? Color.Red
                    : Color.Blue;

            spriteBatch.Draw(
                _pixel,
                new Vector2(
                    x,
                    centerY),
                null,
                color,
                0f,
                new Vector2(
                    0.5f,
                    0.5f),
                new Vector2(
                    vehicleLength,
                    vehicleWidth),
                SpriteEffects.None,
                0f);

            x +=
                vehicleLength +
                spacing;
        }
    }
}
