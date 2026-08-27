using RailDispatchMono.Core.Game.Map;
using System;
namespace RailDispatchMono.Core.Game.Railway;

public readonly record struct JunctionPosition(
    TrackGeometry Geometry,
    TrackConnections Connections);




