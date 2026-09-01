# Screens and UI inventory

The Core screen area contains reusable screen infrastructure and concrete game/application screens. Migrated standard UI uses Myra through the shared `MyraUIManager`.

## Confirmed concrete screens

- `GameplayScreen` — primary gameplay screen and authoritative pause/persistence owner.
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
- `MyraUIManager` — shared Myra `Desktop` and active root owner.

Train/station information has one Myra presentation. The former duplicate legacy HUD is not part of the current UI architecture.

## Pause lifecycle

`GameplayScreen` owns pause state. Entering pause activates `MyraPauseView`; simulation updates stop while Myra input remains active. Resume clears the gameplay pause state and Myra root. No pause popup is added to `ScreenManager`.

## Gameplay HUD interaction

The Myra gameplay panel provides build-tool selection, simulation-speed controls and train/station selection. Selecting a train or station requests camera navigation to the selected object's world-space center.

## Remaining non-Myra UI

The following are still migration candidates where present in the current implementation: junction/signal radial interaction menus, legacy floating/tooltips, dedicated train/station detail windows, wagon-route detail/editor UI beyond the existing workflow, and depot-specific interaction UI.

Railway/world rendering itself is not a Myra UI concern.

## Input ownership

- Myra Desktop handles migrated widget interaction.
- `GameplayScreen` owns authoritative pause behavior.
- `ScreenManager` owns registered screen lifecycle.
- Each visible action must have one presentation/interaction owner.

## AI rule

Before adding UI, inspect existing Myra views and lifecycle. Do not add a legacy fallback UI for an already migrated surface or create another Myra `Desktop`.
