# Screens and UI inventory

The Core screen area contains reusable screen infrastructure and concrete game/application screens. Migrated standard UI uses Myra through the shared `MyraUIManager`.

## Confirmed concrete screens

- `GameplayScreen` — primary gameplay screen and authoritative pause/persistence owner.
- `DepotScreen` — full-screen Depot train builder; not a popup.
- `MenuScreen` — legacy menu abstraction retained for compatibility.
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

Clicking an existing Depot through `InputManager.DepotSelected` opens `DepotScreen`.

The builder provides:

- locomotive selection: EP07, EU200 — Newag Griffin E4ACP, SU42;
- wagon selection: three passenger coach definitions;
- add wagon to the end of the consist;
- remove individual wagons;
- clear all wagons;
- live Vmax, total mass, total length and wagon count;
- create a locomotive-only or locomotive-plus-wagons train;
- cancel without creating a train.

The created train is placed on an adjacent free track cell through `TrainManager.CreateTrainFromComposition()`.

## Pause lifecycle

`GameplayScreen` owns pause state. Entering pause activates `MyraPauseView`; simulation updates stop while Myra input remains active. Resume clears the gameplay pause state and restores the gameplay Myra root. No pause popup is added to `ScreenManager`.

## Gameplay HUD interaction

The Myra gameplay panel provides build-tool selection, simulation-speed controls and train/station selection. Selecting a train or station requests camera navigation to the selected object's world-space center.

## Remaining non-Myra UI

Junction/signal radial interaction menus and some legacy floating/tooltips remain outside the Myra HUD. Depot-specific train creation is no longer a legacy SpriteBatch panel.

Railway/world rendering itself is not a Myra UI concern.

## Input ownership

- Myra Desktop handles migrated widget interaction.
- `GameplayScreen` owns authoritative pause behavior.
- `DepotScreen` owns temporary builder state only; train ownership remains in `TrainManager`.
- `ScreenManager` owns registered screen lifecycle.
- Each visible action must have one presentation/interaction owner.

## AI rule

Before adding UI, inspect existing Myra views and lifecycle. Do not add a legacy fallback UI for an already migrated surface or create another Myra `Desktop`.
