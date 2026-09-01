# Changelog

This file is the high-level release history. Detailed release notes are kept in `docs/changelog/`.

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
- Zachowano `PauseScreen` jako właściciela stanu pauzy oraz callbacków zapisu, odczytu i wyjścia.
- Zachowano obsługę `ESC` i istniejący kontrakt `ScreenManager`/`MenuScreen`.

## [0.1.2e] — Myra initialization-order fix
**Data:** 2026-09-01

- Naprawiono crash przy starcie wynikający z `MyraEnvironment.Game == null`.
- `MyraUIManager.Initialize(this)` jest wykonywane przed utworzeniem ekranów Myra.

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
