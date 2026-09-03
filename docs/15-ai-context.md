# AI context packet

## Current release

**RailDispatchMono `0.1.5pre`** is the current consolidated 0.1.5 milestone. It combines the rolling-stock catalogue/visual work, locomotive power/Vmax model, consist-mass braking, speed-dependent signal safety, Depot builder, static and runtime coupling data, rigid coupling/decoupling commands, runtime short-label persistence compatibility and train-scoped diagnostics. `0.1.4pre` remains the previous consolidated milestone; `0.1.5a`–`0.1.5f` are immutable historical development stages.

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

At the beginning of `Train.Update`, the movement layer establishes a temporary train GUID context. `DebugManager` rewrites messages beginning with `[TRAIN]` to `[TRAIN:<first-8-guid-chars>]`. The context is cleared after movement completes. This is diagnostic correlation only and must not become a second train identity system. Coupling operations use `[COUPLING]` diagnostics.

## Coupling and decoupling contract — 0.1.5

- `Vehicle.Coupling` exposes static front/rear `CouplerType` metadata.
- `VehicleCouplingState` stores runtime connections independently on each vehicle.
- `CouplingConnection` links two concrete vehicle ends.
- `CouplingService` is authoritative for validation and state mutation.
- `TrainComposition` remains the only authoritative ordered vehicle container.
- Coupling is restricted to compatible free outer train boundaries within the configured distance and alignment constraints.
- Successful coupling stops both consists through `RadioStop`, merges them without reordering vehicles and leaves the merged train stopped.
- Successful decoupling stops the train, splits the composition at the connected adjacent boundary, clears both endpoints, creates a stopped detached `Train` and registers it with `TrainManager`.
- `C` couples the nearest valid candidate.
- `X` decouples the last coupling created by `C`, with fallback to the first remaining runtime connection.
- `F6` / `F7` / `F8` select `3` / `4` / `5 km/h` shunting limits; `5 km/h` is the default.
- `SignalAspect.Reserve3` (`S14`, `Rezerwowy 3`) is the semantic shunting profile for the command layer.
- Coupling connections are not persisted.
- There is no dynamic coupler force/slack model, impact shock, animation/delay or brake-pipe propagation.
- There is no final vehicle/end selection UI yet.

## Runtime save/load contract

- Runtime save schema remains version `1`.
- `ShortName` is persisted for rolling stock.
- Loading remains compatible with saves that do not contain `ShortName`.
- `RuntimeSaveService` uses the current `Locomotive` and `Wagon` constructor signatures.
- Coupling connections are runtime-only.

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
- Do not use UI code to mutate `TrainComposition.Vehicles` directly.
- Do not add a second coupling/decoupling command path while the current temporary command contract is active.

## Verification

The repository currently has no dedicated automated Core test project. Verify changes by building the solution and, for runtime behavior, exercising the affected gameplay flow in the DesktopGL environment and inspecting diagnostics where relevant.

## Pause lifecycle

Pause is a gameplay state, not a popup screen. `GameplayScreen` owns the pause state and activates `MyraPauseView`. While paused, simulation progression stops while Myra remains interactive. Save/Load are gameplay-owned operations behind the existing save service boundary.

## Documentation rule

`0.1.2pre`, `0.1.3pre`, `0.1.4pre` and `0.1.5pre` have current-state snapshots. Lettered stages remain historical changelog stages. When code changes, update the affected maintained docs, the current `0.1.5pre` snapshot and the changelog.
