using System;
namespace RailDispatchMono.Core.Game.Railway;

[Flags]
public enum TrackConnections
{
    None = 0,

    North = 1,
    East = 2,
    South = 4,
    West = 8
}







