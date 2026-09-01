# Current State — 0.1.3e

`0.1.3e` is the current Myra gameplay HUD consolidation stage.

## Myra-owned gameplay UI

- Simulation clock.
- Simulation day (`GameDay`).
- Equal-width `x1`, `x2`, `x5` speed controls.
- Collapsible build-tools panel.
- Train list with camera focus.
- Station list with waiting-passenger count and camera focus.
- Main gameplay HUD root shared through `MyraUIManager`.

## Navigation

Train and station selection calculates the world-space camera position from the selected object's center and the current viewport/zoom. The selected object is therefore placed at the center of the visible world area.

## Legacy UI boundary

The old SpriteBatch train/station information panel and its hit-testing path have been removed from gameplay. There is no second train/station HUD implementation.

World-specific interaction surfaces that still use SpriteBatch (for example some legacy radial menus and tooltips) are separate from the Myra gameplay HUD and are candidates for later migration.

## Pause

Pause remains owned by `GameplayScreen` and uses the stable Myra pause surface. The gameplay HUD is restored after Resume.

## Planned remaining migrations

- Train/station detail windows.
- Wagon-route detail/editor window.
- Junction radial menu migration.
- Signal radial menu migration.
- Legacy tooltip migration where appropriate.
- Depot interaction window migration where appropriate.
- Richer state/color presentation for lists.
