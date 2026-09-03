# AI context packet

## Current release

**RailDispatchMono `0.1.4pre`** is the current consolidated 0.1.4 milestone. It combines the rolling-stock catalogue/visual work, locomotive power/Vmax model, consist-mass braking, speed-dependent signal safety, Depot builder, static coupling metadata, runtime short-label persistence compatibility and train-scoped diagnostics. `0.1.3pre` remains the previous consolidated milestone; `0.1.4a`–`0.1.4i` are immutable historical development stages.

## One-paragraph context

RailDispatchMono is a C#/.NET 9 MonoGame project with shared Core code and platform hosts. `RailDispatchMonoGame` owns the game loop and delegates screen lifecycle/update/draw to `ScreenManager`. `MyraUIManager` is the single Myra integration boundary and owns one shared `Desktop` and active root. Main Menu, Settings, About, Pause, gameplay HUD and the full-screen Depot builder use Myra. `TrainManager` is the authoritative train lifecycle owner; `TrainComposition` owns ordered vehicles and derived consist statistics. `RollingStockCatalog` owns reusable locomotive/wagon definitions. `DepotScreen` presents those definitions and creates a train through `TrainManager.CreateTrainFromComposition()`.

## Myra contract

- Package: Myra `1.6.5`.
- One shared `Desktop` owned by `MyraUIManager`.
- One active Myra root at a time.
- Depot uses a full-screen `GameScreen`, not a popup and not a second `Desktop`.
- `MyraUIManager` restores the gameplay root when Depot closes.
- Myra does not replace gameplay simulation or railway rendering.

## Rolling stock contract

- `Game/RollingStock` contains catalogue definitions and factories.
- `EP07`, `EU200 — Newag Griffin E4ACP` and `SU42` are the current locomotive definitions.
- Locomotives carry `PowerMW` through `LocomotiveParameters`.
- Three passenger coach definitions are available.
- One locomotive is allowed per consist; zero or more wagons may be added.
- Wagon visual labels are `1KL`, `2KL`, `3KL`.
- Electric locomotives render red; diesel locomotives render black; rolling-stock labels render white and centered and remain readable in both travel directions.

## Consist performance contract

- Locomotive acceleration and braking use the non-linear mass model.
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
- `Stop` / `StopStation` calculate target speed from available distance using effective braking.
- Restricted signal aspects use effective braking when deciding whether enough distance remains to reduce speed.
- The current Stop target uses a `0.8`-cell offset plus the physical half-length of the leading vehicle and the existing `0.15 s` reaction allowance.
- `1 map cell = 10 m` remains the authoritative spatial scale.

## RadioStop contract

`TrainCollisionController` keeps a minimum safety distance of `3` cells but expands it with speed. The protected distance includes effective braking distance, `0.15 s` reaction distance and a `0.8`-cell buffer. A protecting matching signal encountered before the conflicting train still suppresses RadioStop.

RadioStop is a collision-protection fallback, not a replacement for signal or block authority.

## Diagnostics contract

At the beginning of `Train.Update`, the movement layer establishes a temporary train GUID context. `DebugManager` rewrites messages beginning with `[TRAIN]` to `[TRAIN:<first-8-guid-chars>]`. The context is cleared after movement completes. This is diagnostic correlation only and must not become a second train identity system.

## Coupling data boundary — 0.1.5 preparation

- `Vehicle.Coupling` exposes static front/rear `CouplerType` metadata.
- `CouplingSpecification` is descriptive data only; default front/rear type is `Screw`.
- No runtime coupling state exists yet.
- No coupling/decoupling commands, detection, compatibility rules, forces or persistence are implemented in `0.1.4pre`.
- Runtime connection state should be owned by the train/consist layer in `0.1.5`, not by rendering or UI.
- `TrainComposition` remains the ordered vehicle container; `0.1.5` should extend it rather than introduce a second vehicle collection.

## Runtime save/load contract

- Runtime save schema remains version `1`.
- `ShortName` is persisted for rolling stock.
- Loading remains compatible with saves that do not contain `ShortName`.
- `RuntimeSaveService` uses the current `Locomotive` and `Wagon` constructor signatures.

## Depot lifecycle

Clicking an existing depot through `InputManager.DepotSelected` opens `DepotScreen`. The builder allows locomotive selection, wagon addition/removal, clearing wagons, live composition statistics and train creation. The created train spawns on an adjacent free track cell when one exists.

Depot buildings are rendered in world space by `DepotRenderer`, using the same camera-space rendering contract as stations. Depot placement preview follows the same contract.

The old hard-coded gameplay train creation path is not the authoritative way to create player consists. Train creation belongs to `TrainManager.CreateTrainFromComposition()`.

## Gameplay safety boundaries

- Do not create a second train-creation system outside `TrainManager`.
- Do not create a second Myra manager or `Desktop`.
- Preserve `BlockController` and `StationController` authority.
- When changing acceleration, braking or Vmax, audit signal stopping and RadioStop calculations for the same dependency.
- Do not reintroduce a fixed-distance collision scan that ignores current speed.
- Do not implement coupling mechanics in `0.1.4pre`; keep coupling metadata static until the `0.1.5` implementation pass.
- Do not use UI code to mutate `TrainComposition.Vehicles` directly.

## Pause lifecycle

Pause is a gameplay state, not a popup screen. `GameplayScreen` owns the pause state and activates `MyraPauseView`. While paused, simulation progression stops while Myra remains interactive. Save/Load are gameplay-owned operations behind the existing save service boundary.

## Documentation rule

`0.1.2pre`, `0.1.3pre` and `0.1.4pre` have current-state snapshots. Lettered `0.1.4a`–`0.1.4i` remain historical changelog stages. When code changes, update the affected maintained docs, the current `0.1.4pre` snapshot and the changelog.
