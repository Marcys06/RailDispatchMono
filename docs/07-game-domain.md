# Game domain

## Railway subsystem

The repository contains `Game/Railway` domain classes including `BlockController`, `Junction`, `TrackRoute`, `Depot` and `DepotController`. Railway infrastructure/control remains owned by the domain layer rather than by screens.

## Train subsystem

The `Game/Train` area contains `TrainManager`, `Train`, `TrainComposition`, `Vehicle`, `VehicleParameters`, `Locomotive` and `Wagon`.

`TrainComposition` is the authoritative ordered list of vehicles in a train. It exposes total physical mass/length, effective maximum speed and wagon count. A composition may contain a locomotive without wagons; a locomotive is required for movement.

`TrainManager` is the authoritative train lifecycle owner. `CreateTrainFromComposition()` is the single creation path used by the Depot builder for player-created consists.

## Rolling stock catalog

`Game/RollingStock` contains reusable rolling-stock definitions:

- `RollingStockCatalog` — registered locomotives and wagons.
- `LocomotiveDefinition` — catalogue data and vehicle factory for a locomotive.
- `WagonDefinition` — catalogue data and vehicle factory for a wagon.
- `TractionType` — electric/diesel classification.

The first `0.1.4e` locomotive set is:

- `EP07` — electric, 125 km/h, 80 t, 16.2 m.
- `EU200 — Newag Griffin E4ACP` — electric AC, 200 km/h, 84 t, 19.9 m.
- `SU42` — diesel, 90 km/h, 74 t, 14.4 m.

These values are gameplay-scaled around the existing EU06-level simulation response; they are not a full traction-curve simulation.

The first wagon catalogue contains three passenger coach variants. Wagon technical Vmax participates in the composition's effective Vmax calculation.

## Physical parameter contract

`VehicleParameters` continues to expose the internal simulation values used by movement: speed is m/s, mass is retained in kg for compatibility, and vehicle visual length remains in map-cell units. `MassTons` and `LengthMeters` expose the real-world/gameplay values used by the rolling-stock catalogue and Depot summary.

`VehicleParameters.CreatePhysical()` converts catalogue Vmax from km/h to internal m/s and keeps the established visual grid proportions separate from the `1 cell = 10 m` physical scale.

## Depot lifecycle

`DepotController` owns depot buildings. Clicking an existing depot opens the full-screen `DepotScreen`, which uses `MyraDepotView` and the existing shared `MyraUIManager`/`Desktop`.

The Depot builder allows:

1. selecting one locomotive;
2. adding any number of passenger wagons;
3. removing individual wagons;
4. clearing all wagons;
5. reviewing Vmax, total mass, total length and wagon count;
6. creating the train on an adjacent free track cell.

The builder may create a locomotive-only consist. It does not currently support multiple locomotives.

## Domain vs presentation

Screens request or present domain state. They do not become authoritative owners of train collections, depot state or railway simulation.

## Ownership rule

Before adding a property to a domain object, determine which existing class already owns the state. Train collection/creation belongs to `TrainManager`; composition order and composition statistics belong to `TrainComposition`; catalogue data belongs to `Game/RollingStock`; depot buildings belong to `DepotController`.

## Safe extension sequence

When extending railway gameplay:

1. Locate the domain object that owns the state.
2. Locate its manager/coordinator, if one exists.
3. Search all call sites before changing a signature.
4. Check the active screen(s) that consume the state.
5. Only then implement the smallest change that preserves the existing flow.
6. Update this documentation if the ownership or lifecycle contract changes.
