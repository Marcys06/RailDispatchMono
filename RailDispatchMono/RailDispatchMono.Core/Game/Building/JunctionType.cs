namespace RailDispatchMono.Core.Game.Building;

/// <summary>
/// All twelve possible three-way junction orientations.
/// The first direction is the common stem; the two remaining directions
/// are the straight and diverging exits respectively.
/// </summary>
public enum JunctionType
{
    // Common stem: South
    South_NorthEast,
    South_NorthWest,
    South_EastWest,

    // Common stem: North
    North_SouthEast,
    North_SouthWest,
    North_EastWest,

    // Common stem: East
    East_WestNorth,
    East_WestSouth,
    East_NorthSouth,

    // Common stem: West
    West_EastNorth,
    West_EastSouth,
    West_NorthSouth
}
