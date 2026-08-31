# System pasażerów — 0.0.10

## Cel

Wersja 0.0.10 wprowadza quasi-indywidualny model pasażera oraz obsługę pasażerów na poziomie pojedynczego wagonu. Model jest celowo przygotowany pod przyszłe sprzęganie i rozprzęganie składów.

## Model pasażera

`Passenger` posiada własną tożsamość (`Guid`) oraz:

- `OriginStation` — stacja początkowa,
- `DestinationStation` — stacja docelowa,
- `State` — `WaitingAtStation`, `OnBoard` albo `Arrived`,
- `CurrentTrainId` — identyfikator pociągu, jeśli pasażer jest w składzie.

Pasażer nie jest agregowany wyłącznie jako licznik. Każdy pasażer zachowuje własny cel podróży.

## Wagon

`Wagon` posiada:

- `WagonType`,
- `PassengerCapacity`,
- listę pasażerów,
- `ServiceRoute` — opcjonalną listę stacji obsługiwanych przez wagon.

Pusta `ServiceRoute` oznacza brak ograniczenia trasy. Dzięki temu pierwsza wersja może używać dowolnego wagonu pasażerskiego, a przyszła implementacja może przypisać różne trasy do poszczególnych wagonów.

Przykład przyszłego składu:

- wagon 1: A-B-C,
- wagon 2: A-B-C,
- wagon 3: A-B-D,
- wagon 4: F-B-C.

Nie wymaga to zmiany modelu pasażera.

## Obsługa pasażera w wagonie

Pasażer może wejść do wagonu, jeżeli:

1. wagon jest typu `Passenger`,
2. wagon ma wolne miejsce,
3. `ServiceRoute` jest pusta albo zawiera stację docelową pasażera.

Pasażer wysiada tylko z wagonu, który aktualnie go przewozi, i tylko na swojej stacji docelowej.

## Przesiadki

Przesiadki nie są jeszcze automatycznie realizowane.

Jeżeli w przyszłości wagon lub skład nie będzie mógł kontynuować przejazdu do celu, pasażer może zostać ustawiony z powrotem w stanie `WaitingAtStation`. To jest przewidziane przez model stanu i nie wymaga zmiany tożsamości pasażera.

## Stacje

`Station` jest obiektem domenowym zakotwiczonym w `MapPosition`.

Parametry:

- `Name`,
- `Position`,
- `StopRadius`,
- `DwellTimeSeconds`,
- `PassengerServiceEnabled`.

## Zatrzymywanie pociągów

`StationController` działa poza klasą `Train`.

Mechanizm:

1. wyszukuje najbliższą stację przed pociągiem,
2. wykorzystuje bieżące hamowanie pociągu do obliczenia bezpiecznej prędkości,
3. ogranicza prędkość przed stacją,
4. po wejściu w obszar stacji i zejściu do prędkości postojowej rozpoczyna postój,
5. wykonuje wysiadanie,
6. wykonuje wsiadanie,
7. utrzymuje postój przez `DwellTimeSeconds`,
8. po zakończeniu postoju pozwala pociągowi kontynuować jazdę.

Obecny look-ahead stacji jest oparty o istniejącą siatkę mapy. Docelowo powinien zostać zastąpiony przez rzeczywistą odległość wynikającą z `TrackRoute` i planowanej trasy.

## Integracja z TrainManager

`TrainManager` automatycznie tworzy `StationController`.

Przed aktualizacją pociągu kontroler może wstrzymać aktualizację podczas postoju. Po aktualizacji pociągu kontroler sprawdza hamowanie i obsługę stacji.

## Cel architektoniczny

Model został przygotowany tak, aby późniejsze:

- coupling,
- decoupling,
- różne trasy wagonów,
- częściowe odłączanie wagonów,
- transfery pasażerów,
- planowanie tras,

nie wymagały przebudowania klasy `Passenger`.
