// TrainRenderer.cs
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.Game.Rendering;

public sealed class TrainRenderer
{
    private Texture2D? _pixel;
    private SpriteFont? _labelFont;
    private TrainManager? _trainManager;

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void SetFont(SpriteFont font) => _labelFont = font;

    public void SetTrainManager(TrainManager trainManager) => _trainManager = trainManager;

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
        const float labelScale = 0.016f;
        var origin = new Vector2(0.5f, 0.5f);
        var positions = train.GetVehiclePositions();

        for (var i = 0; i < train.Composition.Vehicles.Count; i++)
        {
            var vehicle = train.Composition.Vehicles[i];
            Vector2 position = positions[i];
            float angle = train.GetVehicleTransform(i).Rotation;

            Color color;
            string label;
            if (vehicle is Locomotive locomotive)
            {
                color = locomotive.Type == LocomotiveType.Diesel ? Color.Black : Color.Red;
                label = locomotive.ShortName;
            }
            else if (vehicle is Wagon wagon)
            {
                color = wagon.ShortName switch
                {
                    "1KL" => Color.Blue,
                    "2KL" => Color.LightBlue,
                    "3KL" => new Color(20, 45, 120),
                    "CARGO" => new Color(120, 75, 35),
                    _ => Color.Blue
                };
                label = wagon.ShortName;
            }
            else
            {
                color = Color.Gray;
                label = string.Empty;
            }

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

            if (_labelFont != null && !string.IsNullOrWhiteSpace(label))
                DrawVehicleLabel(spriteBatch, label, position, angle, labelScale);
        }
    }

    private void DrawVehicleLabel(SpriteBatch spriteBatch, string label, Vector2 position, float angle, float scale)
    {
        if (_labelFont == null)
            return;

        Vector2 textSize = _labelFont.MeasureString(label);
        float labelAngle = NormalizeLabelAngle(angle);

        spriteBatch.DrawString(
            _labelFont,
            label,
            position,
            Color.White,
            labelAngle,
            textSize * 0.5f,
            scale,
            SpriteEffects.None,
            0.01f);
    }

    private static float NormalizeLabelAngle(float angle)
    {
        if ((float)Math.Cos(angle) < 0f)
            angle += (float)Math.PI;
        return angle;
    }

    public (global::RailDispatchMono.Core.Game.Train.Train train, int vehicleIndex, Vector2 worldPosition)? GetVehicleAtPosition(
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

    public global::RailDispatchMono.Core.Game.Train.Train? GetTrainAtPosition(
        TrainManager trainManager,
        Vector2 worldPosition,
        float detectionRadius = 0.6f)
    {
        var result = GetVehicleAtPosition(trainManager, worldPosition, detectionRadius);
        return result?.train;
    }
}