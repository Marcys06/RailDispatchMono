# Screens and UI inventory

## Current development line

`0.2.4` is the current documented UI baseline. Myra remains the single migrated UI integration boundary through `MyraUIManager`.

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
- `MyraLocomotiveScheduleView` — locomotive timetable editor
- `MyraTrackInfrastructureView` — F8 track infrastructure editor
- `MyraRailwayDiagnosticsView` — full infrastructure/dispatcher diagnostics
- `MyraUIManager`

There is one shared Myra `Desktop` and one active root. Gameplay temporarily replaces the gameplay root with the locomotive timetable editor, infrastructure editor or diagnostics view and restores it on close.

## 0.2.4 gameplay controls

- `F6` — dispatcher-only force passage;
- `F8` — edit infrastructure metadata of the track under the mouse cursor;
- `F9` — locomotive timetable editor;
- `F10` — railway diagnostics.

F8 changes only infrastructure metadata: track role, line class and traction (`BRAK`, `DC`, `AC`). It does not modify track geometry, connections, block occupancy, signals or switches.

New track built through `TrackBuilder` uses the builder's selected infrastructure defaults. The domain also exposes metadata-only configuration methods for existing track cells.

## 0.2.3 operational HUD retained

`MyraGameplayView` remains the operational dashboard. The train list exposes `AUTO` / `RĘCZ`, timetable point `n/m`, delay and speed. The selected-train panel exposes current point, next point, ETA, required departure, delay and dispatcher state.

`WYMUSZENIE [F6]`, `ZWOLNIJ TRASĘ` and `AUTO / RĘCZ` remain explicit player actions. F6 does not clear physical block occupancy or change signal aspects.

## Locomotive timetable editor — F9

After selecting a train with a locomotive, **F9** opens `MyraLocomotiveScheduleView`.

The editor supports adding/removing timetable points, station assignment, independent arrival/departure adjustment, reordering, enable/disable and validation before save. Wagon timetables are not edited by this view.

## Track infrastructure editor — F8

F8 samples the world position under the mouse cursor and opens `MyraTrackInfrastructureView` for that `TrackCell`.

The editor displays geometry and condition and allows cycling:

- `TrackType`: `Mainline`, `Secondary`, `Siding`, `Platform`;
- `LineClass`: `Local`, `Regional`, `Mainline`, `Magistral`;
- `TractionSystem`: `None`, `DC`, `AC`.

The view is deliberately metadata-only. It does not become a second routing, block or signal controller.

## Railway diagnostics — F10

**F10** opens `MyraRailwayDiagnosticsView` and shows:

- every block as `FREE`, `RESERVED` or `OCCUPIED`;
- block cooldown and train ownership;
- FCFS queue with target block and wait time;
- signal aspects and their associated block state;
- timetable runtime and dispatcher state for every train;
- safe dispatcher actions for force-proceed and route release.

The diagnostic release action never writes `Block.IsOccupied` directly.

## World interaction boundary

`TrainRenderer`, `StationRenderer`, `SignalRenderer` and track rendering remain outside Myra. Myra presents operational state and invokes domain actions; it does not become a second world renderer or simulation controller.

## Input ownership

- Myra Desktop handles migrated widget interaction;
- `GameplayScreen` owns pause state;
- `DepotScreen` owns temporary builder state; train ownership remains in `TrainManager`;
- `InputManager` owns world input and cursor selection;
- F6 is the explicit dispatcher override;
- F8 opens infrastructure editing for the cursor track;
- F9/F10 open operational views;
- domain managers/models own simulation and infrastructure state.

The HUD does not automate switches, signal aspects, coupling/decoupling, passenger transfers or route repair.

## Passenger ownership boundary

`StationController`, `PassengerManager`, `DefaultPassengerService` and `Wagon` remain authoritative. A passenger belongs to a concrete wagon, not to a UI model or directly to a train. Coupling/decoupling therefore must not trigger UI-owned passenger migration.

## AI rule

Before changing Myra gameplay UI, inspect `MyraGameplayView`, `MyraUIManager`, `RailDispatchMonoGame`, `GameplayScreen`, `TrackBuilder`, `TrackCell` and the relevant domain owner together. Keep one Myra `Desktop`, keep domain state outside UI models and update this document plus the current-state/changelog documentation whenever the UI contract changes.
