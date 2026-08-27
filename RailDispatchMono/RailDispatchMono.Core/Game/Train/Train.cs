using System;

namespace RailDispatchMono.Core.Game.Train;

public sealed class Train
{
    public Guid Id { get; }

    public TrainComposition Composition { get; }

    public Train()
    {
        Id = Guid.NewGuid();
        Composition = new TrainComposition();
    }

    public bool CanMove =>
        Composition.CanMove;

    public float Length =>
        Composition.Length;
}
