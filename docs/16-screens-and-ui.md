# Screens and UI inventory

## Current development line

`0.2.5` is the current documented UI baseline. Myra remains the single migrated UI integration boundary through `MyraUIManager`.

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
- `MyraTrackInfrastructureView` — F8 single-track infrastructure editor
- `MyraRailwayDiagnosticsView` — full infrastructure/dispatcher diagnostics
- `MyraRailwayLineView` — F11 named railway line and bulk infrastructure editor
- `MyraUIManager`

There is one shared Myra `Desktop` and one active root. Gameplay temporarily replaces the gameplay root with the timetable, infrastructure, diagnostics or railway-line editor and restores it on close.

## 0.2.5 gameplay controls

- `F6` — dispatcher-only force passage;
- `F8` — edit infrastructure metadata of the track under the mouse cursor;
- `F9` — locomotive timetable editor;
- `F10` — railway diagnostics;
- `F11` — named railway line editor.

F8 changes only infrastructure metadata: track role, line class and traction (`None`, `DC`, `AC`). It does not modify track geometry, connections, block occupancy, signals or switches.

## GUI/world input separation

A temporary Myra GUI is modal with respect to the gameplay world. When `MyraUIManager.IsGameplayOverlayOpen` is true, `GameplayScreen` does not call `InputManager.Update()`.

Therefore the mouse can interact with the GUI without interacting with the map underneath it. In particular, a previously selected build mode such as `1` cannot cause a track to be placed by clicking the GUI or its surrounding area.

Opening F8/F9/F10/F11 also resets `TrackBuilder.Mode` to `None`. Closing the GUI therefore cannot leave a pending track-placement action armed.

This is intentionally a world-input lock, not a replacement Myra input system: Myra owns GUI pointer interaction while `InputManager` owns world interaction only when the gameplay GUI is not modal.

## Multi-track selection

In `TrackBuildMode.None`, existing track cells can be selected directly on the map:

- LPM — replace selection;
- Shift+LPM — add to selection;
- Ctrl+LPM — toggle selection.

`TrackRenderer` draws a white contour around the active selection. The selection is owned by `InputManager` and is passed to the F11 editor; it is not a second domain model.

## Railway line editor — F11

`MyraRailwayLineView` supports:

- naming and creating a line from the current selection;
- listing and selecting existing named lines;
- selecting a line's tracks back on the map;
- adding/removing the current selection from a line;
- mass-applying `TrackType`, `LineClass` and `TractionSystem` to the selection or selected line;
- selecting a connected track area using the existing topology;
- deleting a named line without deleting physical track.

Named lines are organizational infrastructure metadata only. They do not become a routing, block, signal or dispatcher subsystem.

## Map presentation of named lines

Each named line receives a deterministic display color. `TrackRenderer` draws this color as a contour behind the normal track. The normal traction color remains visible in the track center and line-class thickness remains unchanged. This lets the player read both infrastructure properties and the named-line grouping at the same time.

## Locomotive timetable editor — F9

After selecting a train with a locomotive, F9 opens `MyraLocomotiveScheduleView`.

## Track infrastructure editor — F8

F8 samples the world position under the mouse cursor and opens `MyraTrackInfrastructureView` for that `TrackCell`.

## Railway diagnostics — F10

F10 opens `MyraRailwayDiagnosticsView` and shows blocks, FCFS requests, signal aspects, timetable runtime and dispatcher state.

## World interaction boundary

`TrainRenderer`, `StationRenderer`, `SignalRenderer` and track rendering remain outside Myra. Myra presents operational state and invokes domain actions; it does not become a second simulation controller.

## Input ownership

- Myra Desktop handles migrated widget interaction;
- `GameplayScreen` owns pause state;
- `DepotScreen` owns temporary builder state; train ownership remains in `TrainManager`;
- `InputManager` owns world input and multi-track selection only while no modal Myra gameplay GUI is open;
- `MyraUIManager` exposes the modal gameplay-overlay state;
- F6 is the explicit dispatcher override;
- F8/F9/F10/F11 open operational views;
- `RailwayLineManager` owns named-line domain state;
- domain managers/models own simulation and infrastructure state.

The HUD does not automate switches, signal aspects, coupling/decoupling, passenger transfers or route repair.

## AI rule

Before changing Myra gameplay UI, inspect `MyraGameplayView`, `MyraUIManager`, `RailDispatchMonoGame`, `GameplayScreen`, `InputManager` and the relevant domain owner together. Every gameplay GUI must preserve the modal world-input lock: GUI pointer actions must never fall through to track placement, selection, deletion, switch/signal actions or camera movement.
