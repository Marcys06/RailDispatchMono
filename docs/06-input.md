# Input system

`InputState` remains the shared input snapshot/action layer used by the screen system. `InputManager` handles gameplay/world input interpretation. Coupling commands use the existing `TrainManager` path and must not be duplicated by another UI/input owner.

## Current build controls

- `1` / NumPad `1` — straight track
- `2` / NumPad `2` — curve
- `3` / NumPad `3` — junction
- `4` / NumPad `4` — signal
- `5` / NumPad `5` — station
- `9` / NumPad `9` — depot building
- `S` — toggle wagon route edit mode
- `H` / `V` — straight-track orientation where supported
- `R` — rotate current track/junction; in station mode cycle station size
- `J` — toggle signal or junction switch
- `LMB` — build/select; in `S` mode, clicking a wagon opens its route editor
- `PPM` — remove/open object menu; in the wagon route editor, closes the editor
- `Shift + PPM` — explicit removal for objects that support it
- `MMB` — move camera
- mouse wheel — zoom camera
- `Escape` / `P` — pause/resume

## Operational shortcuts

- `F6` — **WYMUSZENIE PRZEJAZDU** for the selected train; bypasses dispatcher arbitration only and does not clear physical block occupancy or change signal/switch state;
- `F7` — change travel direction under the existing movement rules;
- `F8` — open infrastructure editor for the track under the mouse cursor;
- `F9` — open the selected locomotive's timetable editor;
- `F10` — open railway/dispatcher diagnostics.

F6 is no longer the old manual shunting path. RadioStop remains authoritative outside this explicit dispatcher override contract, and normal movement/collision/signal safety remains unchanged.

## Coupling and decoupling controls

The rigid coupling command path remains:

- `C` — couple the nearest valid outer-boundary candidate using the existing shunting validation;
- `X` — decouple the wagon currently under the cursor using the authoritative coupling service.

Do not add a second coupling/decoupling command implementation to `InputManager`, Myra or a screen.

## Infrastructure editor — F8

F8 is handled by `RailDispatchMonoGame` and opens `MyraTrackInfrastructureView` for the `TrackCell` beneath the mouse cursor.

The editor can change:

- `TrackType` — `Mainline`, `Secondary`, `Siding`, `Platform`;
- `LineClass` — `Local`, `Regional`, `Mainline`, `Magistral`;
- `TractionSystem` — `None`, `DC`, `AC`.

This is metadata editing. It does not change track geometry, block occupancy, signal aspects or junction state.

## Pause input ownership

At the current baseline, pause is owned by `GameplayScreen`.

- `ESC` is handled by the gameplay screen as the authoritative pause/resume toggle.
- The pause UI is rendered by `MyraPauseView` through the shared `MyraUIManager`.
- Myra pointer input is handled by the shared `Desktop`.
- `InputManager` does not own a second pause menu and does not compete with Myra for pause button clicks.

## Wagon route edit mode

Pressing `S` toggles wagon route edit mode and clears the active build mode. While the mode is active, the HUD/menu shows a small active `S` indicator.

With the mode active, a new LPM click on a wagon opens its screen-space route editor. The editor handles station buttons independently, supports adding/removing/clearing stations and persists route changes through the existing schedule storage. PPM closes the editor.

## Station building

Station mode supports `1x1`, `2x2`, `3x3` and `4x4` areas. The complete selected rectangle must contain track and cannot overlap another station.

## Depot building

Depot mode is activated with `9`. A depot is a world building and does not require a track cell. The building is rendered using programmatic geometry. Clicking an existing depot opens the Depot workflow; removal remains available through the existing right-click interaction.

`DepotScreen` and `MyraDepotView` own builder interaction. Train creation goes through `TrainManager.CreateTrainFromComposition()`; `InputManager` does not construct train objects directly.

## Coordinate transformation

World clicks are converted through the existing camera coordinate helpers. Do not compare raw mouse coordinates with map cells.

F8 samples the mouse screen position through `Camera.ScreenToWorld`, converts it to a `MapPosition`, and opens the infrastructure editor for that track.

## Window resizing

The desktop game window is user-resizable. UI should use the current viewport/client bounds and measure text where required instead of assuming `1280x720`.

## AI rule

Do not introduce a second input singleton or coordinate system. Extend the existing `InputManager`/`InputState` flow when adding general gameplay controls. F6/F8/F9/F10 operational views are owned by `RailDispatchMonoGame` and Myra while domain state remains in the relevant managers/models. Keep one owner for each action.
