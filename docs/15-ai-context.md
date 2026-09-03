# AI context packet

## Current release

**RailDispatchMono `0.1.4h`** is the current rolling-stock, locomotive power/Vmax and speed-dependent safety development stage. `0.1.3pre` remains the previous consolidated Myra gameplay UI snapshot; `0.1.4a`–`0.1.4g` are lettered development stages recorded in changelogs.

## One-paragraph context

RailDispatchMono is a C#/.NET 9 MonoGame project with shared Core code and platform hosts. `RailDispatchMonoGame` owns the game loop and delegates screen lifecycle/update/draw to `ScreenManager`. `MyraUIManager` is the single Myra integration boundary and owns one shared `Desktop` and active root. Main Menu, Settings, About, Pause, gameplay HUD and the full-screen Depot builder use Myra. `TrainManager` is the authoritative train lifecycle owner; `TrainComposition` owns ordered vehicles and derived consist statistics. `RollingStockCatalog` owns reusable locomotive/wagon definitions. `DepotScreen` presents those definitions and creates a train through `TrainManager.CreateTrainFromComposition()`.

## Myra contract

- Package: Myra 1.6.5.
- One shared `Desktop` owned by `MyraUIManager`.
- One active Myra root at a time.
- Depot uses a full-screen `GameScreen`, not a popup and not a second `Desktop`.
- `MyraUIManager` restores the gameplay root when Depot closes.
- Myra does not replace gameplay simulation or railway rendering.

## Rolling stock contract

- `Game/RollingStock` contains catalogue definitions and factories.
- `EP07`, `EU200 — Newag Griffin E4ACP` and `SU42` are the first locomotive definitions.
- Locomotives carry `PowerMW` through `LocomotiveParameters`.
- Three passenger coach definitions are available.
- One locomotive is allowed per consist; zero or more wagons may be added.
- Wagon visual labels are `1KL`, `2KL`, `3KL`.
- Electric locomotives render red; diesel locomotives render black; rolling-stock labels render white and remain readable in both travel directions.

## Consist performance contract

- Locomotive acceleration and braking use the `0.1.4f` non-linear mass model.
- Total consist mass reduces both acceleration and braking.
- Mass sensitivity uses exponent `1.30`.
- `factor = 1 / (totalMass / locomotiveMass)^1.30`.
- Locomotive power additionally limits Vmax when the consist becomes too heavy.
- Power/load calibration uses `0.006 MW/t` and exponent `0.55`.
- `supportedMass = PowerMW / 0.006`.
- Above supported mass: `VmaxMultiplier = (supportedMass / totalMass)^0.55`.
- Effective Vmax is the lower of power-limited locomotive Vmax and wagon Vmax.
- `EU200` (5.5 MW, 84 t) with ten 40 t wagons remains at 200 km/h.
- `SU42` (1.2 MW, 74 t) is approximately 75 km/h with five 40 t wagons and approximately 55 km/h with ten.

## Speed-dependent braking contract

Signal stopping and safety calculations must use the same effective braking capability as `TrainMovement`. Do not use a raw wagon/locomotive braking value for heavy-consist stopping distances.

- `Train.EffectiveBrakingRate` exposes the current locomotive braking capability after the mass factor.
- `Stop` / `StopStation` calculate the target speed from available distance using effective braking.
- Restricted signal aspects use effective braking when deciding whether enough distance remains to reduce speed.
- The current Stop target uses a `0.8`-cell offset plus the physical half-length of the leading vehicle and the existing `0.15 s` reaction allowance.
- `1 map cell = 10 m` remains the authoritative spatial scale.

## RadioStop contract

`TrainCollisionController` keeps a minimum safety distance of `3` cells but expands it with speed. The protected distance includes effective braking distance, `0.15 s` reaction distance and a `0.8`-cell buffer. A protecting matching signal encountered before the conflicting train still suppresses RadioStop.

RadioStop is a collision-protection fallback, not a replacement for signal or block authority.

## Depot lifecycle

Clicking an existing depot through the existing `InputManager.DepotSelected` event opens `DepotScreen`. The builder allows locomotive selection, wagon addition/removal, clearing wagons, live composition statistics and train creation. The created train spawns on an adjacent free track cell when one exists.

The previous hard-coded test train in `GameplayScreen` has been removed. The test track remains for the new-game development scenario.

## Gameplay safety boundaries

- Do not create a second train-creation system outside `TrainManager`.
- Do not create a second Myra manager or `Desktop`.
- Preserve `BlockController` and `StationController` authority.
- When changing acceleration, braking or Vmax, audit signal stopping and RadioStop calculations for the same dependency.
- Do not reintroduce a fixed-distance collision scan that ignores current speed.

## Pause lifecycle

Pause is a gameplay state, not a popup screen. `GameplayScreen` owns the pause state and activates `MyraPauseView`. While paused, simulation progression stops while Myra remains interactive. Save/Load are gameplay-owned operations behind `MapSaveService`.

## Documentation rule

Only `0.1.2pre` and `0.1.3pre` have current-state snapshots. `0.1.4a`–`0.1.4h` remain lettered changelog stages and must not receive current-state snapshot files.
