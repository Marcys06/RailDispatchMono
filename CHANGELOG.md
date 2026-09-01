# Changelog

This file is the high-level release history. Detailed release notes are kept in `docs/changelog/`.

## [0.1.2c] — Myra main menu migration
**Data:** 2026-09-01

- Rozszerzono `MyraUIManager` o współdzielony root widget, czyszczenie desktopu i dynamiczne granice viewportu.
- Dodano standardowy widok `MyraMainMenuView` oparty o widgety Myra.
- Przeniesiono wizualną warstwę i obsługę myszy/klawiatury głównego menu do Myra.
- Zachowano istniejący `MenuEntry` i `MenuScreen` jako kontrakt kompatybilności oraz właściciela lifecycle.
- Wprowadzono jeden wspólny render Myra po renderowaniu stosu `ScreenManager`.
- Load Game, Settings, About i Pause pozostają jeszcze na dotychczasowym UI.

## [0.1.2b] — Myra namespace compatibility fix
**Data:** 2026-09-01

- Poprawiono konflikt `Game` namespace/type w `MyraUIManager` przez jawny alias typu MonoGame.
- Build `RailDispatchMono.Core` został przywrócony do stanu kompilowalnego.

## [0.1.2a] — Myra UI integration foundation
**Data:** 2026-09-01

- Dodano standardową bibliotekę `Myra` w wersji `1.6.5` do `RailDispatchMono.Core`.
- Dodano `MyraUIManager` jako wspólną granicę integracji Myra.
- `MyraEnvironment.Game` jest inicjalizowane jednokrotnie podczas `RailDispatchMonoGame.LoadContent()`.
- Utworzono współdzielony Myra `Desktop`.
- Nie migrowano jeszcze istniejących ekranów; `ScreenManager` pozostaje właścicielem lifecycle ekranów i routingu wejścia.
- Aktualizowano dokumentację architektury i reguły dla AI.

## [0.1.1] — Przebudowa dokumentacji
**Data:** 2026-08-31

- Zredukowano aktywny zestaw `docs/` do 20 dokumentów autorytatywnych.
- Usunięto przestarzałe snapshoty i jednorazowe dokumenty audytowe z aktywnego zestawu.
- Usunięto redundantny `architecture.json`.
- Dodano `19-current-state-0.1.1.md` jako snapshot aktualnego stanu.
- Utrzymano szczegółową historię w `docs/changelog/` i `CHANGELOG.md`.

## [0.0.16] — Save slots, Main Menu i runtime persistence
**Data:** 2026-08-31

- Dodano katalogi zapisów z `metadata.json`.
- Save składa się z rozdzielonych plików `map.json`, `trains.json`, `schedules.json`, `passengers.json` i `economy.json`.
- Dodano wersjonowanie danych save (`schemaVersion`).
- Zapis obejmuje pociągi, składy, parametry pojazdów, pozycje, trasy wagonów, pasażerów znajdujących się w wagonach oraz `GameDay`/`GameTime`.
- Dodano Main Menu: `NOWA GRA`, `WCZYTAJ GRĘ`, `USTAWIENIA`, `O GRZE`, `WYJDŹ`.
- Nowa gra tworzy pusty slot; Load wybiera zapis bez dodatkowego potwierdzenia.
- Niepoprawny lub niekompletny zapis jest odrzucany komunikatem zamiast cichego ładowania.
- Zapis rozkładów został przeniesiony do aktywnego slotu.
- Auto-save nie jest jeszcze włączony.
- Istniejący `DepotTrainMenu` pozostaje punktem tworzenia wielu pociągów z jednego depot.
