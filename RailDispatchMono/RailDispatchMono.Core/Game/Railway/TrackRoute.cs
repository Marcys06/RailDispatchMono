using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;

namespace RailDispatchMono.Core.Game.Railway;

public sealed class TrackRoute
{
    private readonly List<MapPosition> _cells = new();

    public IReadOnlyList<MapPosition> Cells =>
        _cells;

    public float Length =>
        _cells.Count;

    public void AddCell(
        MapPosition position)
    {
        _cells.Add(position);
    }

    public Vector2 GetPosition(
        float distance)
    {
        if (_cells.Count == 0)
            throw new InvalidOperationException(
                "TrackRoute contains no cells.");

        var index =
            (int)MathF.Floor(distance);

        index =
            Math.Clamp(
                index,
                0,
                _cells.Count - 1);

        var position =
            _cells[index];

        return new Vector2(
            position.X + 0.5f,
            position.Y + 0.5f);
    }
}
