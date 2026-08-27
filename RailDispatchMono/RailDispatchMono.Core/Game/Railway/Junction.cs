using RailDispatchMono.Core.Game.Map;
using System.Collections.Generic;
using System;
namespace RailDispatchMono.Core.Game.Railway;

public sealed class Junction
{
    public MapPosition Position { get; }

    public IReadOnlyList<JunctionPosition> AvailablePositions { get; }

    public int CurrentPositionIndex { get; private set; }

    public JunctionPosition Current =>
        AvailablePositions[CurrentPositionIndex];

    public Junction(
        MapPosition position,
        IReadOnlyList<JunctionPosition> availablePositions)
    {
        if (availablePositions.Count < 2)
        {
            throw new ArgumentException(
                "Junction requires at least two available positions.",
                nameof(availablePositions));
        }

        Position = position;
        AvailablePositions = availablePositions;
        CurrentPositionIndex = 0;
    }

    public void ToggleNext()
    {
        CurrentPositionIndex =
            (CurrentPositionIndex + 1) %
            AvailablePositions.Count;
    }

    public void SwitchTo(int index)
    {
        if (index < 0 || index >= AvailablePositions.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        CurrentPositionIndex = index;
    }
}




