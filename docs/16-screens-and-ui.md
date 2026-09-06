# Screens and UI inventory

## Current development line

`0.2.3` is the current documented UI baseline. Myra remains the single migrated UI integration boundary through `MyraUIManager`.

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
- `MyraRailwayDiagnosticsView` — full infrastructure/dispatcher diagnostics
- `MyraUIManager`

There is one shared Myra `Desktop` and one active root. Gameplay temporarily replaces the gameplay root with the locomotive timetable editor or diagnostics view and restores it on close.

## 0.2.3 gameplay HUD and operational views

`MyraGameplayView` is the operational dashboard. The train list exposes:

- `AUTO` / `RĘCZ` mode;
- current timetable point `n/m`;
- delay and speed;
- selected-train marker.

The selected-train panel exposes current point, next point, ETA, required departure, delay and dispatcher state. It also contains explicit actions:

- `WYMUSZENIE [F6]` — dispatcher-only override;
- `ZWOLNIJ TRASĘ` — dispatcher release boundary;
- `AUTO / RĘCZ` — toggle locomotive timetable execution without deleting the timetable.

Operational notification text reports selected-train waiting, route grant and delay states. Notifications are derived from domain state and do not own it.

### Locomotive timetable editor — F9

After selecting a train with a locomotive, **F9** opens `MyraLocomotiveScheduleView`.

The editor supports adding/removing timetable points, station assignment, independent arrival/departure adjustment, reordering, enable/disable and validation before save. Wagon timetables are not edited by this view.

### Railway diagnostics — F10

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
- F9/F10 open operational views;
- domain managers own simulation state.

F6 does not clear physical block occupancy or change signal aspects. The HUD does not automate switches, signal aspects, coupling/decoupling or passenger transfers.

## Passenger ownership boundary

`StationController`, `PassengerManager`, `DefaultPassengerService` and `Wagon` remain authoritative. A passenger belongs to a concrete wagon, not to a UI model or directly to a train. Coupling/decoupling therefore must not trigger UI-owned passenger migration.

## AI rule

Before changing Myra gameplay UI, inspect `MyraGameplayView`, `MyraUIManager`, `GameplayScreen`, `TrainManager`, `StationController`, `RailwayDispatcher`, `LocomotiveSchedule` and `WagonSchedule` together. Keep one Myra `Desktop`, keep domain state outside UI models and update this document plus the current-state/changelog documentation whenever the UI contract changes.
