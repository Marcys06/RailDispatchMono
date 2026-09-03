# Game domain

## Railway subsystem

The repository contains `Game/Railway` domain classes including `BlockController`, `Junction`, `TrackRoute`, `Depot` and `DepotController`. Railway infrastructure/control remains owned by the domain layer rather than by screens.

## Train subsystem

The `Game/Train` area contains `TrainManager`, `Train`, `TrainComposition`, `Vehicle`, `VehicleParameters`, `LocomotiveParameters`, `Locomotive` and `Wagon`.

`TrainComposition` is the authoritative ordered list of vehicles in a train. It exposes total physical mass/length, effective maximum speed and wagon count. A composition may contain a locomotive without wagons; a locomotive is required for movement.

`TrainManager` is the authoritative train lifecycle owner. `CreateTrainFromComposition()` is the single creation path used by the Depot builder for player-created consists.

### Mass-dependent acceleration/braking

Train acceleration and braking are calculated from the locomotive's own acceleration/braking capability and the total physical consist mass. Added wagon mass therefore reduces both acceleration and deceleration; wagons do not contribute their own propulsion/braking rates.

The mass penalty is deliberately non-linear. For a locomotive mass `M_loco` and total consist mass `M_total`, the performance factor is:

`factor = 1 / (M_total / M_loco)^1.30`

The exponent `1.30` makes mass sensitivity approximately 30% stronger than a simple linear inverse-mass relationship while preserving the locomotive-only baseline (`factor = 1`). The same factor is applied to acceleration and braking. This is a gameplay model, not a full traction/brake-force simulation.

### Power-dependent Vmax

Locomotives have a dedicated `LocomotiveParameters` type with `PowerMW`. Power determines how much total consist mass can be carried at the locomotive's base Vmax.

The current gameplay calibration uses:

- `PowerToMassThresholdMWPerTon = 0.006 MW/t`;
- `PowerLoadExponent = 0.55`;
- supported mass = `PowerMW / 0.006`;
- if total mass is within supported mass, the power multiplier is `1.0`;
- above the supported mass, `multiplier = (supportedMass / totalMass)^0.55`;
- effective Vmax is `base locomotive Vmax * multiplier`, additionally limited by wagon Vmax.

This produces the intended calibration with 40 t passenger wagons: `EU200` (5.5 MW, 84 t) with 10 wagons (484 t total) keeps 200 km/h, while `SU42` (1.2 MW, 74 t) with 5 wagons (274 t total) is approximately 75 km/h and with 10 wagons (474 t total) approximately 55 km/h.

### Speed-dependent signal stopping

Signal stopping uses the same effective consist braking capability as movement. The stopping calculation must not use a raw vehicle braking value because that would ignore the `1.30` mass penalty and underestimate the distance required by heavy trains.

For a `Stop` or `StopStation` aspect, the target speed is derived from the available distance using the kinematic relation `v = sqrt(2*a*d)`. The available distance includes the current reaction-distance allowance and excludes the configured stopping offset plus the physical half-length of the leading locomotive. The current stopping offset is `0.8` map cell, with `1 map cell = 10 m`.

Speed-restricted non-stop signal aspects use the same effective braking rate when determining whether there is enough distance to reduce from the current speed to the aspect's target speed.

### RadioStop safety

`TrainCollisionController` retains a minimum RadioStop safety distance of `3` map cells, but the protected distance is now speed-dependent. At higher speed it expands to cover:

- current braking distance using the effective consist braking rate;
- `0.15 s` reaction distance;
- a `0.8`-cell safety buffer.

Therefore RadioStop no longer relies on a fixed 3-cell scan at all speeds. A protecting matching signal encountered before the conflicting train still suppresses RadioStop for that route segment.

RadioStop remains a collision-protection fallback, not a replacement for signal or block authority.

## Rolling stock catalog

`Game/RollingStock` contains reusable rolling-stock definitions:

- `RollingStockCatalog` — registered locomotives and wagons.
- `LocomotiveDefinition` — catalogue data, power, short label and vehicle factory for a locomotive.
- `WagonDefinition` — catalogue data, short label and vehicle factory for a wagon.
- `TractionType` — electric/diesel classification.

The locomotive catalogue currently contains:

- `EP07` — electric, 125 km/h, 80 t, 2.0 MW, label `EP07`.
- `EU200 — Newag Griffin E4ACP` — electric AC, 200 km/h, 84 t, 5.5 MW, label `EU200`.
- `SU42` — diesel, 90 km/h, 74 t, 1.2 MW, label `SU42`.

The wagon catalogue contains three passenger coach variants. Their visual labels are `1KL`, `2KL` and `3KL`; each currently permits 200 km/h so a compliant EU200 consist is not artificially capped by the coach catalogue Vmax.

## Physical parameter contract

`VehicleParameters` continues to expose the internal simulation values used by movement: speed is m/s, mass is retained in kg for compatibility, and vehicle visual length remains in map-cell units. `MassTons` and `LengthMeters` expose the real-world/gameplay values used by the rolling-stock catalogue and Depot summary.

`LocomotiveParameters` extends `VehicleParameters` with `PowerMW` and is created by `LocomotiveDefinition`.

`VehicleParameters.CreatePhysical()` converts catalogue Vmax from km/h to internal m/s and keeps the established visual grid proportions separate from the `1 cell = 10 m` physical scale.

## Rolling stock presentation

`TrainRenderer` draws rolling stock as differentiated top-down vehicles:

- electric locomotives are red;
- diesel locomotives are black;
- all rolling-stock labels are white and centered;
- locomotive labels use their short class names (`EP07`, `EU200`, `SU42`);
- passenger coaches use `1KL`, `2KL`, `3KL` with distinct blue shades;
- labels are normalized to remain readable in both travel directions.

## Coupling data boundary — prepared for 0.1.5

Coupling is intentionally prepared as a data boundary only. `Vehicle` exposes a `CouplingSpecification` containing the static interface type at the front and rear of the vehicle. The current default is a screw coupler on both ends.

The following are explicitly **not** implemented in 0.1.4:

- coupled/uncoupled runtime state;
- coupling distance detection;
- coupling/decoupling commands;
- consist merge/split as a coupling action;
- coupler compatibility checks;
- coupling forces, slack or longitudinal dynamics;
- persistence of individual coupler connections.

The planned 0.1.5 boundary is: static rolling-stock coupling data belongs to `Vehicle`; runtime connection state and consist mutations belong to `Train`/`TrainComposition`/`TrainManager`; UI/input should request those operations rather than mutate vehicle lists directly.

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
