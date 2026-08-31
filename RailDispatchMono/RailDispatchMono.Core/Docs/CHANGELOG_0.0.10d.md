# [0.0.10d] — Wielopolowe stacje

**Data:** 2026-08-31

## Station Areas

- Zmieniono model `Station` z pojedynczego pola mapy na prostokątny obszar.
- Dodano `Width` i `Height` oraz enumerację wszystkich komórek należących do stacji.
- Zachowano `Position` jako punkt kotwiczący kompatybilny z istniejącym kodem.
- Dodano `Contains()` i `GetCenterCell()` do pracy z obszarem stacji.

## Station Controller

- `GetStationAt()` wykrywa stację na dowolnym polu jej obszaru.
- Dodano `GetStationsAt()`.
- Dodano ochronę przed nakładaniem się obszarów stacji.
- Wykrywanie następnej stacji uwzględnia cały jej obszar, a nie tylko pojedynczą pozycję.
- Odległość hamowania jest liczona do najbliższego pola stacji.

## Train / Signals

- Nie dodano `StationStopPoint`.
- Semafory pozostają odpowiedzialne za określanie miejsca zatrzymania pociągu.
- Obszar stacji służy do określenia, gdzie dostępna jest obsługa pasażerska.

## Rendering

- `StationRenderer` renderuje cały obszar stacji zamiast pojedynczego znacznika.
- Dodano podgląd obszaru stacji z zachowaniem środka kursora.
- Wypełnienie obszaru jest celowo półprzezroczyste, aby nie zasłaniać torów, semaforów i pociągów.

## Compatibility

- Istniejący konstruktor `Station(name, position)` nadal tworzy stację 1×1.
- Możliwe jest tworzenie stacji 3×3 przez `new Station(name, position, 3, 3)`.
- Istniejące stacje 1×1 nie wymagają migracji.

## Known Limitation

- Interfejs budowania nadal tworzy stacje 1×1; wybór rozmiaru stacji z poziomu UI będzie osobnym krokiem.
