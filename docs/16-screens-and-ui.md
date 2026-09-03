# Screens and UI inventory

The Core screen area contains reusable screen infrastructure and concrete game/application screens. Migrated standard UI uses Myra through the shared `MyraUIManager`.

## Current consolidated baseline

`0.1.4pre` is the current consolidated 0.1.4 screen/UI baseline.

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

## Depot interaction

Clicking an existing Depot through `InputManager.DepotSelected` opens `DepotScreen` directly. The old preset-based SpriteBatch train menu is not the current train-creation architecture.

The Myra builder provides:

- locomotive selection: EP07, EU200 — Newag Griffin E4ACP, SU42;
- wagon selection: three passenger coach definitions;
- add any number of wagons to the end of the consist;
- remove individual wagons;
- clear all wagons;
- live Vmax, total mass, total length and wagon count;
- create a locomotive-only or locomotive-plus-wagons train;
- cancel without creating a train.

The created train is placed on an adjacent free track cell through the single authoritative `TrainManager.CreateTrainFromComposition()` path. `InputManager` does not construct train objects directly.

## Train physics and safety UI implications

Consist acceleration and braking use the non-linear mass factor:

`factor = 1 / (totalMass / locomotiveMass)^1.30`

Locomotive power additionally limits Vmax above the supported consist mass.

Signal `Stop` / `StopStation` braking and restricted-aspect safety use the same effective braking capability as train movement. `TrainCollisionController` retains the 3-cell RadioStop minimum but expands protected distance at higher speed using effective braking distance, `0.15 s` reaction distance and a `0.8`-cell buffer.

The current Stop target retains the `0.8`-cell offset and leading-vehicle physical half-length correction. Spatial scale remains `1 map cell = 10 m`.

## Rolling stock presentation

`TrainRenderer` is responsible for world-space vehicle presentation. It receives the `Arial24` font from `GameplayScreen` and draws a centered white short label on every visible rolling-stock unit. Locomotive labels are `EP07`, `EU200`, `SU42`; passenger labels are `1KL`, `2KL`, `3KL`. Label rotation is normalized so the text remains readable when the train reverses direction.

`DepotRenderer` uses the same world-space/camera transform contract as `StationRenderer`. The Depot is rendered as a visible 1x1-cell building with outline and entrance details, plus a matching placement preview. It must not convert world cells to screen pixels internally.

## Train diagnostics

Train movement diagnostics are rendered through the central `DebugManager` logger. During a train update, messages beginning with `[TRAIN]` receive the active train's first eight GUID characters, for example `[TRAIN:de148bda] START ...`. The identifier is for log correlation and does not alter UI state or simulation.

## Coupling preparation — implementation target 0.1.5

Coupling has no gameplay UI in `0.1.4pre`. The code exposes only static `Vehicle.Coupling` metadata through `CouplingSpecification`; runtime connection state belongs to the future train/consist layer.

Do not add coupling buttons, coupling distance detection or visual connection state before the `0.1.5` implementation pass. The eventual UI should request operations from the train domain rather than edit `TrainComposition.Vehicles` directly.

## Pause lifecycle

`GameplayScreen` owns pause state. Entering pause activates `MyraPauseView`; simulation updates stop while Myra input remains active. Resume clears the gameplay pause state and restores the gameplay Myra root. No pause popup is added to `ScreenManager`.

Opening `DepotScreen` also covers `GameplayScreen`, so gameplay simulation updates do not continue underneath the Depot builder. Closing the screen restores the gameplay Myra root and normal simulation/HUD operation.

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
