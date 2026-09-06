# Screens and UI inventory

## Current development line

`0.2.1` is the current documented UI baseline. Myra remains the single migrated UI integration boundary through `MyraUIManager`.

## Concrete screens

- `GameplayScreen` — primary gameplay screen and authoritative pause/persistence owner;
- `DepotScreen` — full-screen Depot train builder;
- `MenuScreen`, `MenuEntry`, `MessageBoxScreen` — legacy infrastructure retained where required;
- `SettingsScreen`, `AboutScreen`, `LoadingScreen`, `BackgroundScreen` — application/game screens.

Pause is gameplay state owned by `GameplayScreen`; presentation is `MyraPauseView`, not a separate popup screen.

## Myra surfaces

- `MyraMainMenuView`
- `MyraSettingsView`
- `MyraAboutView`
- `MyraPauseView`
- `MyraGameplayView`
- `MyraDepotView`
- `MyraUIManager`

There is one shared Myra `Desktop` and one active root. Depot temporarily replaces the gameplay root and restores it on close.

## 0.2.1 gameplay HUD

`MyraGameplayView` is now a complete operational dashboard rather than a collection of unrelated floating lists. The layout has four functional zones:

1. **Top status bar** — game clock, day, simulation speed, global operating mode and speed controls.
2. **Operations** — infrastructure build tools, wagon route entry point and simulation controls.
3. **Operational centre** — selected-train summary, locomotive timetable state, dispatcher/block status and expandable diagnostics.
4. **Traffic** — train list, station passenger summary and wagon summary.

The bottom area also exposes the keyboard operating contract so the HUD and world controls describe the same workflow.

Train rows explicitly distinguish `AUTO` from `RĘCZ`. Selecting a train focuses the camera and makes its timetable, effective signal speed, consist Vmax, dispatcher state and RadioStop state available in the operational centre.

Station rows are expandable passenger summaries. Wagon rows show occupancy and timetable delay and retain route tooltips. The UI does not own or mutate passenger, train, block or timetable state.

The HUD uses fixed side rails and a flexible centre column rather than the former collection of independent 280/350-pixel panels. Lists are scrollable and long labels wrap inside their widgets to prevent the previous overlap between sections.

## World interaction boundary

`TrainRenderer`, `StationRenderer`, `SignalRenderer` and track rendering remain outside Myra. Myra presents operational state and invokes domain actions; it does not become a second world renderer or simulation controller.

## Input ownership

- Myra Desktop handles migrated widget interaction;
- `GameplayScreen` owns pause state;
- `DepotScreen` owns temporary builder state; train ownership remains in `TrainManager`;
- `InputManager` owns world input and cursor selection;
- domain managers own simulation state.

The HUD deliberately does not automate switches, signal aspects, coupling/decoupling or unresolved dispatching situations.

## Passenger ownership boundary

`StationController`, `PassengerManager`, `DefaultPassengerService` and `Wagon` remain authoritative. A passenger belongs to a concrete wagon, not to a UI model or directly to a train. Coupling/decoupling therefore must not trigger UI-owned passenger migration.

## AI rule

Before changing Myra gameplay UI, inspect `MyraGameplayView`, `MyraUIManager`, `GameplayScreen`, `TrainManager`, `StationController`, `RailwayDispatcher`, `LocomotiveSchedule` and `WagonSchedule` together. Keep one Myra `Desktop`, keep domain state outside UI models and update this document plus the current-state/changelog documentation whenever the UI contract changes.
