# Screens and UI inventory

The Core screen area contains reusable screen infrastructure and concrete game/application screens. Migrated standard UI uses Myra through the shared `MyraUIManager`.

## Confirmed concrete screens

- `GameplayScreen` — primary gameplay screen and authoritative pause/persistence owner.
- `DepotScreen` — full-screen Depot train builder; not a popup.
- `MenuScreen` — legacy menu abstraction retained for compatibility with older application screens.
- `MenuEntry` — legacy menu support retained where older screens require it.
- `SettingsScreen` — settings logic owner with Myra presentation.
- `AboutScreen` — about logic owner with Myra presentation.
- `LoadingScreen` — loading-stage screen.
- `BackgroundScreen` — background presentation.
- `MessageBoxScreen` — legacy dialog infrastructure retained where required.

`PauseScreen` is not the current pause architecture. Pause presentation is `MyraPauseView` owned by `GameplayScreen` state.

## Myra surfaces

- `MyraMainMenuView` — startup menu.
- `MyraSettingsView` — settings presentation.
- `MyraAboutView` — about presentation.
- `MyraPauseView` — pause actions: Resume, Save, Load, Quit.
- `MyraGameplayView` — gameplay HUD: clock, GameDay, speed controls, collapsible tools and train/station lists.
- `MyraDepotView` — full-screen train builder: locomotive selection, wagon selection, ordered consist, live statistics and creation actions.
- `MyraUIManager` — shared Myra `Desktop` and active root owner.

Depot does not create another Myra manager or Desktop. When `DepotScreen` becomes active it temporarily replaces the gameplay Myra root; `MyraUIManager.Clear()` restores the previous gameplay root when the screen closes.

## Depot interaction — 0.1.4f+

Clicking an existing Depot through `InputManager.DepotSelected` opens `DepotScreen` directly. The old `DepotTrainMenu` SpriteBatch preset menu has been removed.

There are no longer three hardcoded train presets such as short/standard/long. Depot train creation is entirely owned by `DepotScreen` + `MyraDepotView`.

The Myra builder provides:

- locomotive selection: EP07, EU200 — Newag Griffin E4ACP, SU42;
- wagon selection: three passenger coach definitions;
- add any number of wagons to the end of the consist;
- remove individual wagons;
- clear all wagons;
- live Vmax, total mass, total length and wagon count;
- create a locomotive-only or locomotive-plus-wagons train;
- cancel without creating a train.

The created train is placed on an adjacent free track cell through the single authoritative `TrainManager.CreateTrainFromComposition()` path. `InputManager` no longer contains the former preset-based train spawn path or its hardcoded EU06-style `VehicleParameters`.

## Train physics note — 0.1.4h

Consist acceleration and braking use the non-linear mass factor from `0.1.4f`:

`factor = 1 / (totalMass / locomotiveMass)^1.30`

Locomotive power additionally limits Vmax above the supported consist mass.

Signal `Stop` / `StopStation` braking now uses the same effective braking capability as train movement, so the stopping-distance calculation reflects the actual loaded consist rather than the raw locomotive/wagon braking parameter.

Restricted signal aspects also use effective braking when deciding whether enough distance remains to reduce speed.

`TrainCollisionController` keeps the 3-cell RadioStop minimum but expands its protected distance at higher speed using effective braking distance, `0.15 s` reaction distance and a `0.8`-cell buffer.

The current Stop target retains the `0.8`-cell offset and leading-vehicle physical half-length correction. Spatial scale remains `1 map cell = 10 m`.

## Rolling stock presentation

The Myra Depot builder exposes the catalogue; world rendering is handled separately by `TrainRenderer`:

- electric locomotives are red;
- diesel locomotives are black;
- all rolling-stock labels are white and centered;
- locomotive labels use `EP07`, `EU200`, `SU42`;
- passenger coaches use `1KL`, `2KL`, `3KL` with distinct blue shades;
- labels remain readable in both travel directions.

## Pause lifecycle

`GameplayScreen` owns pause state. Entering pause activates `MyraPauseView`; simulation updates stop while Myra input remains active. Resume clears the gameplay pause state and restores the gameplay Myra root. No pause popup is added to `ScreenManager`.

Opening `DepotScreen` also covers `GameplayScreen`, so the ScreenManager lifecycle prevents gameplay simulation updates while the Depot builder is active. Closing the screen restores the gameplay Myra root and normal simulation/HUD operation.

## Gameplay HUD interaction

The Myra gameplay panel provides build-tool selection, simulation-speed controls and train/station selection. Selecting a train or station requests camera navigation to the selected object's world-space center.

## Remaining non-Myra UI

Junction/signal radial interaction menus and some legacy floating/tooltips remain outside the Myra HUD. These are separate world-interaction surfaces and are not used for Depot train creation.

Railway/world rendering itself is not a Myra UI concern.

## Input ownership

- Myra Desktop handles migrated widget interaction.
- `GameplayScreen` owns authoritative pause behavior.
- `DepotScreen` owns temporary builder state only; train ownership remains in `TrainManager`.
- `InputManager` handles world input and raises `DepotSelected`; it does not create Depot trains.
- `ScreenManager` owns registered screen lifecycle.
- Each visible action must have one presentation/interaction owner.

## AI rule

Before adding UI, inspect existing Myra views and lifecycle. Do not add a legacy fallback UI for an already migrated surface or create another Myra `Desktop`.
