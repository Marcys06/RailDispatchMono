# AI context packet

## Current release

**RailDispatchMono `0.1.4f`** is the current rolling-stock, Depot train-builder and consist-mass-performance development stage. `0.1.3pre` remains the previous consolidated Myra gameplay UI snapshot; `0.1.4a`–`0.1.4e` are lettered development stages recorded in changelogs.

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
- Three passenger coach definitions are available.
- One locomotive is allowed per `0.1.4f` consist; zero or more wagons may be added.
- A locomotive-only train is valid.
- Composition Vmax is the minimum Vmax of all vehicles.
- Physical mass/length are displayed in tonnes/metres; internal speed remains m/s.
- Visual vehicle proportions remain on the established map-cell geometry rather than being shrunk by the 10 m physical scale.

## Consist performance contract — 0.1.4f

- Locomotive acceleration and braking are the base capabilities for the consist.
- Total consist mass reduces both acceleration and braking.
- Mass sensitivity is non-linear and uses exponent `1.30`.
- `factor = 1 / (totalMass / locomotiveMass)^1.30`.
- The `1.30` exponent is approximately 30% stronger than a linear inverse-mass relationship.
- A locomotive-only consist has factor `1.0` and therefore keeps the locomotive's base rates.
- This is a gameplay approximation, not a full traction-curve or brake-force simulation.

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

Only `0.1.2pre` and `0.1.3pre` have current-state snapshots. `0.1.4a`–`0.1.4f` remain lettered changelog stages and must not receive current-state snapshot files.
