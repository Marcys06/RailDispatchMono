# Changelog

This file is the high-level release history. Detailed release notes are kept in `docs/changelog/`.

## [0.1.2j] — Final Myra integration and pause-resume lifecycle fix
**Data:** 2026-09-01

- Naprawiono `WZNÓW GRĘ`: kliknięcie przycisku Myra poprawnie kończy stan pauzy i usuwa dokładnie ten `PauseScreen` ze `ScreenManager`.
- `PauseScreen` nie wykonuje już drugiego `ExitScreen()` po wywołaniu callbacku Resume; lifecycle jest zamykany wyłącznie przez `GameplayScreen.ResumeGame()`.
- Zachowano `ESC`/controller cancel jako alternatywną drogę wznowienia, korzystającą z tego samego lifecycle path.
- Potwierdzono jeden współdzielony `MyraUIManager`/`Desktop` oraz czyszczenie root widgetu przy opuszczaniu aktywnego ekranu.
- Main Menu, Settings, About i Pause korzystają ze standardowych widgetów Myra.
- Save/Load pozostają wyłącznie w Myra Pause Menu; Gameplay HUD nie posiada drugiej powierzchni persistence.
- `0.1.2j` zamyka zakres migracji Myra dla linii `0.1.2x`. Dalsze prace nad UI/Myra wymagają osobnego celu wydaniowego.
- Zaktualizowano dokumentację AI, inventory UI, kontekst architektury i changelog do finalnego stanu integracji Myra.

## [0.1.2i] — Full Myra pause surface and UI consolidation
**Data:** 2026-09-01

- Usunięto zależność `PauseScreen` od legacy `MenuScreen`/`MenuEntry`; pauza ma teraz jedną widoczną powierzchnię UI — standardowe widgety Myra.
- Zachowano `ESC`/controller cancel jako wejście do wznowienia gry, ale bez drugiego legacy menu.
- `ZAPISZ GRĘ` i `WCZYTAJ GRĘ` pozostają wyłącznie w `MyraPauseView` i wywołują logiczne callbacki `GameplayScreen`.
- Po zapisie `WCZYTAJ GRĘ` jest aktywowane w bieżącym menu pauzy, więc nie trzeba go zamykać i otwierać ponownie.
- Usunięto stare przyciski SAVE/LOAD z `InputManager` oraz skróty F6/F7; persistence ma jedno miejsce wejścia w Pause Menu.
- Zapis/wczytanie korzystają ze wspólnego `MapSaveService`, który obejmuje mapę/infrastrukturę oraz runtime persistence aktywnego slotu.
- Uporządkowano kontrakt `MyraUIManager`: jeden `Desktop`, jeden aktywny root, renderowanie Myra po stacku `ScreenManager`; `Desktop.Render()` pozostaje miejscem obsługi inputu widgetów.
- Przeprowadzono audyt migracji UI: HUD gameplay nie posiada drugiej widocznej powierzchni Save/Load; HUD gameplay, rendering torów i radialne narzędzia pozostają świadomie poza Myra.
- Zaktualizowano dokumentację AI, inventory UI, indeks i current-state snapshot do `0.1.2i`.

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
