namespace RailDispatchMono.Core.Game.Railway;

/// <summary>Operational role of a track cell. Geometry is modelled separately by <see cref="TrackGeometry"/>.</summary>
public enum TrackType
{
    Mainline,
    Secondary,
    Siding,
    Platform
}
