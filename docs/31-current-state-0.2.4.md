# Current state — 0.2.4

## Infrastruktura torowa

0.2.4 rozdziela geometrię toru od jego parametrów infrastrukturalnych.

- `TrackGeometry` opisuje kształt: prosty, łuk, rozjazd.
- `TrackType` opisuje rolę eksploatacyjną: `Mainline`, `Secondary`, `Siding`, `Platform`.
- `LineClass` opisuje klasę linii: `Local`, `Regional`, `Mainline`, `Magistral`.
- `TractionSystem` opisuje elektryfikację: `None`, `DC`, `AC`.
- `TrackCell.WearPercent` jest przygotowany pod przyszłe zużycie infrastruktury, ale nie jest jeszcze symulowany.

Parametry klasy linii udostępniają przygotowane limity infrastrukturalne (Vmax i nacisk osi). Nie są jeszcze nadrzędną warstwą fizyki; zastosowanie ich do ruchu należy do późniejszego rozwoju fizyki.

## Trakcja taboru

`TractionType` pozostaje rozróżnieniem napędu `Electric` / `Diesel`, a `TractionSystem` opisuje konkretny system elektryczny.

- EP07 obsługuje DC;
- EU200 obsługuje AC;
- lokomotywa dieslowska może pracować niezależnie od elektryfikacji toru;
- model lokomotywy obsługuje wiele systemów elektrycznych, więc lokomotywy wielosystemowe są możliwe bez zmiany architektury.

W 0.2.4 kompatybilność jest właściwością modelu taboru/infrastruktury. System nie tworzy automatycznie objazdu ani nie zmienia lokomotywy, gdy infrastruktura nie pasuje. Nie dodano automatycznego komunikatu „BRAK TRAKCJI / TRASA NIEWYKONALNA” na poziomie planowania trasy.

## Edycja infrastruktury — F8

F8 otwiera edytor infrastruktury dla toru znajdującego się pod kursorem.

Można zmienić:

- rodzaj toru;
- klasę linii;
- trakcję `BRAK / DC / AC`.

Edycja nie zmienia geometrii, połączeń, bloków, sygnałów ani zwrotnic. Jest to warstwa infrastrukturalna nad istniejącą siecią ruchową.

## Budowanie toru

`TrackBuilder` przechowuje aktualnie wybrane parametry infrastruktury. Nowo budowany tor otrzymuje je automatycznie. Istniejący tor można przekonfigurować bez przebudowy geometrii przez `ConfigureInfrastructure` oraz wyspecjalizowane metody ustawiające typ, klasę i trakcję.

## Bloki i sygnały

Bloki nadal pozostają warstwą zajętości/ruchu. Parametry infrastruktury należą do `TrackCell`, czyli do segmentu toru. Nie wprowadzono nowego systemu bloków ani równoległego systemu rezerwacji tras.

Sygnały i zwrotnice zachowują dotychczasową własność gracza. Dispatcher nie zaczyna nimi sterować.

## Zapisy gry

`map.json` przechodzi na schemat `2` i zapisuje dla każdego segmentu:

- typ toru;
- klasę linii;
- trakcję;
- zużycie;
- dotychczasową geometrię, połączenia i stan zwrotnicy.

Zgodnie z polityką projektu stare zapisy nie muszą być kompatybilne z nową linią rozwojową.

## Przygotowanie pod roadmapę

### 0.3.0 — Zarządzanie infrastrukturą

Model 0.2.4 przygotowuje klasy linii, elektryfikację i zużycie. W 0.3.0 należy dodać rzeczywistą eksploatację: koszty utrzymania, zużywanie się infrastruktury, modernizacje i ograniczenia wynikające ze stanu.

### 0.4.0 — Ekonomia

Warstwa ekonomiczna ma pozostać oddzielona od modelu fizycznego i ruchowego. 0.2.4 nie dodaje pieniędzy ani kosztów do rozgrywki.

### 0.5.0 — Rozkład jazdy

Obecny rozkład lokomotyw pozostaje operacyjnym szkieletem. Kolejny etap rozszerza go o publiczny rozkład, koordynację i planowanie.

### 0.6.0 — Symulacja pasażerska

Obecny właściciel pasażera pozostaje `Wagon`. Przyszły popyt i wybór środka transportu powinny zostać dołożone bez przenoszenia własności pasażerów do UI lub pociągu.

### 0.7.0 — Fizyka

Klasy linii i parametry infrastruktury są gotowe jako dane wejściowe dla limitów prędkości, łuków, gradientów i hamowania odzyskowego. Model ruchu pozostaje obecnie bez przebudowy.

### 0.8.0 — Kryzysy

Awaria infrastruktury, pogoda i zarządzanie kryzysowe powinny korzystać z przygotowanego `WearPercent`/`ConditionPercent`, zamiast tworzyć osobny równoległy model torów.

### 0.9.0 — Sieć

Segmentowe parametry infrastruktury i istniejąca topologia pozostają podstawą przyszłego łączenia miast i stacji węzłowych.

### 1.0.0 — Full release

Pełne wydanie ma integrować powyższe warstwy, ale bez odbierania graczowi decyzji operacyjnych.

## Profil rozgrywki

Przyjęto wariant B: przystępna symulacja. Realizm ma wynikać z czytelnych zależności i konsekwencji decyzji, a nie z maksymalnej liczby parametrów wymaganych do ręcznego obsługiwania.

## Weryfikacja

Nie wykonano tutaj kompilacji Windows ani testu live gameplay. Po pobraniu zmian należy zweryfikować co najmniej: budowanie toru z wybraną trakcją, F8, zmianę infrastruktury istniejącego toru, zapis/odczyt mapy, katalog EP07/EU200/SU42 oraz brak zmian w działaniu bloków, sygnałów, zwrotnic i F6.
