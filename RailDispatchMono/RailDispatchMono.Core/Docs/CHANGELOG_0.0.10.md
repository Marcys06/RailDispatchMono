# [0.0.10] — Stacje i pasażerowie

**Data:** 2026-08-31

## Station System

- Dodano model `Station` zakotwiczony w `MapPosition`.
- Dodano nazwę stacji, promień zatrzymania, czas postoju i przełącznik obsługi pasażerów.
- Dodano `StationController` odpowiedzialny za wyszukiwanie stacji przed pociągiem, hamowanie, postój i obsługę pasażerską.
- Dodano automatyczne utrzymywanie pociągu w stanie postoju podczas `DwellTimeSeconds`.
- Po postoju pociąg wraca do normalnej aktualizacji ruchu.
- `TrainManager` automatycznie tworzy i wykorzystuje `StationController`.

## Passenger System

- Dodano quasi-indywidualny model `Passenger`.
- Każdy pasażer posiada własną stację początkową i docelową.
- Dodano stany `WaitingAtStation`, `OnBoard`, `Arrived`.
- Dodano `CurrentTrainId` do śledzenia pociągu, w którym pasażer aktualnie podróżuje.
- Dodano `PassengerManager` do tworzenia, wyszukiwania i obsługi pasażerów.

## Wagon Passenger Handling

- Dodano `WagonType`.
- Wagon posiada własną listę pasażerów.
- Każdy wagon posiada niezależną pojemność pasażerską.
- Wsiadanie odbywa się wagon po wagonie.
- Wysiadanie odbywa się wagon po wagonie.
- Dodano opcjonalny `ServiceRoute` do przyszłego ograniczania trasy konkretnego wagonu.
- Pusta `ServiceRoute` oznacza wagon bez ograniczenia trasy.

## Coupling / Decoupling Preparation

Model wagonu został przygotowany pod składy, których wagony mogą obsługiwać różne trasy, np.:

- A-B-C,
- A-B-D,
- F-B-C.

Nie dodano jeszcze mechaniki sprzęgania ani rozprzęgania. `ServiceRoute` jest przygotowaniem modelu danych pod tę funkcjonalność.

## Transfers

- Automatyczne przesiadki nie są jeszcze implementowane.
- Model pasażera zachowuje stan `WaitingAtStation`, dzięki czemu przyszły system przesiadek może ponownie włączyć pasażera do obsługi stacji.

## Technical Notes

- Obsługa stacji została umieszczona poza `Train`, aby nie mieszać polityki postoju i pasażerów z geometrią ruchu.
- Obecne wyszukiwanie następnej stacji wykorzystuje siatkę mapy.
- Docelowo wyszukiwanie stacji powinno korzystać z autorytatywnej `TrackRoute`.

## Known Limitations

- Brak edytora/stawiania stacji w UI.
- Brak renderera stacji.
- Brak harmonogramów postojów.
- Brak planowania trasy pasażera.
- Brak automatycznych przesiadek.
- Brak coupling/decoupling.
- `ServiceRoute` nie jest jeszcze generowana automatycznie z planowanej trasy pociągu.
- Look-ahead stacji jest obecnie przybliżeniem gridowym, a nie pełnym pomiarem długości trasy.
