# Changelog

This file is the high-level release history. Detailed release notes are kept in `docs/changelog/`.

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
- Latest developer bugfix is included in this pre-release state.
- Remaining legacy world-interaction UI is tracked explicitly for future migration.

## [0.1.3e]
**Data:** 2026-09-01

- Equal-width speed controls and train/station camera navigation.
- Removed duplicate legacy train/station HUD.
- Updated current-state documentation.

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
