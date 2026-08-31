# [0.0.11] — Przebudowa systemu stacji i pasażerów
**Data:** 2026-08-31

## Station / Passenger Architecture
- Rozdzielono decyzję o postoju pociągu od obsługi pasażerów.
- Dodano `ITrainStopDecision` — abstrakcję odpowiedzialną wyłącznie za decyzję, czy pociąg powinien zatrzymać się na stacji.
- Dodano `DefaultTrainStopDecision` — obecnie każda aktywna stacja pasażerska jest przystankiem.
- Dodano `IPassengerService` — niezależną abstrakcję obsługi wymiany pasażerów.
- Dodano `DefaultPassengerService` — wysiadanie przed wsiadaniem.
- Pozostawiono semafory jako element definiujący fizyczny punkt zatrzymania; stacja nie posiada `StopPoint`.

## Passenger Model
- Rozszerzono `Passenger` o `CurrentStationId`.
- Zachowano model quasi-indywidualny: każdy pasażer posiada własną stację początkową i końcową.
- Podczas oczekiwania pasażer ma `CurrentStationId` ustawione na stację.
- Po wejściu do pociągu `CurrentStationId` jest zerowane, a `CurrentTrainId` wskazuje pociąg.
- Po dotarciu do celu pasażer otrzymuje stan `Arrived` i ponownie wskazuje stację docelową jako `CurrentStationId`.
- Przygotowano model pod przyszłe przesiadki bez implementowania ich w tej wersji.

## Per-Wagon Passenger Handling
- Każdy wagon posiada własną listę pasażerów.
- Każdy wagon samodzielnie sprawdza pojemność i możliwość przyjęcia pasażera.
- Zachowano `WagonType` jako podstawę przyszłego rozróżniania wagonów.
- Dodano `PassengerCount` i `AvailablePassengerCapacity`.
- Domyślna pojemność wagonu pasażerskiego wynosi 80 miejsc.
- `ServiceRoute` nadal pozwala przygotować poszczególne wagony do późniejszego modelu A-B-C / A-B-D.

## Automatic Passenger Generation
- Stacje automatycznie generują pasażerów.
- Domyślny generator losuje stację docelową spośród pozostałych stacji.
- Dodano `IPassengerDemandProvider`, dzięki czemu obecny generator losowy można w przyszłości zastąpić modelem miasta/populacji bez przebudowy `StationController`.
- Dodano `RandomPassengerDemandProvider`.
- Generowanie jest wykonywane okresowo przez `StationController.Update()`.
- Dodano parametry stacji:
  - `PassengerGenerationEnabled`
  - `PassengerGenerationIntervalSeconds`
  - `PassengerGenerationBatchSize`
  - `PassengerWaitingCapacity`
- Domyślnie stacja generuje do 2 pasażerów co 10 sekund, z limitem 100 oczekujących.
- Generowanie rozpoczyna się dopiero wtedy, gdy istnieje co najmniej jedna możliwa stacja docelowa.

## Station Controller
- `StationController` przejął rolę koordynatora cyklu życia stacji, ale nie zawiera już bezpośrednio logiki obsługi pasażerów.
- Dwell time nadal działa niezależnie od wymiany pasażerów.
- Wymiana pasażerów odbywa się przy rozpoczęciu postoju.
- Zachowano obsługę stacji wielokomórkowych.
- Zachowano brak `StopPoint` — fizyczne zatrzymanie pozostaje związane z istniejącym systemem semaforów.

## Train Manager
- `TrainManager.Update()` aktualizuje teraz system generowania pasażerów przed obsługą ruchu pociągów.
- Cykl generowania pasażerów jest niezależny od liczby pociągów.

## UI
- Tooltip pociągu został rozszerzony o dane konkretnego pojazdu w składzie.
- Po najechaniu na wagon wyświetlane są dodatkowo:
  - numer wagonu w składzie,
  - typ wagonu,
  - liczba pasażerów,
  - pojemność wagonu,
  - liczba wolnych miejsc,
  - grupy pasażerów według stacji docelowej,
  - liczba przystanków w `ServiceRoute`, jeśli została skonfigurowana.
- Tooltip nadal działa osobno dla każdego pojazdu, co stanowi podstawę pod przyszły coupling/decoupling.

## Bug Fixes
- Poprawiono zliczanie wysiadających pasażerów — wynik jest teraz liczbą pasażerów obsłużonych podczas konkretnego postoju, a nie sumą wszystkich zakończonych podróży.
- Kolejka oczekujących pasażerów jest oparta na `CurrentStationId`, a nie wyłącznie na stacji początkowej.

## Przygotowanie pod przyszłość
- Przyszły model miast może zostać podłączony przez `IPassengerDemandProvider`.
- Przyszła logika rozkładów jazdy może zastąpić `DefaultTrainStopDecision` bez przebudowy systemu pasażerów.
- Przesiadki mogą zostać dodane poprzez rozszerzenie `PassengerState` i obsługi `IPassengerService`.
- Coupling/decoupling może wykorzystywać istniejący podział pasażerów na konkretne wagony oraz `ServiceRoute`.
- Rozdzielenie decyzji postoju i obsługi pasażerów przygotowuje system pod rozkłady jazdy planowane na `0.0.12`.

## Build
- Zmiany zostały wprowadzone w `RailDispatchMono.Core`.
- Pełny build powinien zostać wykonany lokalnie po pobraniu aktualnego `master`.
