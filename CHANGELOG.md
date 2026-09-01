# Changelog

This file is the high-level release history. Detailed release notes are kept in `docs/changelog/`.

## [0.1.2pre] — Myra UI stabilization preview
**Data:** 2026-09-01

- Stabilized the Myra UI integration after the `0.1.2a`–`0.1.2k` development stages.
- Fixed `FloatingTextManager` rendering so `SpriteBatch.DrawString` is executed inside a valid `Begin`/`End` scope.
- Removed the legacy `PauseScreen` dependency from `InputManager`.
- `ESC` is handled exclusively by `GameplayScreen` for entering/leaving pause.
- Pause lifecycle was rebuilt so pause state is owned by `GameplayScreen` and Myra owns only the visible pause surface.
- Pause Save/Load/Resume actions are dispatched through the shared Myra UI integration without inserting a second pause `GameScreen`.
- Preserved the standard Myra Main Menu, Pause, Settings and About surfaces.
- Current working state has been verified by the developer after an additional small bugfix.
- Build status recorded by the `0.1.2pre` commit: 0 errors.

`0.1.2pre` is the current development version. The lettered `0.1.2a`–`0.1.2k` stages remain historical/immutable milestones.

## [0.1.2k] — Pause-system rebuild
**Data:** 2026-09-01

- Rebuilt the pause lifecycle around `GameplayScreen` rather than a second popup screen.
- Removed the obsolete `PauseScreen` implementation from the runtime screen stack.
- Kept `MyraPauseView` as the visible UI surface.
- Centralized pause state, resume, save, load and quit ownership in `GameplayScreen`.
- Eliminated the legacy popup lifecycle path that could consume Myra commands without changing the gameplay pause state.

## [0.1.2j] — Myra pause action stabilization
**Data:** 2026-09-01

- Introduced several incremental fixes for Myra pause action dispatch and update ordering.
- Moved pause actions away from direct Myra render callbacks.
- Corrected action processing order so gameplay state changes occur during the game update lifecycle.
- Final `0.1.2j` experiments led to the subsequent pause-system rebuild used by `0.1.2k`.

## [0.1.2i] — Full Myra pause surface and UI consolidation
**Data:** 2026-09-01

- Usunięto zależność `PauseScreen` od legacy `MenuScreen`/`MenuEntry`; pauza ma jedną widoczną powierzchnię UI — standardowe widgety Myra.
- Zachowano `ESC`/controller cancel jako wejście do wznowienia gry.
- `ZAPISZ GRĘ` i `WCZYTAJ GRĘ` pozostają wyłącznie w `MyraPauseView`.
- Po zapisie `WCZYTAJ GRĘ` jest aktywowane w bieżącym menu pauzy.
- Usunięto stare przyciski SAVE/LOAD z `InputManager` oraz skróty F6/F7.
- Persistence korzysta ze wspólnego `MapSaveService`.
- Uporządkowano kontrakt `MyraUIManager`: jeden `Desktop`, jeden aktywny root i jeden hostowy render pass.

## [0.1.2h] — Myra pause input fix
**Data:** 2026-09-01

- Naprawiono aktualizację wejścia pointer/keyboard dla Myra `Desktop` w menu pauzy.
- Przywrócono prawidłowe przetwarzanie interakcji widgetów podczas pauzy.
- Uporządkowano odpowiedzialność za obsługę wejścia tak, aby Myra mogła odbierać kliknięcia bez tworzenia drugiego globalnego systemu input.

## [0.1.2g] — Myra menu migration and persistence UI consolidation
**Data:** 2026-09-01

- Migrated `SettingsScreen` to Myra.
- Migrated `AboutScreen` to Myra.
- Added `MyraSettingsView` and `MyraAboutView`.
- Settings and About keep their existing logical ownership in the screen layer while Myra owns the visible widgets.
- Pause remains the functional location for gameplay Save/Load.
- Main Menu continues without a separate Load Game entry.
- The developer-side `Enabled` correction is part of the current baseline.

## [0.1.2f] — Myra pause menu and main-menu layout
**Data:** 2026-09-01

- Wyśrodkowano główne menu Myra w poziomie i pionie.
- Usunięto `WCZYTAJ GRĘ` z menu startowego; zapis/wczytanie są dostępne z menu pauzy.
- Dodano `MyraPauseView` i przeniesiono widoczne menu pauzy na standardowe widgety Myra.
- Zachowano obsługę `ESC` i istniejący kontrakt `ScreenManager`/`MenuScreen`.

## [0.1.2e] — Myra initialization-order fix
**Data:** 2026-09-01

- Naprawiono crash przy starcie wynikający z `MyraEnvironment.Game == null`.
- `MyraUIManager.Initialize(this)` jest wykonywane przed utworzeniem ekranów Myra.

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
- Zapis rozkładów został przeniesiony do aktywnego slotu.

## Historical releases

Older releases are documented in `docs/changelog/`. When a historical commit has no reliable release description, its detailed entry should be recorded simply as `bugfix` rather than inventing functionality that is not supported by the repository history.
