# Changelog

## [Unreleased] — 0.0.4 (w trakcie)

### Railway / Track Building

- Dodano model geometrii torów: `TrackGeometry` (Straight, Curve).
- Dodano obsługę kierunków zakrętów: `CurveDirection` (NorthEast, EastSouth, SouthWest, WestNorth).
- Dodano flagowy model połączeń toru: `TrackConnections` (North, East, South, West).
- Dodano tryby budowania torów: `TrackBuildMode` (Straight, Curve).
- Dodano `TrackCell` jako reprezentację pojedynczego elementu toru na mapie.
- Dodano `TrackBuilder` odpowiedzialny za budowanie i usuwanie pojedynczych elementów toru.
- Dodano tory proste w orientacji poziomej i pionowej.
- Dodano cztery orientacje zakrętów: NorthEast, EastSouth, SouthWest, WestNorth.
- Dodano automatyczne logiczne łączenie nowo postawionego toru z istniejącymi sąsiadami (`ConnectNeighbours`).
- Dodano możliwość usuwania pojedynczego elementu toru prawym przyciskiem myszy.
- Budowanie torów odbywa się przez stawianie pojedynczych elementów kliknięciem (nie przeciąganiem).
- Dodano wybór typu toru z poziomu sterowania: `1` = tor prosty, `2` = zakręt.
- Dodano zmianę orientacji toru prostego: `H` = poziomy, `V` = pionowy.
- Dodano obracanie zakrętu klawiszem `R`.
- Dodano panel narzędzia na mapie pokazujący aktualnie wybrany typ i orientację toru.
- Dodano renderowanie torów prostych w `MapControl` (`RenderStraight`).
- Dodano renderowanie zakrętów krzywymi Béziera, z końcami zgodnymi z punktami połączeń sąsiednich pól (`RenderCurve`).

### Map

- Dodano `GameMap` z siatką terenu i limitem rozmiaru 16384 × 16384 (`MapSize`).
- Dodano `MapPosition`, `MapCell`, `TerrainType` (Grass, Forest, Hill, Mountain).
- Dodano `MapRenderer` renderujący widoczny fragment mapy zależnie od kamery i zoomu.
- Dodano sterowanie kamerą: przesuwanie środkowym przyciskiem myszy, zoom kółkiem myszy (zakres 2–40, z zachowaniem punktu pod kursorem).

### Build

- `RailDispatch.Domain` — build OK.
- `RailDispatch.Infrastructure` — build OK (pusty, gotowy na przyszłą implementację).
- `RailDispatch.Simulation` — build OK (pusty, gotowy na przyszłą implementację).
- `RailDispatch.UI` — build OK.
- `RailDispatch.App` — build OK.
- `RailDispatch.Tests` — build OK (testy dla `GameMap`: teren, granice mapy, limit rozmiaru).

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

- Proceduralne generowanie terenu — jeszcze nie zaimplementowane (teren ustawiany ręcznie przez `SetTerrain`).
- Rozjazdy (Junction) — brak.
- Semafory (Signal) — brak.
- Sekcje blokowe (BlockSection) — brak.
- Interlocking — brak.
- Wszystko z zakresu 0.1.0+ (pociągi, ruch, tabor, planowanie, pasażerowie) — nierozpoczęte.

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
