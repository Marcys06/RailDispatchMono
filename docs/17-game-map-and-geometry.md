# Game map and geometry

The Core game area contains geometry/map support in addition to railway and train domains.

## Confirmed files

- `Game/Map/MapSize.cs` — map-size representation.
- `Game/Layer.cs` — game-layer representation.
- `Game/Circle.cs` — circular geometry/helper type.
- `Game/RectangleExtensions.cs` — rectangle extension/helper methods.
- `Game/FaceDirection.cs` — facing/direction type.

## Geometry discipline

Do not create ad-hoc geometry structs when an existing type already represents the required concept. Search `Game/` for related helpers first.

Keep coordinate-space conversions explicit. The rendering/input system has a logical presentation space distinct from the physical backbuffer.

## Map and rendering boundary

Map geometry is a domain/game concern. Camera and rendering are presentation concerns. A map object should not acquire responsibility for platform window management merely because it is drawn on screen.

## AI workflow

For map-related changes:

1. inspect `MapSize` and all consumers;
2. inspect layer/geometry helpers;
3. inspect `Camera` for rendering-space behavior;
4. inspect railway classes that reference map positions;
5. inspect gameplay screens that render or manipulate the map;
6. preserve existing coordinate conventions.

## Caution

The current inventory does not by itself prove the exact semantics of every geometry class. The class implementation and call sites remain the source of truth for units, origins, orientation conventions and map topology.
