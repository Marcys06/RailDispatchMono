# Current State — 0.1.3pre

`0.1.3pre` is the current consolidated pre-release state of the Myra gameplay UI migration.

## Myra-owned gameplay UI

- Gameplay HUD root.
- Simulation clock.
- Simulation day (`GameDay`).
- Simulation speed controls.
- Collapsible build-tools panel.
- Train list.
- Station list with waiting-passenger counts.
- Train/station camera-focus interactions.
- Pause UI surface.
- Main Menu, Settings and About UI surfaces.

## Single-source UI rule

Train and station information is presented through Myra only. The former duplicate SpriteBatch train/station HUD and its hit-testing path are not part of the gameplay UI anymore.

The same principle applies to the migrated clock/speed presentation: legacy duplicate rendering must not be reintroduced.

## Camera navigation

Selecting a train or station from the Myra information panel calculates the target from the selected object's world-space center and moves the gameplay camera so the object is centered in the viewport.

## Pause

Pause state is owned by `GameplayScreen`. `MyraPauseView` is the visual/input surface. Pause is not represented as a second gameplay `GameScreen`.

## Remaining migration candidates

- Junction radial interaction UI.
- Signal radial interaction UI.
- Legacy world tooltips where appropriate.
- Dedicated train detail window.
- Dedicated station detail window.
- Dedicated wagon-route detail/editor window.
- Depot interaction UI where applicable.
- Advanced configurable train/station colors and state visualization.

## Version policy

`0.1.3pre` is the current milestone. Unless a new feature materially changes the scope, subsequent work should be treated as bugfixes, polish or completion work against this baseline.
