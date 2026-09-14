# Current state — 0.3.0

**Data:** 2026-09-14

## Cel wydania

0.3.0 aktywuje pierwszą warstwę zarządzania stanem infrastruktury. `TrackCell.WearPercent`, które wcześniej było tylko przygotowanym polem danych, jest teraz napędzane przez czas symulacji i ruch pociągów.

## Zużycie infrastruktury

`GameMap.Maintenance` jest właścicielem symulacji zużycia. `InfrastructureMaintenanceManager`:

- nalicza zużycie na podstawie czasu symulacji, a nie czasu ściennego;
- zwiększa zużycie toru aktualnie używanego przez pociąg szybciej niż toru nieużywanego;
- uwzględnia `LineClass`, `TrackType` i `TractionSystem` jako współczynniki zużycia;
- utrzymuje progi `Warning` od 60% zużycia i `Critical` od 85%;
- udostępnia agregat stanu sieci i listę najbardziej zużytych odcinków;
- pozwala naprawić wszystkie odcinki albo tylko krytyczne.

Zużycie nadal jest właściwością `TrackCell`; nie powstał drugi model toru.

## Integracja z ruchem

`GameplayScreen` przekazuje do systemu utrzymania czas symulacji z `GameClock` oraz pozycje torów zajmowane w danym momencie przez pociągi. System działa przed aktualizacją ruchu w danej klatce, więc nie steruje dispatcherem, sygnałami ani zwrotnicami.

## F10

`MyraRailwayDiagnosticsView` pokazuje:

- średni stan infrastruktury;
- liczbę odcinków ostrzegawczych i krytycznych;
- czas symulacji wykorzystany przez system utrzymania;
- osiem najbardziej zużytych odcinków;
- akcje `NAPRAW KRYTYCZNE` i `NAPRAW WSZYSTKIE`.

F10 pozostaje ekranem diagnostycznym, a nie osobnym kontrolerem symulacji.

## Zapis

`WearPercent` był już częścią schema 3 `map.json` i pozostaje zapisywany oraz odczytywany bez zmiany schematu. Nie ma migracji zapisu w 0.3.0.

## Zakres poza wydaniem

0.3.0 nie dodaje jeszcze ekonomii, kosztów finansowych, dynamicznych ograniczeń prędkości, awarii infrastruktury ani automatycznego zamykania zużytych torów. Zużycie jest informacją i podstawą przyszłych decyzji.

## Weryfikacja

Nie wykonano kompilacji Windows ani live gameplay w tym środowisku. Zmiana została sprawdzona statycznie względem `GameMap`, `TrackCell`, `GameClock`, `GameplayScreen`, `MapSaveService` i F10. Lokalnie należy sprawdzić narastanie zużycia przy ruchu, zachowanie podczas pauzy, zapis/odczyt `WearPercent` oraz przyciski naprawy F10.
