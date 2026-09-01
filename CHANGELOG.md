# Changelog

This file is the high-level release history. Detailed release notes are kept in `docs/changelog/`.

## [0.1.2f] — Myra pause menu and main-menu layout
**Data:** 2026-09-01

- Wyśrodkowano główne menu Myra w poziomie i pionie.
- Usunięto `WCZYTAJ GRĘ` z menu startowego; zapis/wczytanie są dostępne z menu pauzy.
- Dodano `MyraPauseView` i przeniesiono widoczne menu pauzy na standardowe widgety Myra.
- Zachowano `PauseScreen` jako właściciela stanu pauzy oraz callbacków zapisu, odczytu i wyjścia.
- Zachowano obsługę `ESC` i istniejący kontrakt `ScreenManager`/`MenuScreen`.
- Usunięto stare renderowanie overlay/menu pauzy z `PauseScreen`; współdzielony `MyraUIManager` renderuje aktywny root.

## [0.1.2e] — Myra initialization-order fix
**Data:** 2026-09-01

- Naprawiono crash przy starcie wynikający z `MyraEnvironment.Game == null`.
- `MyraUIManager.Initialize(this)` jest teraz wykonywane w `RailDispatchMonoGame.Initialize()` przed utworzeniem i inicjalizacją `ScreenManager`.
- Zachowano drugie, idempotentne wywołanie w `LoadContent()` jako bezpieczny punkt zgodny z cyklem MonoGame.
- Nie zmieniono publicznego API istniejących ekranów ani kontraktu `ScreenManager`.
- Przyspieszono integrację Myra bez tworzenia drugiego systemu UI.

## [0.1.2c] — Myra main menu migration
**Data:** 2026-09-01

- Rozszerzono `MyraUIManager` o współdzielony root widget, czyszczenie desktopu i dynamiczne granice viewportu.
- Dodano standardowy widok `MyraMainMenuView` oparty o widgety Myra.
- Przeniesiono wizualną warstwę i obsługę myszy/klawiatury głównego menu do Myra.
- Zachowano istniejący `MenuEntry` i `MenuScreen` jako kontrakt kompatybilności oraz właściciela lifecycle.
- Wprowadzono jeden wspólny render Myra po renderowaniu stosu `ScreenManager`.
- Load Game, Settings, About i Pause pozostawały wtedy jeszcze na dotychczasowym UI.

## [0.1.2b] — Myra namespace compatibility fix
**Data:** 2026-09-01

- Poprawiono konflikt `Game` namespace/type w `MyraUIManager` przez jawny typ `Microsoft.Xna.Framework.Game`.
- Build `RailDispatchMono.Core` został przywrócony do stanu kompilowalnego.

## [0.1.2a] — Myra UI integration foundation
**Data:** 2026-09-01

- Dodano standardową bibliotekę `Myra` w wersji `1.6.5` do `RailDispatchMono.Core`.
- Dodano `MyraUIManager` jako wspólną granicę integracji Myra.
- Utworzono współdzielony Myra `Desktop`.

## [0.1.1] — Przebudowa dokumentacji
**Data:** 2026-08-31

- Zredukowano aktywny zestaw `docs/` do 20 dokumentów autorytatywnych.
- Usunięto przestarzałe snapshoty i jednorazowe dokumenty audytowe z aktywnego zestawu.
- Usunięto redundantny `architecture.json`.
- Dodano `19-current-state-0.1.1.md` jako snapshot aktualnego stanu.
- Utrzymano szczegółową historię w `docs/changelog/` i `CHANGELOG.md`.
