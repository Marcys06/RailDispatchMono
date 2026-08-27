
# Changelog

## [Unreleased] — 0.0.4 (w trakcie)

### Railway / Track Building

- Dodano model geometrii torów: `TrackGeometry` (`Straight`, `Curve`).
- Dodano obsługę kierunków zakrętów: `CurveDirection` (`NorthEast`, `EastSouth`, `SouthWest`, `WestNorth`).
- Dodano flagowy model połączeń toru: `TrackConnections` (`North`, `East`, `South`, `West`).
- Dodano tryby budowania torów: `TrackBuildMode` (`Straight`, `Curve`).
- Dodano `TrackCell` jako reprezentację pojedynczego elementu toru na mapie.
- Dodano `TrackBuilder` odpowiedzialny za budowanie i usuwanie pojedynczych elementów toru.
- Dodano tory proste w orientacji poziomej i pionowej.
- Dodano cztery orientacje zakrętów: `NorthEast`, `EastSouth`, `SouthWest`, `WestNorth`.
- Dodano automatyczne logiczne łączenie nowo postawionego toru z istniejącymi sąsiadami (`ConnectNeighbours`).
- Dodano możliwość usuwania pojedynczego elementu toru prawym przyciskiem myszy.
- Budowanie torów odbywa się przez stawianie pojedynczych elementów kliknięciem.
- Dodano wybór typu toru z poziomu sterowania: `1` = tor prosty, `2` = zakręt.
- Dodano zmianę orientacji toru prostego: `H` = poziomy, `V` = pionowy.
- Dodano obracanie zakrętu klawiszem `R`.
- Dodano panel narzędzia na mapie pokazujący aktualnie wybrany typ i orientację toru.
- Dodano renderowanie torów prostych.
- Dodano renderowanie zakrętów krzywymi Béziera, z końcami zgodnymi z punktami połączeń sąsiednich pól.
- Dodano podgląd pola i planowanego toru przed jego postawieniem.

### Map

- Dodano `GameMap` z siatką terenu i limitem rozmiaru 16384 × 16384 (`MapSize`).
- Dodano `MapPosition`, `MapCell`, `TerrainType` (`Grass`, `Forest`, `Hill`, `Mountain`).
- Dodano `MapRenderer` renderujący widoczny fragment mapy zależnie od kamery i zoomu.
- Dodano sterowanie kamerą: przesuwanie środkowym przyciskiem myszy oraz zoom kółkiem myszy.

### Train System

- Dodano model pojazdu bazowego `Vehicle`.
- Dodano model parametrów technicznych pojazdu `VehicleParameters`.
- Dodano `Locomotive` jako niezależny typ pojazdu.
- Dodano `Wagon` jako niezależny typ pojazdu.
- Dodano `LocomotiveType` dla różnych typów lokomotyw.
- Dodano `TrainComposition` przechowujący uporządkowaną listę pojazdów.
- Dodano obliczanie długości składu na podstawie długości pojazdów.
- Dodano sprawdzanie zdolności składu do ruchu na podstawie obecności lokomotywy.
- Dodano `Train` jako reprezentację składu posiadającego własną tożsamość.
- Dodano `TrainManager` zarządzający aktywnymi pociągami.
- Dodano `TrainRenderer` renderujący lokomotywy i wagony jako oddzielne pojazdy.
- Dodano testowy skład składający się z lokomotywy i dwóch wagonów.
- Dodano powiązanie pociągu z `GameMap`.
- Dodano podstawową walidację możliwości ruchu pociągu względem toru.
- Pociąg stojący na polu bez toru pozostaje nieruchomy.
- Dodano pierwszy etap ruchu pociągu po rzeczywistym torze.
- Pociąg może rozpocząć ruch na wschód, jeżeli zajmowane pole posiada połączenie `East`.

### Build

- `RailDispatchMono.Core` — build OK po dodaniu systemu pociągów.
- `RailDispatchMono.DesktopGL` — build OK.
- Dodano podstawowy przepływ aktualizacji: `GameplayScreen` → `TrainManager` → `Train`.

### Controls

- `1` — wybór toru prostego.
- `2` — wybór zakrętu.
- `H` — orientacja pozioma.
- `V` — orientacja pionowa.
- `R` — obrót zakrętu.
- LPM — postaw jeden element toru.
- PPM — usuń jeden element toru.
- MMB — przesuwanie kamery.
- Kółko myszy — zoom mapy.

### Known gaps / Poza zakresem tego wpisu

- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów.
- Ruch pociągu nie obsługuje jeszcze pełnego przebiegu po sieci torowej.
- Ruch po zakrętach nie jest jeszcze powiązany z geometrią toru.
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru.
- Brak obsługi kierunku jazdy i zmiany kierunku.
- Brak rozjazdów (`Junction`).
- Brak semaforów (`Signal`).
- Brak sekcji blokowych (`BlockSection`).
- Brak interlockingu.
- Brak pełnej fizyki ruchu pociągu.
- Brak wykolejenia.
- Brak manewrów i sprzęgania w warstwie symulacji.
- Brak planowania jazdy.
- Brak pasażerów.
- Brak harmonogramów.
- Brak proceduralnego generowania terenu.

---

## [0.0.2] — Mapa

### Railway / Map

- Dodano `GameMap`, `MapCell`, `MapPosition`, `MapSize`.
- Dodano typy terenu: `TerrainType`.
- Dodano podstawowe renderowanie mapy z kamerą.
- Zweryfikowano kompilację całego rozwiązania.

---

## [0.0.0] — Fundament projektu

- Utworzono repozytorium GitHub.
- Skonfigurowano projekt w Visual Studio (.NET, bez zewnętrznego silnika gier).
- Utworzono strukturę rozwiązania: `RailDispatch.Domain`, `RailDispatch.Infrastructure`, `RailDispatch.Simulation`, `RailDispatch.UI`, `RailDispatch.App`, `RailDispatch.Tests`.
- Dodano dokumentację: `VISION.md`, `GAMEPLAY.md`, `RAILWAY.md`, `TRAIN_SYSTEM.md`, `SCHEDULES.md`, `PASSENGERS.md`, `MAP.md`, `UI.md`, `ARCHITECTURE.md`, `DATA_MODEL.md`, `TECHNICAL.md`, `DEVELOPMENT.md`, `ROADMAP.md`.
```
