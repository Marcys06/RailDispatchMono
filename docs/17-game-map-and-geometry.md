# Game map and geometry

The Core game area contains geometry/map support in addition to railway and train domains.

## Current train-geometry contract

The `0.1.6pre` train model treats physical vehicle placement and travelled trajectory as separate but coordinated concepts:

- `Composition.Vehicles` defines physical consist order;
- travel direction/reversal determines the active travel head without reversing that list;
- each vehicle has a physical length expressed through the shared `SimulationScale` conversion boundary;
- vehicle movement is sampled from trajectory history using its physical distance behind the active travel head;
- trajectory reseeding preserves exact world positions after F7 and coupling/decoupling discontinuities;
- vehicle orientation is intrinsic state and is handled by the transform/coupling geometry layer rather than by mutating composition order;
- local trajectory tangent determines vehicle rotation on curves;
- the train head uses the exact active curve-arc tangent.

This separation is required to prevent vehicle interpenetration when reversing, entering curves or resuming movement after a rigid consist operation.

## Confirmed files

- `Game/Map/MapSize.cs` — map-size representation.
- `Game/Layer.cs` — game-layer representation.
- `Game/Circle.cs` — circular geometry/helper type.
- `Game/RectangleExtensions.cs` — rectangle extension/helper methods.
- `Game/FaceDirection.cs` — facing/direction type.

## Geometry discipline

Do not create ad-hoc geometry structs when an existing type already represents the required concept. Search `Game/` for related helpers first.

Keep coordinate-space conversions explicit. The rendering/input system has a logical presentation space distinct from the physical backbuffer.

`SimulationScale` is the single boundary for physical metre ↔ map-grid conversion. Do not introduce local conversion constants for vehicle lengths or movement distances.

## Coupling geometry

`CouplingGeometry` is responsible for deriving physical coupling endpoints and directions from vehicle transforms/specifications. `VehicleOrientation`, including `Reverse`, must be applied consistently here.

Coupling operations preserve exact vehicle world positions. Runtime coupling links are rebuilt from physical composition order rather than inferred by reversing the vehicle list.

## Map and rendering boundary

Map geometry is a domain/game concern. Camera and rendering are presentation concerns. A map object should not acquire responsibility for platform window management merely because it is drawn on screen.

## AI workflow

For map-related changes:

1. inspect `MapSize` and all consumers;
2. inspect layer/geometry helpers;
3. inspect `Camera` for rendering-space behavior;
4. inspect railway classes that reference map positions;
5. inspect train trajectory/movement and coupling geometry when vehicle placement is involved;
6. inspect gameplay screens that render or manipulate the map;
7. preserve existing coordinate conventions.

## Caution

The current inventory does not by itself prove the exact semantics of every geometry class. The class implementation and call sites remain the source of truth for units, origins, orientation conventions and map topology.
