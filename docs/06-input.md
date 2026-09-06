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
- `R` — rotate current track/junction; in station mode cycle station size
- `J` — toggle signal or junction switch
- `LMB` on an existing track in `None` build mode — select track
- `Shift + LMB` — add track to multi-selection
- `Ctrl + LMB` — toggle track in multi-selection
- `PPM` — remove/open object menu
- `Shift + PPM` — explicit removal for objects that support it
- `MMB` — move camera
- mouse wheel — zoom camera
- `Escape` / `P` — pause/resume

## GUI input lock

When any temporary Myra GUI replaces the gameplay root, gameplay/world mouse input is completely suspended.

This means clicks on the map do **not**:

- build a track even if build mode `1` was selected;
- select or deselect tracks;
- remove tracks or other world objects;
- toggle signals or junctions;
- move or zoom the gameplay camera through world input.

The GUI itself continues to receive the mouse normally. Closing the GUI returns to gameplay with build mode reset to `None`, preventing the click used to close a window from immediately placing a track.

This rule is centralized through `MyraUIManager.IsGameplayOverlayOpen` and enforced by `GameplayScreen` before calling `InputManager.Update()`.

## Operational shortcuts

- `F6` — **WYMUSZENIE PRZEJAZDU** for the selected train; bypasses dispatcher arbitration only;
- `F7` — change travel direction under the existing movement rules;
- `F8` — open infrastructure editor for the track under the mouse cursor;
- `F9` — open the selected locomotive's timetable editor;
- `F10` — open railway/dispatcher diagnostics;
- `F11` — open the named railway line editor and bulk infrastructure editor.

Opening F8/F9/F10/F11 explicitly cancels the active track-building mode before showing the GUI.

## Named railway lines — F11

The F11 editor operates on the multi-track selection created directly on the map. It can:

- create a named `RailwayLine` from the selection;
- select an existing line on the map;
- add/remove the current selection from an existing named line;
- mass-apply `TrackType`, `LineClass` and `TractionSystem` to the selection or whole line;
- select the whole connected track area from one selected starting cell;
- delete a line without deleting physical track.

Named lines are organizational infrastructure metadata. They do not replace blocks, signals, switches, routes or dispatcher logic.

## Infrastructure editor — F8

F8 opens `MyraTrackInfrastructureView` for the `TrackCell` beneath the mouse cursor. It edits `TrackType`, `LineClass` and `TractionSystem` on one segment.

## Save format

Named lines are persisted in map schema `3`. Older map schemas are intentionally not accepted by the current loader; the project does not require backward compatibility for these development versions.

## Pause input ownership

Pause remains owned by `GameplayScreen`. Myra pointer input is handled by the shared `Desktop`. `InputManager` does not own a second pause menu.

## Wagon route edit mode

Pressing `S` toggles wagon route edit mode and clears the active build mode. With the mode active, LPM on a wagon opens its route editor.

## Coordinate transformation

World clicks are converted through the existing camera coordinate helpers. Do not compare raw mouse coordinates with map cells.

## AI rule

Do not introduce a second input singleton or coordinate system. Extend the existing `InputManager`/`InputState` flow when adding general gameplay controls. F6/F8/F9/F10/F11 operational views are owned by `RailDispatchMonoGame` and Myra while domain state remains in the relevant managers/models. Keep one owner for each action. Any new gameplay GUI must use the same world-input lock contract: while the GUI replaces the gameplay root, `InputManager.Update()` must not process world input.
