# Changelog

This file is the high-level release history. Detailed release notes are kept in `docs/changelog/`.

## [0.1.3e] — Myra HUD consolidation and navigation
**Data:** 2026-09-01

- Fixed `x1` / `x2` / `x5` so all three Myra controls have equal fixed widths.
- Kept speed controls directly below the simulation clock.
- Removed the duplicate legacy SpriteBatch train/station information HUD and its hit-testing path.
- Train and station information is now presented and interacted with through the Myra gameplay HUD only.
- Clicking a train centers the camera on the selected train.
- Clicking a station centers the camera on the station's actual center.
- Added a dedicated `0.1.3e` current-state document and changelog.
- Marked remaining legacy world-interaction UI explicitly for later migration instead of maintaining duplicate HUD implementations.

## [0.1.3d] — Myra HUD migration cleanup
**Data:** 2026-09-01

- Kept `x1` / `x2` / `x5` as equal-width Myra controls directly below the simulation clock.
- Completed the train/station HUD migration to Myra as the single presentation and interaction layer.
- Removed the legacy SpriteBatch train/station panel rendering from `GameplayScreen`.
- Removed the legacy train/station panel mouse-hit handling.
- Preserved train camera-centering and station waiting-passenger information.

## [0.1.3c] — Myra Gameplay HUD layout
**Data:** 2026-09-01

- Reorganized the gameplay HUD around a dedicated right-side information area.
- Moved speed controls and build tools directly below the clock.
- Made the build-tools section collapsible.
- Added Myra train and station lists with waiting-passenger information and camera-centering actions.
- Improved HUD widths, spacing and alignment.

## [0.1.3b] — Gameplay HUD layout polish
**Data:** 2026-09-01

- Reworked the gameplay tool panel into a collapsible Myra section.
- Moved the `x1` / `x2` / `x5` simulation controls to the bottom-right corner of the HUD.
- Kept the speed controls fully functional through the existing simulation clock.
- Tightened HUD spacing, widths and alignment to reduce visual drift between panels.
- Kept the simulation clock and `GameDay` presentation unchanged because their current layout is acceptable.
- Kept the existing train and station list functionality unchanged.

## [0.1.3a] — Myra Gameplay UI
**Data:** 2026-09-01

- Added the first large Myra gameplay HUD integration.
- Migrated the simulation clock and `GameDay` display to `MyraGameplayView`.
- Removed the legacy SpriteBatch-rendered clock and speed controls from `GameplayScreen`.
- Added `x1` / `x2` / `x5` simulation speed controls to Myra.
- Added live train and station lists with approximately 0.5 s refresh cadence.
- Station entries display the number of waiting passengers.
- Train and station entries dispatch camera-centering actions.
- Added Myra build-tool controls for straight track, curve, junction, signal, station and depot.
- Added a Myra control for the existing wagon route-edit workflow.
- Kept existing keyboard shortcuts as an additional input path.
- Preserved pause as a `GameplayScreen`-owned state while allowing the gameplay Myra root to be restored after pause.
- Fixed the main-menu-to-game transition so `MainMenuScreen.UnloadContent()` cannot clear the newly installed gameplay Myra root.

## [0.1.2pre] — Myra UI stabilization preview
**Data:** 2026-09-01

- Stabilized the Myra UI integration after the `0.1.2a`–`0.1.2k` development stages.
- Fixed `FloatingTextManager` rendering so `SpriteBatch.DrawString` is executed inside a valid `Begin`/`End` scope.
- Removed the legacy `PauseScreen` dependency from `InputManager`.
- `ESC` is handled exclusively by `GameplayScreen` for entering/leaving pause.
- Pause lifecycle was rebuilt so pause state is owned by `GameplayScreen` and Myra owns only the visible pause surface.
- Pause Save/Load/Resume actions are dispatched through the shared Myra UI integration without inserting a second pause `GameScreen`.
- Preserved the standard Myra Main Menu, Pause, Settings and About surfaces.
- Current working state was verified by the developer after an additional small bugfix.
- Build status recorded by the `0.1.2pre` commit: 0 errors.

## [0.1.2k] — Pause-system rebuild
**Data:** 2026-09-01

- Rebuilt the pause lifecycle around `GameplayScreen` rather than a second popup screen.
- Removed the obsolete `PauseScreen` implementation from the runtime screen stack.
- Kept `MyraPauseView` as the visible UI surface.
- Centralized pause state, resume, save, load and quit ownership in `GameplayScreen`.

## [0.1.2j] — Myra pause action stabilization
**Data:** 2026-09-01

- Introduced incremental fixes for Myra pause action dispatch and update ordering.
- Moved pause actions away from direct Myra render callbacks.
- Corrected action processing order so gameplay state changes occur during the game update lifecycle.

## [0.1.2i] — Full Myra pause surface and UI consolidation
**Data:** 2026-09-01

- Usunięto zależność `PauseScreen` od legacy `MenuScreen`/`MenuEntry`.
- Zachowano `ESC`/controller cancel jako wejście do wznowienia gry.
- `ZAPISZ GRĘ` i `WCZYTAJ GRĘ` pozostają wyłącznie w `MyraPauseView`.
- Persistence korzysta ze wspólnego `MapSaveService`.
- Uporządkowano kontrakt `MyraUIManager`: jeden `Desktop`, jeden aktywny root i jeden hostowy render pass.

## [0.1.2h] — Myra pause input fix
**Data:** 2026-09-01

- Naprawiono aktualizację wejścia pointer/keyboard dla Myra `Desktop` w menu pauzy.
- Przywrócono prawidłowe przetwarzanie interakcji widgetów podczas pauzy.

## [0.1.2g] — Myra menu migration and persistence UI consolidation
**Data:** 2026-09-01

- Migrated `SettingsScreen` and `AboutScreen` to Myra.
- Added `MyraSettingsView` and `MyraAboutView`.
- Pause remains the functional location for gameplay Save/Load.

## [0.1.2f] — Myra pause menu and main-menu layout
**Data:** 2026-09-01

- Wyśrodkowano główne menu Myra.
- Usunięto `WCZYTAJ GRĘ` z menu startowego.
- Dodano `MyraPauseView`.

## [0.1.2e] — Myra initialization-order fix
**Data:** 2026-09-01

- Naprawiono crash przy starcie wynikający z `MyraEnvironment.Game == null`.

## [0.1.2d] — Myra Game namespace/type collision fix
**Data:** 2026-09-01

- Naprawiono konflikt nazwy `Game` pomiędzy przestrzenią nazw Myra a typem MonoGame.

## [0.1.2c] — Myra main menu migration
**Data:** 2026-09-01

- Dodano `MyraMainMenuView`.
- Przeniesiono wizualną warstwę głównego menu do Myra.

## [0.1.2b] — Myra namespace compatibility fix
**Data:** 2026-09-01

- Poprawiono konflikt `Game` namespace/type w `MyraUIManager`.

## [0.1.2a] — Myra UI integration foundation
**Data:** 2026-09-01

- Dodano bibliotekę `Myra` 1.6.5.
- Dodano współdzielony `MyraUIManager`.

## [0.1.1] — Przebudowa dokumentacji
**Data:** 2026-08-31

- Zredukowano aktywny zestaw `docs/` do 20 dokumentów autorytatywnych.
- Uporządkowano dokumentację i historię zmian.

## [0.0.16] — Save slots, Main Menu i runtime persistence
**Data:** 2026-08-31

- Dodano save slots i wersjonowanie danych zapisu.
- Dodano Main Menu i runtime persistence.

## Historical releases

Older releases are documented in `docs/changelog/`. When a historical commit has no reliable release description, its detailed entry should be recorded simply as `bugfix` rather than inventing functionality that is not supported by the repository history.
