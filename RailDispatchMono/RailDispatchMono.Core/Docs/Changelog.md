Changelog

[0.0.5] — System semaforów i rozjazdów
Data: 2026-08-29

Railway / Track Building
- Dodano tryby budowania: 3 = rozjazd (Junction), 4 = semafor (Signal).
- Rozszerzono TrackBuildMode o Junction i Signal.
- Dodano TrackCell jako reprezentację pojedynczego elementu toru na mapie.
- Dodano automatyczne logiczne łączenie nowo postawionego toru z istniejącymi sąsiadami.

Junctions (Rozjazdy)
- Dodano rozjazdy jako element toru z możliwością przełączania kierunku.
- Dodano JunctionType definiujący 8 typów rozjazdów.
- Dodano przełączanie rozjazdu między prostym a odchylonym torem (SwitchPosition: Straight, Diverging).
- Dodano interakcję z rozjazdami: kliknięcie prawym przyciskiem myszy otwiera radialne menu wyboru typu rozjazdu.
- Dodano możliwość przełączania rozjazdu klawiszem J.
- Rozjazdy renderowane z aktywną iglicą w kolorze pomarańczowym (skręt) lub zielonym (prosto).
- Dodano JunctionRadialMenu jako okrągłe menu do wyboru typu rozjazdu.

Signals (Sema)
- Dodano pełny system semaforów kolejowych z 10 aspektami:
  - S1a (Stop) - Stój, przejazd zabroniony
  - S1b (StopStation) - Stój (stacja), przejazd zabroniony
  - S2 (Clear) - Jazda z Vmax, droga wolna
  - S5 (Warning) - Ostrzeżenie, następny semafor stój
  - S6 (Speed100) - Jazda ≤ 100 km/h
  - S10 (Speed40) - Jazda ≤ 40 km/h
  - S12-S15 (Reserve1-4) - Rezerwy
- Dodano SignalController do zarządzania semaforami na mapie.
- Dodano SignalRenderer do rysowania semaforów z kolorami zależnymi od aspektu.
- Dodano SignalRadialMenu - okrągłe menu do wyboru aspektu semafora (po kliknięciu prawym przyciskiem).
- Dodano SignalDirectionMenu - menu wyboru kierunku przy stawianiu semafora (gdy tor ma więcej niż jedno połączenie).
- Dodano SignalSelectionMenu - menu wyboru semafora gdy na jednej pozycji jest ich więcej.
- Dodano obsługę klawisza J do szybkiego przełączania semafora między Stop a Clear.
- Stawianie semafora w trybie Signal (klawisz 4) na istniejącym torze.
- Semafor przechowuje dostępne aspekty (AvailableAspects) dla danego egzemplarza.
- Dodano możliwość rozszerzania aspektów przez modyfikację SignalAspects.cs.

UI / Input
- Dodano InputManager jako centralną obsługę wejścia (mysz, klawiatura).
- Dodano obsługę trybów budowania za pomocą klawiszy numerycznych (1-4).
- Dodano radialne menu dla rozjazdów (JunctionRadialMenu) i semaforów (SignalRadialMenu).
- Dodano czcionkę Arial24 dla menu.

Controls
- 1 — tor prosty
- 2 — zakręt
- 3 — rozjazd
- 4 — semafor
- H — orientacja pozioma (tor prosty)
- V — orientacja pionowa (tor prosty)
- R — obrót zakrętu / zmiana typu rozjazdu
- J — przełącz rozjazd / przełącz semafor (Stop ↔ Clear)
- LPM — postaw element toru / semafor
- PPM — usuń element toru / otwórz menu rozjazdu lub semafora
- MMB — przesuwanie kamery
- Kółko myszy — zoom mapy
- Escape — zamknij menu radialne

Build
- RailDispatchMono.Core — build OK.
- RailDispatchMono.DesktopGL — build OK.
- Dodano wszystkie nowe klasy semaforów i rozjazdów.

Known gaps / Poza zakresem tego wpisu
- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów.
- Ruch pociągu nie obsługuje jeszcze pełnego przebiegu po sieci torowej.
- Ruch po zakrętach nie jest jeszcze powiązany z geometrią toru.
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru.
- Brak obsługi kierunku jazdy i zmiany kierunku.
- Brak sekcji blokowych (BlockSection).
- Brak interlockingu.
- Brak pełnej fizyki ruchu pociągu.
- Brak wykolejenia.
- Brak manewrów i sprzęgania w warstwie symulacji.
- Brak planowania jazdy.
- Brak pasażerów.
- Brak harmonogramów.
- Brak proceduralnego generowania terenu.

[0.0.4] — Podstawowy system torów i pociągów
Data: 2026-08-20

Railway / Track Building
- Dodano model geometrii torów: TrackGeometry (Straight, Curve).
- Dodano obsługę kierunków zakrętów: CurveDirection (NorthEast, EastSouth, SouthWest, WestNorth).
- Dodano flagowy model połączeń toru: TrackConnections (North, East, South, West).
- Dodano tryby budowania torów: TrackBuildMode (Straight, Curve).
- Dodano TrackCell jako reprezentację pojedynczego elementu toru na mapie.
- Dodano TrackBuilder odpowiedzialny za budowanie i usuwanie pojedynczych elementów toru.
- Dodano tory proste w orientacji poziomej i pionowej.
- Dodano cztery orientacje zakrętów.
- Dodano automatyczne logiczne łączenie nowo postawionego toru z istniejącymi sąsiadami.
- Dodano możliwość usuwania pojedynczego elementu toru prawym przyciskiem myszy.
- Dodano renderowanie torów prostych i zakrętów.
- Dodano podgląd pola i planowanego toru przed jego postawieniem.

Train System
- Dodano model pojazdu bazowego Vehicle.
- Dodano model parametrów technicznych pojazdu VehicleParameters.
- Dodano Locomotive jako niezależny typ pojazdu.
- Dodano Wagon jako niezależny typ pojazdu.
- Dodano LocomotiveType dla różnych typów lokomotyw.
- Dodano TrainComposition przechowujący uporządkowaną listę pojazdów.
- Dodano Train jako reprezentację składu posiadającego własną tożsamość.
- Dodano TrainManager zarządzający aktywnymi pociągami.
- Dodano TrainRenderer renderujący lokomotywy i wagony jako oddzielne pojazdy.
- Dodano testowy skład składający się z lokomotywy i dwóch wagonów.
- Dodano podstawową walidację możliwości ruchu pociągu względem toru.
- Dodano pierwszy etap ruchu pociągu po rzeczywistym torze.

[0.0.3] — Mapa i kamera
Data: 2026-08-15

- Dodano GameMap z siatką terenu.
- Dodano MapPosition, MapCell, TerrainType.
- Dodano MapRenderer renderujący widoczny fragment mapy.
- Dodano sterowanie kamerą: przesuwanie środkowym przyciskiem myszy oraz zoom kółkiem myszy.

[0.0.2] — Podstawowa infrastruktura
Data: 2026-08-10

- Utworzono repozytorium GitHub.
- Skonfigurowano projekt w Visual Studio (.NET 9.0, MonoGame).
- Utworzono strukturę rozwiązania.

[0.0.1] — Dokumentacja
Data: 2026-08-05

- Dodano dokumentację: VISION.md, GAMEPLAY.md, RAILWAY.md, TRAIN_SYSTEM.md, SCHEDULES.md, PASSENGERS.md, MAP.md, UI.md, ARCHITECTURE.md, DATA_MODEL.md, TECHNICAL.md, DEVELOPMENT.md, ROADMAP.md.

[0.0.0] — Fundament projektu
Data: 2026-08-01

- Inicjalizacja repozytorium.
- Przygotowanie struktury projektu.