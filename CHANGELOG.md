# Changelog

This file is the high-level release history. Detailed release notes are kept in `docs/changelog/`.

## [0.1.4g] — Rolling stock visuals and locomotive power
**Data:** 2026-09-03

- Added differentiated rolling-stock rendering: electric locomotives are red, diesel locomotives are black, and passenger coach variants use distinct blue shades.
- Added centered white rolling-stock labels: `EP07`, `EU200`, `SU42`, `1KL`, `2KL`, `3KL`.
- Labels remain readable when trains travel in either direction.
- Added `LocomotiveParameters` with `PowerMW`.
- Calibrated locomotive power: EP07 2.0 MW, EU200 5.5 MW, SU42 1.2 MW.
- Added nonlinear power/load Vmax model with a 0.006 MW/t supported-mass threshold and 0.55 load exponent.
- EU200 + 10 passenger wagons remains at 200 km/h; SU42 + 5 wagons is approximately 75 km/h and SU42 + 10 wagons approximately 55 km/h.
- Passenger coach catalogue Vmax was raised to 200 km/h so EU200 is not artificially capped by the coach definitions.
- The 0.1.4f nonlinear mass effect on acceleration/braking remains unchanged and independent from the new power/Vmax model.
- Detailed notes: `docs/changelog/0.1.4g.md`.
- No `current-state` snapshot was created; `0.1.4g` is a lettered development stage.

## [0.1.4f] — Depot Myra migration and consist mass performance
**Data:** 2026-09-01

- Removed the legacy `DepotTrainMenu` SpriteBatch train-preset UI.
- Removed the three hardcoded short/standard/long consist presets.
- Clicking an existing Depot now opens `DepotScreen` directly through `InputManager.DepotSelected`.
- Depot train creation is fully owned by `MyraDepotView` and `DepotScreen`.
- Removed the former preset-based `InputManager` train spawning path and its hardcoded EU06-style `VehicleParameters`.
- `TrainManager.CreateTrainFromComposition()` remains the single train-creation path for Depot consists.
- Kept the shared single `MyraUIManager` / `Desktop`; no second Myra UI host was introduced.
- Depot remains a full-screen `GameScreen`; gameplay is covered while the builder is active and the gameplay Myra root is restored on close.
- Added non-linear consist-mass influence to acceleration and braking: `1 / (totalMass / locomotiveMass)^1.30`.
- The mass exponent is approximately 30% stronger than a linear inverse-mass relationship; locomotive-only performance remains unchanged.
- Updated documentation baseline and final `0.1.4f` release notes.
- No `current-state` snapshot was created; `0.1.4f` is a lettered development stage.
- Detailed notes: `docs/changelog/0.1.4f.md`.

## [0.1.4e] — Rolling stock and Depot train builder
**Data:** 2026-09-01

- Added reusable rolling-stock catalogue with EP07, EU200 — Newag Griffin E4ACP and SU42.
- Added three passenger coach definitions.
- Added physical rolling-stock metadata in tonnes/metres while keeping internal speed in m/s and established visual grid geometry.
- Extended `TrainComposition` with total mass, total physical length, effective Vmax and wagon management.
- Added full-screen Myra `DepotScreen` / `MyraDepotView` train builder.
- Depot-created consists are routed through `TrainManager.CreateTrainFromComposition()`.
- A locomotive-only consist is valid; one locomotive plus zero or more wagons is supported.
- Removed the hard-coded test train from `GameplayScreen`; the development test track remains.
- No changes to RadioStop or semaphore mechanics.
- Detailed notes: `docs/changelog/0.1.4e.md`.

## [0.1.4d] — RadioStop safety distance
**Data:** 2026-09-01

- RadioStop safety distance increased from 2 to 3 cells.
- RadioStop continues to inspect all vehicles of the other train.
- Detailed notes: `docs/changelog/0.1.4d.md`.

## [0.1.4c] — Train speed display and stop distance
**Data:** 2026-09-01

- User-facing train speeds are displayed in km/h while internal simulation remains in m/s.
- Stop braking target was moved to the accepted 0.8-cell front clearance before the signal.
- Train acceleration/braking response was softened.
- Detailed notes: `docs/changelog/0.1.4c.md`.

## [0.1.4b] — Simulation scale and simulation clock
**Data:** 2026-09-01

- Added centralized `SimulationScale` with `1 grid cell = 10 metres`.
- Train physical speeds remain in m/s; movement converts metres to grid cells through the central scale.
- Train vehicle positions and rendered vehicle lengths use the same spatial scale.
- Signal braking distances are converted from route cells to metres before physical braking calculations.
- Block metre lengths now use the centralized spatial scale.
- `GameClock.Update()` now returns simulation elapsed time rather than raw real elapsed time.
- Gameplay systems consume the authoritative simulation delta, so train acceleration, braking and movement follow the same x1/x2/x5 time scale as the simulation clock.
- Detailed notes: `docs/changelog/0.1.4b.md`.

## [0.1.4a] — Train and semaphore mechanics
**Data:** 2026-09-01

- Train compositions spawn/render using physical vehicle spacing instead of stacking all vehicles at the head position.
- Spawn and train collision checks use spaced vehicle positions.
- Added persistent per-train semaphore speed limits.
- Semaphore state is tracked by signal identity, so multiple directional signals in one cell remain independent.
- A semaphore changes the train's persistent speed limit only after it is passed.
- `Clear` no longer causes acceleration before the train passes it.
- Existing BlockController/StationController stop and dwell systems remain authoritative for their respective responsibilities.
- Detailed notes: `docs/changelog/0.1.4a.md`.

## [0.1.3pre] — Myra Gameplay UI stabilization
**Data:** 2026-09-01

- Consolidated the `0.1.3a`–`0.1.3e` Myra gameplay HUD work.
- Gameplay clock and `GameDay` are Myra-owned.
- Simulation speed controls are Myra-owned.
- Build tools use a collapsible Myra panel.
- Train/station lists use Myra as the single presentation and interaction layer.
- Train/station selection centers the camera on the selected world object.
- Main Menu → Gameplay Myra lifecycle is corrected.
- Pause remains a stable `GameplayScreen`-owned state with `MyraPauseView` as its UI surface.
- Remaining legacy world-interaction UI is tracked explicitly for future migration.

## [0.1.3e]
**Data:** 2026-09-01

- Equal-width speed controls and train/station camera navigation.
- Removed duplicate legacy train/station HUD.

## [0.1.3d]
**Data:** 2026-09-01

- Cleaned up Myra HUD migration and train/station presentation.

## [0.1.3c]
**Data:** 2026-09-01

- Reorganized gameplay HUD into a dedicated right-side information area.
- Added collapsible build tools and Myra train/station lists.

## [0.1.3b]
**Data:** 2026-09-01

- Gameplay HUD layout polish and collapsible tools.

## [0.1.3a]
**Data:** 2026-09-01

- Initial large Myra gameplay HUD integration.

## [0.1.2pre]
**Data:** 2026-09-01

- Myra UI stabilization preview and pause-system stabilization.

## [0.1.2k]
**Data:** 2026-09-01

- Rebuilt pause lifecycle around `GameplayScreen`.

## [0.1.2j]
**Data:** 2026-09-01

- Stabilized Myra pause action dispatch and update ordering.

## [0.1.2i]
**Data:** 2026-09-01

- Consolidated Myra pause surface and persistence UI.

## [0.1.2h]
**Data:** 2026-09-01

- Fixed Myra pause input handling.

## [0.1.2g]
**Data:** 2026-09-01

- Migrated Settings and About screens to Myra.

## [0.1.2f]
**Data:** 2026-09-01

- Migrated main menu/pause presentation to Myra.

## [0.1.2e]
**Data:** 2026-09-01

- Fixed Myra initialization order.

## [0.1.2d]
**Data:** 2026-09-01

- Fixed Myra/MonoGame `Game` namespace collision.

## [0.1.2c]
**Data:** 2026-09-01

- Migrated main menu visual layer to Myra.

## [0.1.2b]
**Data:** 2026-09-01

- Fixed Myra namespace compatibility.

## [0.1.2a]
**Data:** 2026-09-01

- Added Myra integration foundation.

## [0.1.1]
**Data:** 2026-08-31

- Documentation restructuring.

## [0.0.16]
**Data:** 2026-08-31

- Save slots, Main Menu and runtime persistence.

## Historical releases

Older releases are documented in `docs/changelog/`. When a historical commit has no reliable release description, its detailed entry should be recorded simply as `bugfix` rather than inventing functionality.
