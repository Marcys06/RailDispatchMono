# Screens and UI inventory

## Current development line

`0.2.2` is the current documented UI baseline. Myra remains the single migrated UI integration boundary through `MyraUIManager`.

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

## 0.2.2 gameplay HUD and operational views

`MyraGameplayView` remains the operational dashboard. It exposes automatic/manual train state, selected-train timetable information, dispatcher/block state, station passenger summaries and wagon occupancy/delay.

The selected-train timetable information now includes current point, next point, ETA, expected/planned departure, current delay and propagated delay. The runtime source remains `LocomotiveScheduleRuntime`; the HUD does not own timetable state.

### Locomotive timetable editor — F9

After selecting a train with a locomotive, **F9** opens `MyraLocomotiveScheduleView`.

The editor supports:

- adding/removing timetable points;
- changing the station assigned to a point;
- changing arrival and departure independently;
- reordering points;
- enabling/disabling the locomotive timetable;
- validation and transactional save/cancel.

Wagon timetables are not edited by this view.

### Railway diagnostics — F10

**F10** opens `MyraRailwayDiagnosticsView` and shows:

- every block as `FREE`, `RESERVED` or `OCCUPIED`;
- block cooldown and train ownership;
- dispatcher FCFS queue;
- signal aspects and their associated block state;
- timetable runtime and dispatcher state for every train.

## World interaction boundary

`TrainRenderer`, `StationRenderer`, `SignalRenderer` and track rendering remain outside Myra. Myra presents operational state and invokes domain actions; it does not become a second world renderer or simulation controller.

## Input ownership

- Myra Desktop handles migrated widget interaction;
- `GameplayScreen` owns pause state;
- `DepotScreen` owns temporary builder state; train ownership remains in `TrainManager`;
- `InputManager` owns world input and cursor selection;
- F9/F10 open presentation-only operational views;
- domain managers own simulation state.

The HUD deliberately does not automate switches, signal aspects, coupling/decoupling or unresolved dispatching situations.

## Passenger ownership boundary

`StationController`, `PassengerManager`, `DefaultPassengerService` and `Wagon` remain authoritative. A passenger belongs to a concrete wagon, not to a UI model or directly to a train. Coupling/decoupling therefore must not trigger UI-owned passenger migration.

## AI rule

Before changing Myra gameplay UI, inspect `MyraGameplayView`, `MyraUIManager`, `GameplayScreen`, `TrainManager`, `StationController`, `RailwayDispatcher`, `LocomotiveSchedule` and `WagonSchedule` together. Keep one Myra `Desktop`, keep domain state outside UI models and update this document plus the current-state/changelog documentation whenever the UI contract changes.
