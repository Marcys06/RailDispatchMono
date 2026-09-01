# Current State — 0.1.3pre

`0.1.3pre` is the current Myra gameplay UI pre-release snapshot.

## Myra gameplay UI

- Gameplay HUD is presented through Myra.
- Simulation clock and `GameDay` are displayed by Myra.
- `x1`, `x2`, `x5` simulation speed controls are Myra buttons with equal sizing.
- Build tools use a collapsible Myra panel.
- Train and station information uses a single Myra implementation.
- Station entries display waiting passenger counts.
- Selecting a train or station from the Myra panel centers the camera on the selected object's world-space center.

## Main Menu and pause

- Main Menu uses Myra and correctly transitions to gameplay without clearing the gameplay HUD.
- Pause is owned by `GameplayScreen` and uses `MyraPauseView`.
- Resume, Save, Load and Quit follow the stable pause action path.

## UI architecture

Myra is the presentation and interaction layer for the migrated application/gameplay UI. Simulation, map, railway, train, station, camera and building systems remain authoritative gameplay systems.

The legacy duplicate train/station HUD and legacy clock presentation have been removed. New migrated UI must not introduce a second presentation path for the same information.

## Remaining UI migration

The following gameplay interaction surfaces are not yet fully migrated to Myra:

- junction interaction/radial menu,
- signal interaction/radial menu,
- legacy floating/tooltips where still used,
- dedicated train detail window,
- dedicated station detail window,
- wagon-route detail/editor window,
- depot-specific interaction UI where applicable,
- richer configurable state/color presentation.

## Documentation rule

This file is a snapshot for the `pre` milestone only. Lettered development stages are documented in the changelog and do not receive separate current-state snapshots.
