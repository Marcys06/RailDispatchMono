# Input system

`InputState` remains the shared input snapshot/action layer used by the screen system. `InputManager` handles gameplay/world input interpretation. Coupling commands currently use the existing `TrainManager.HandleCouplingHotkeys()` path and must not be duplicated by another UI/input owner.

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

## Coupling and decoupling controls

The current rigid coupling command path is:

- `C` — couple the nearest valid outer-boundary candidate;
- `X` — decouple the last coupling created by `C`, with fallback to the first remaining runtime connection;
- `F6` — select a `3 km/h` shunting/coupling speed limit;
- `F7` — select a `4 km/h` shunting/coupling speed limit;
- `F8` — select a `5 km/h` shunting/coupling speed limit (default).

`C` executes only when the selected candidate passes the authoritative `CouplingService` validation and both participating trains are at or below the selected shunting limit. `X` delegates decoupling to the same domain service.

These commands are currently implemented in `TrainManager` as the temporary `0.1.5` command path. Do not add a second coupling/decoupling command implementation to `InputManager`, Myra or a screen until the planned vehicle/end selection UI replaces the temporary command path.

## Pause input ownership

At the current baseline, pause is owned by `GameplayScreen`.

- `ESC` is handled by the gameplay screen as the authoritative pause/resume toggle.
- The pause UI is rendered by `MyraPauseView` through the shared `MyraUIManager`.
- Myra pointer input is handled by the shared `Desktop`.
- `InputManager` does not own a second pause menu and does not compete with Myra for pause button clicks.
- The pause menu is not a `GameScreen` popup, so it cannot block gameplay by remaining in the `ScreenManager` stack after resume.

Pause button actions are dispatched to gameplay-owned operations and are executed through the normal update lifecycle rather than mutating screen/gameplay state from inside a Myra render callback.

## Wagon route edit mode

Pressing `S` toggles wagon route edit mode and clears the active build mode. While the mode is active, the HUD/menu shows a small active `S` indicator.

With the mode active, a new LPM click on a wagon opens its screen-space route editor. The editor handles station buttons independently, supports adding/removing/clearing stations and persists route changes through the existing schedule storage. PPM closes the editor.

## Station building

Station mode supports `1x1`, `2x2`, `3x3` and `4x4` areas. The complete selected rectangle must contain track and cannot overlap another station.

## Depot building

Depot mode is activated with `9`. A depot is a world building and does not require a track cell. The building is rendered using programmatic geometry. Clicking an existing depot opens the Depot workflow; removal remains available through the existing right-click interaction.

`DepotScreen` and `MyraDepotView` own builder interaction. Train creation goes through `TrainManager.CreateTrainFromComposition()`; `InputManager` does not construct train objects directly.

## Coordinate transformation

World clicks are converted through `Camera.ScreenToMap`. Do not compare raw mouse coordinates with map cells.

## Window resizing

The desktop game window is user-resizable. UI should use the current viewport/client bounds and measure text where required instead of assuming `1280x720`.

## AI rule

Do not introduce a second input singleton or coordinate system. Extend the existing `InputManager`/`InputState` flow when adding general gameplay controls. For Myra menus, use the shared `MyraUIManager`/`Desktop` and preserve a single owner for each action. The temporary coupling command remains in `TrainManager` until its planned vehicle/end selection UI replaces it.
