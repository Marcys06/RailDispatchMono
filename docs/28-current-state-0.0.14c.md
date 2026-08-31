# RailDispatchMono — current implementation state

Last updated: 2026-08-31

This document is the current implementation snapshot for `0.0.14c`.

## Simulation and railway safety

- Train movement operates on the logical track network, including straight track, curves and junctions.
- Signals are integrated with train speed control and block occupancy.
- The `0.0.14` collision controller provides a simple emergency stop when another train is detected on the selected route and no protecting signal takes precedence.
- Collision protection uses a 2-cell safety distance.
- `RadioStop` sets train speed to zero and is cleared when the collision condition is gone.
- Train spawning validates the complete consist against occupied cells.

## Railway objects

- Straight track, curves and junctions are placeable.
- Junctions have switch state and determine the train's route through the cell.
- Signals support multiple aspects and remain the primary movement-protection mechanism.
- Stations are rectangular areas with supported sizes `1x1`, `2x2`, `3x3` and `4x4`.
- Depots are world buildings and support the current depot/train-selection workflow.

## Wagon routes

- Each wagon has an independent `TrainRoute` containing ordered station IDs and route progress.
- Wagon routes do not directly control locomotive movement; physical movement remains governed by tracks, junctions and signals.
- Wagon route changes are persisted through the existing schedule storage path.
- The route editor allows adding, removing and clearing station stops.

## Wagon route input/UI

- `S` toggles wagon route edit mode and clears the active build mode.
- While route edit mode is active, a small active `S` indicator is displayed in the route menu.
- `S + LPM` on a wagon opens its route editor.
- The first update after opening the route menu is consumed so the opening input cannot immediately become another menu action.
- Only a subsequent new LPM click can activate a menu action or close the menu.
- PPM closes the route menu.
- Station buttons are handled independently so a click on one station cannot fall through into another menu action.
- Escape closes the active route-edit mode/menu.

## Passengers

- Wagons manage passenger capacity independently.
- A configured wagon route restricts boarding to passengers whose destination is served by that wagon.
- Empty routes retain the existing legacy passenger-boarding behavior.

## Current controls

- `1` / NumPad `1` — straight track
- `2` / NumPad `2` — curve
- `3` / NumPad `3` — junction
- `4` / NumPad `4` — signal
- `5` / NumPad `5` — station
- `9` / NumPad `9` — depot
- `S` — wagon route edit mode
- `R` — rotate/change current build element; in station mode changes station size
- `J` — signal/switch quick toggle
- `LMB` — build/select; in `S` mode select a wagon for route editing
- `PPM` — remove/open object menu; closes the wagon route editor when it is active
- `Shift + PPM` — explicit removal where supported
- `MMB` — camera movement
- mouse wheel — camera zoom
- `Escape` / `P` — pause

## UI architecture

UI is generated programmatically using `SpriteBatch`, `SpriteFont` and simple generated textures. The wagon route editor is screen-space UI and clamps itself to the current viewport.

## Scope

Still outside the current implementation:

- full interlocking,
- route reservation,
- realistic braking distances against other trains,
- ATP/ETCS,
- complete block-based train detection,
- automatic train priority management,
- automatic route planning for locomotives,
- coupling and decoupling,
- procedural city generation.
