using System;
using Microsoft.Xna.Framework;

namespace RailDispatchMono.Core.Game.Train;

public sealed class Train
{
    public Guid Id { get; }

    public TrainComposition Composition { get; }

    public Vector2 Position { get; set; }

    public float Speed { get; set; }

    public Train()
    {
        Id = Guid.NewGuid();

        Composition =
            new TrainComposition();

        Position =
            new Vector2(
                2.5f,
                2.5f);

        Speed = 2.0f;
    }

    public bool CanMove =>
        Composition.CanMove;

    public float Length =>
        Composition.Length;

    public void Update(
        float deltaTime)
    {
        if (!CanMove)
            return;

        Position +=
            Vector2.UnitX *
            Speed *
            deltaTime;
    }
}
