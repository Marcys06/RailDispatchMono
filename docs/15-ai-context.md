# AI context packet

## Current release

**RailDispatchMono `0.1.4g`** is the current rolling-stock presentation and locomotive power/Vmax development stage. `0.1.3pre` remains the previous consolidated Myra gameplay UI snapshot; `0.1.4a`–`0.1.4f` are lettered development stages recorded in changelogs.

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
- Locomotives now carry `PowerMW` through `LocomotiveParameters`.
- Three passenger coach definitions are available.
- One locomotive is allowed per consist; zero or more wagons may be added.
- A locomotive-only train is valid.
- Wagon visual labels are `1KL`, `2KL`, `3KL`.
- Electric locomotives render red; diesel locomotives render black; rolling-stock labels render white and remain readable in both travel directions.

## Consist performance contract

- Locomotive acceleration and braking remain the `0.1.4f` mass model.
- Total consist mass reduces both acceleration and braking.
- Mass sensitivity is non-linear and uses exponent `1.30`.
- `factor = 1 / (totalMass / locomotiveMass)^1.30`.
- Locomotive power now additionally limits Vmax when the consist becomes too heavy.
- Current power/load calibration uses `0.006 MW/t` and exponent `0.55`.
- `supportedMass = PowerMW / 0.006`.
- Above supported mass: `VmaxMultiplier = (supportedMass / totalMass)^0.55`.
- Effective Vmax is the lower of power-limited locomotive Vmax and wagon Vmax.
- `EU200` (5.5 MW, 84 t) with ten 40 t wagons remains at 200 km/h.
- `SU42` (1.2 MW, 74 t) is approximately 75 km/h with five 40 t wagons and approximately 55 km/h with ten.

## Depot lifecycle

Clicking an existing depot through the existing `InputManager.DepotSelected` event opens `DepotScreen`. The builder allows locomotive selection, wagon addition/removal, clearing wagons, live composition statistics and train creation. The created train spawns on an adjacent free track cell when one exists.

The previous hard-coded test train in `GameplayScreen` has been removed. The test track remains for the new-game development scenario.

## Gameplay safety boundaries

- Do not change `RadioStop` as part of rolling-stock work.
- Do not change semaphore logic as part of rolling-stock work.
- Do not create a second train-creation system outside `TrainManager`.
- Do not create a second Myra manager or `Desktop`.
- Preserve `BlockController` and `StationController` authority.

## Pause lifecycle

Pause is a gameplay state, not a popup screen. `GameplayScreen` owns the pause state and activates `MyraPauseView`. While paused, simulation progression stops while Myra remains interactive. Save/Load are gameplay-owned operations behind `MapSaveService`.

## Documentation rule

Only `0.1.2pre` and `0.1.3pre` have current-state snapshots. `0.1.4a`–`0.1.4g` remain lettered changelog stages and must not receive current-state snapshot files.
