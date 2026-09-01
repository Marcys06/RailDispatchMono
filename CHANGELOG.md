# Changelog

This file is the high-level release history. Detailed release notes are kept in `docs/changelog/`.

## [0.1.2i] — Full Myra pause surface and UI consolidation
**Data:** 2026-09-01

- Usunięto zależność `PauseScreen` od legacy `MenuScreen`/`MenuEntry`; pauza ma teraz jedną widoczną powierzchnię UI — standardowe widgety Myra.
- Zachowano `ESC`/controller cancel jako wejście do wznowienia gry, ale bez drugiego legacy menu.
- `ZAPISZ GRĘ` i `WCZYTAJ GRĘ` pozostają wyłącznie w `MyraPauseView` i nadal wywołują logiczne callbacki `GameplayScreen`.
- Zapis/wczytanie korzystają ze wspólnego `MapSaveService`, który obejmuje mapę/infrastrukturę oraz runtime persistence aktywnego slotu.
- Uporządkowano kontrakt `MyraUIManager`: jeden `Desktop`, jeden aktywny root, renderowanie Myra po stacku `ScreenManager`; `Desktop.Render()` pozostaje miejscem obsługi inputu widgetów.
- Przeprowadzono audyt migracji UI: brak drugiej widocznej powierzchni Save/Load w `GameplayScreen`; HUD gameplay pozostaje świadomie poza Myra.
- Zaktualizowano indeks dokumentacji, kontekst AI, inventory UI i snapshot stanu do `0.1.2i`.

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
