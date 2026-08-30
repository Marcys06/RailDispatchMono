Changelog

[0.0.8b] — System pauzy - naprawa wyświetlania menu
Data: 2026-08-30

Pause System / Bug Fixes

- Naprawiono wyświetlanie opcji menu pauzy (Resume, Quit)
- Dodano logi debugowania w MenuScreen.Draw() i UpdateMenuEntryLocations()
- Poprawiono wywołanie base.Draw() w PauseScreen
- Zaktualizowano MenuScreen o szczegółowe logi diagnostyczne

Screen Management

- Dodano właściwość InputState w ScreenManager (publiczny dostęp)
- Poprawiono obsługę wejścia dla menu pauzy
- Zaktualizowano GameplayScreen o korzystanie z InputState

Input System

- Dodano metodę IsPauseKeyJustPressed() w InputState
- Zintegrowano InputState z GameplayScreen.Update()

Build

- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK
- Usunięto projekty Android i iOS z rozwiązania (tylko DesktopGL)

Controls

- Escape / P — włącz / wyłącz pauzę (Pause Menu)
- Strzałki GÓRA / DÓŁ (lub W / S) — nawigacja po pozycjach menu pauzy
- Enter / Space — zatwierdzenie wybranej opcji w menu

Known gaps / Poza zakresem tego wpisu

- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów
- Ruch pociągu nie obsługuje jeszcze pełnego przebiegu po sieci torowej
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru
- Brak obsługi kierunku jazdy i zmiany kierunku na rozjazdach
- Brak interlockingu
- Brak wykolejenia
- Brak manewrów i sprzęgania w warstwie symulacji
- Brak planowania jazdy
- Brak pasażerów
- Brak harmonogramów


[0.0.8a] — System pauzy i wsparcie menu ekranowego
Data: 2026-08-30

Game Loop / Pause System

Dodano pelny ekran pauzy (PauseScreen.cs) wstrzymujacy aktualizacje symulacji

Zaktualizowano GameplayScreen o obsluge stanu pauzy i nakladki menu

Zoptymalizowano petle gry w RailDispatchMonoGame.cs pod katem przelaczania ekranów

UI / Screen Management & Fonts

Dodano czcionki Hud.spritefont oraz zaktualizowano Arial24.spritefont

Rozbudowano MenuScreen.cs oraz MenuEntry.cs do obslugi dynamicznych pozycji menu

Poprawiono zarzadzanie i rysowanie elementow interfejsu uzytkownika w PauseScreen

Dodano obsluge nawigacji po menu w PauseScreen (wznowienie gry, opcje, powrot do menu)

Input Manager

Zaktualizowano InputManager.cs o obsluge wejscia dla ekranu pauzy i nawigacji menu

Dodano wykrywanie klawiszy aktywacji pauzy oraz poruszania sie po pozycjach menu

Controls

Escape / P — wlacz / wylacz pauze (Pause Menu)

Strzalki GORA / DÓŁ (lub W / S) — nawigacja po pozycjach menu pauzy

Enter / Space — zatwierdzenie wybranej opcji w menu

Build

RailDispatchMono.Core — build OK

RailDispatchMono.DesktopGL — build OK

Zintegrowano PauseScreen.cs i zaktualizowano MenuScreen.cs

Known gaps / Poza zakresem tego wpisu

Usuwanie toru nie aktualizuje jeszcze polaczen sasiednich elementow

Ruch pociagu nie obsluguje jeszcze pelnego przebiegu po sieci torowej

Pociag nie zatrzymuje sie jeszcze automatycznie przed koncem istniejacego toru

Brak obslugi kierunku jazdy i zmiany kierunku na rozjazdach

Brak interlockingu

Brak wykolejenia

Brak manewrow i sprzegania w warstwie symulacji

Brak planowania jazdy

Brak pasazerow

Brak harmonogramow

[0.0.8] — Train UI Update (Tooltip)
Data: 2026-08-30

Train UI / Tooltip System

Dodano tooltip wyswietlajacy sie po najechaniu kursorem na pociag

Tooltip pojawia sie obok kursora (ekran), nie w swiecie gry

Tooltip znika automatycznie po odsunieciu myszy

Dodano wykrywanie pojazdu pod kursorem w TrainRenderer

Dodano metode GetVehicleAtPosition do sprawdzania kolizji z pojazdami

Dodano metode GetTrainAtPosition jako wrapper dla GetVehicleAtPosition

Dane wyswietlane w tooltipie

Typ pojazdu (LOKOMOTYWA / WAGON)

ID pociagu (skrocone do 8 znakow)

Predkosc w m/s i km/h

Masa pojazdu w kg

Dlugosc pojazdu w metrach

Liczba pojazdow w skladzie

Kierunek jazdy (North, East, South, West)

Wyglad tooltipa

Tlo: czerwone dla lokomotywy, niebieskie dla wagonu

Ramka: jasniejszy odcien koloru tla

Pierwsza linia (typ pojazdu): zolty tekst

Reszta tekstu: bialy

Tooltip dostosowuje pozycje aby nie wychodzic poza ekran

TrainRenderer

Dodano SetTrainManager do przekazywania referencji

Dodano GetVehicleAtPosition z parametrem detectionRadius (domyslnie 0.6f)

Dodano GetTrainAtPosition jako metode pomocnicza

Poprawiono blad kompilacji CS0118 (Train jako namespace vs typ)

GameplayScreen

Dodano _tooltipFont do przechowywania czcionki Arial24

Dodano _pixel do rysowania tla i ramki tooltipa

Dodano metode DrawTooltip do rysowania tooltipa

Zaktualizowano LoadContent o inicjalizacje tooltipa

Zaktualizowano Draw o wywolanie DrawTooltip

Controls

Brak nowych klawiszy - tooltip dziala automatycznie po najechaniu mysza

Build

RailDispatchMono.Core — build OK

RailDispatchMono.DesktopGL — build OK

Naprawiono blad CS0118 w TrainRenderer.cs

Known gaps / Poza zakresem tego wpisu

Usuwanie toru nie aktualizuje jeszcze polaczen sasiednich elementow

Ruch pociagu nie obsluguje jeszcze pelnego przebiegu po sieci torowej

Pociag nie zatrzymuje sie jeszcze automatycznie przed koncem istniejacego toru

Brak obslugi kierunku jazdy i zmiany kierunku na rozjazdach

Brak interlockingu

Brak wykolejenia

Brak manewrow i sprzegania w warstwie symulacji

Brak planowania jazdy

Brak pasazerow

Brak harmonogramow

[0.0.7c] — System debugowania i automatyczne resetowanie semaforow
Data: 2026-08-30


Debug System

- Utworzono centralny system debugowania (DebugManager.cs)
- Dodano kategorie debugowania: General, Block, Signal, Train, TrainMovement, Camera, Input, Render, Map, TrackBuilder, UI, Performance, Error, All
- Logi zapisywane do pliku debug_log_*.txt w folderze gry
- Dodano mozliwosc wlaczania/wylaczania poszczegolnych kategorii debugowania
- Dodano klawisze skrotow do sterowania debugowaniem:
  - F1 - przełącz kategorie BLOCK
  - F2 - przełącz kategorie SIGNAL
  - F3 - przełącz kategorie TRAIN
  - F4 - przełącz kategorie TRAIN_MOVEMENT
  - F5 - przełącz wszystkie kategorie (ON/OFF)
  - F12 - zapisz logi do pliku
- Dodano metody skrotu dla kazdej kategorii: DebugManager.Block(), DebugManager.Signal(), DebugManager.Train(), DebugManager.Input(), DebugManager.Render()
- Dodano historie logow (ostatnie 1000 wpisow)
- Dodano mozliwosc czyszczenia historii logow


Block System / Automatic Signal Reset

- Dodano automatyczne resetowanie semaforow po opuszczeniu bloku przez pociag
- Dodano mechanizm cooldown (0.5s) po opuszczeniu bloku przed zmiana semafora
- Zaktualizowano UpdateSignals() w BlockController:
  - Blok zajety -> nie zmieniaj semafora
  - Blok w cooldown -> ustaw STOP
  - Nastepny blok zajety -> ustaw WARNING
  - Blok wolny -> resetuj na Clear
- Dodano OnTrainExited() w Block do obslugi opuszczenia bloku przez pociag
- Dodano ResetEntrySignals() w Block do resetowania semafora na wejsciu
- Dodano StartCooldown() w Block do rozpoczecia odliczania cooldown
- Zaktualizowano UpdateOccupancy() do prawidlowego przypisywania pociagow do blokow


Signal System

- Dodano mozliwosc recznego przelaczania semaforow (klawisz J)
- Dodano menu wyboru aspektow (PPM na semaforze)
- Semafor automatycznie resetuje sie na Clear po opuszczeniu bloku


Train System / Block Tracking

- Dodano sledzenie blokow przez pociag (CheckBlockChange)
- Dodano SetBlockController() do przekazywania kontrolera blokow do pociagu
- Zaktualizowano TrainMovement o powiadamianie BlockController o zmianie pozycji


Controls

- F1 — przełącz kategorie BLOCK
- F2 — przełącz kategorie SIGNAL
- F3 — przełącz kategorie TRAIN
- F4 — przełącz kategorie TRAIN_MOVEMENT
- F5 — przełącz wszystkie kategorie
- F12 — zapisz logi do pliku
- J — przełącz semafor (Stop ↔ Clear) / przełącz rozjazd


Build

- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK
- Dodano DebugManager.cs
- Dodano using RailDispatchMono.Core.Game.Debug we wszystkich plikach


Known gaps / Poza zakresem tego wpisu

- Usuwanie toru nie aktualizuje jeszcze polaczen sasiednich elementow
- Ruch pociagu nie obsluguje jeszcze pelnego przebiegu po sieci torowej
- Pociag nie zatrzymuje sie jeszcze automatycznie przed koncem istniejacego toru
- Brak obslugi kierunku jazdy i zmiany kierunku na rozjazdach
- Brak interlockingu
- Brak wykolejenia
- Brak manewrow i sprzegania w warstwie symulacji
- Brak planowania jazdy
- Brak pasazerow
- Brak harmonogramow

[0.0.7b] — Refaktoryzacja klasy Train na częściowe pliki
Data: 2026-08-30

**Train System / Code Refactoring**

Przeprowadzono refaktoryzację klasy Train, dzieląc ją na trzy osobne pliki
częściowe (partial), aby poprawić czytelność i utrzymanie kodu:

- **Train.cs** — Główna część klasy:
  - Publiczne API (właściwości, metody publiczne)
  - Konstruktory
  - Zarządzanie składami pojazdów
  - Transformacje pojazdów
  - System sygnalizacji (SignalController)
  - Debugger helpers
  - Publiczne metody gridowe (GetCurrentCell, GetDistanceToBoundary)

- **TrainMovement.cs** — Logika ruchu:
  - Update() — główna pętla aktualizacji
  - Move() — główna logika przemieszczania
  - Obsługa torów prostych (MoveStraight, HandleStraight)
  - Obsługa rozjazdów (HandleJunction)
  - Obsługa zakrętów (EnterCurve, MoveOnCurve, FinishCurve)
  - Przechodzenie między komórkami (EnterNextCell)
  - Metody pomocnicze gridowe

- **TrainGeometry.cs** — Geometria i helpery:
  - Stan łuku (IsOnCurve, ArcCenter, StartAngle, SweepAngle)
  - Parametry łuku (SetupArcParams, GetArcPosition)
  - Historia trajektorii (ResetTrajectory, AddTrajectoryPoint)
  - Pomocnicze metody kierunków (DirectionToVector, GetOppositeDirection)
  - Pomocnicze metody geometryczne (IsPerpendicular, GetCurveExitDirection)
  - Stałe (CurveRadius, DefaultCurveLength, MovementEpsilon)
  - MathHelper (Clamp, LerpAngle)

**Bug Fixes**

- Poprawiono błędy kompilacji CS1061 (brakujące metody)
- Poprawiono błędy kompilacji CS0103 (brakujące konteksty)
- Dodano brakującą publiczną metodę GetCurrentCell() dla TrainManager
- Dodano brakującą publiczną metodę GetDistanceToBoundary() dla debuggera
- Poprawiono sygnatury wywołań GetNextCell() z jawnym parametrem direction
- Poprawiono sygnatury wywołań GetPositionAtEntry() z jawnym parametrem direction
- Dodano brakującą metodę ValidateDirection()
- Dodano brakującą metodę VectorToDirection()

**Build**

- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK

**Test Results**

- Wszystkie metody publiczne są dostępne dla TrainManager i TrainDebugger
- Pociąg prawidłowo porusza się po torach prostych, zakrętach i rozjazdach
- System sygnalizacji działa poprawnie
- Trajektoria pojazdów jest prawidłowo obliczana

**Known gaps / Poza zakresem tego wpisu**

- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów
- Ruch pociągu nie obsługuje jeszcze pełnego przebiegu po sieci torowej
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru
- Brak obsługi kierunku jazdy i zmiany kierunku na rozjazdach
- Brak sekcji blokowych (BlockSection)
- Brak interlockingu
- Brak wykolejenia
- Brak manewrów i sprzęgania w warstwie symulacji
- Brak planowania jazdy
- Brak pasażerów
- Brak harmonogramów

[0.0.7a] — Naprawa wykrywania połączeń w prostych torach Data: 2026-08-29

Bug Fixes
- Naprawiono GetExitDirection w TrackCell.cs dla prostych torów
- Metoda GetExitDirection zwracała TrackConnections.None dla Straight
- Dodano poprawne obliczanie przeciwnego połączenia: Connections & ~entrySide
- Naprawiono zatrzymywanie pociągu na (88,89) mimo prawidłowych połączeń
- Usunięto błędne zwracanie None dla torów prostych w GetExitDirection

Train System
- Poprawiono EnterNextCell w Train.cs
- Dodano GetOppositeDirection(Direction) zamiast Direction
- Prawidłowe wykrywanie połączeń przy wejściu do następnej komórki
- Dodano logi diagnostyczne w EnterNextCell

TrackBuilder
- Dodano ręczne budowanie brakujących torów w CreateTestTrack()
- Dodano logi diagnostyczne dla torów na dolnej prostej
- Potwierdzono poprawne Connections = East, West dla wszystkich torów

Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK

Test Results
- Tory na dolnej prostej mają poprawne Connections = East, West
- Pociąg prawidłowo przechodzi przez komórkę (88,89)
- Pociąg kontynuuje jazdę po całej trasie
- Zakręty obsługiwane poprawnie

Known gaps / Poza zakresem tego wpisu
- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów
- Ruch pociągu nie obsługuje jeszcze pełnego przebiegu po sieci torowej
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru
- Brak obsługi kierunku jazdy i zmiany kierunku na rozjazdach
- Brak sekcji blokowych (BlockSection)
- Brak interlockingu
- Brak wykolejenia
- Brak manewrów i sprzęgania w warstwie symulacji
- Brak planowania jazdy
- Brak pasażerów
- Brak harmonogramów

[0.0.7] — Obsługa rozjazdów przez pociągi Data: 2026-08-29

Train System / Junction Handling
- Dodano pełną obsługę rozjazdów (Junction) w ruchu pociągu
- Pociąg odczytuje ustawienie iglicy (SwitchPosition: Straight / Diverging)
- Pociąg wybiera właściwy kierunek wyjścia z rozjazdu na podstawie ustawienia iglicy
- Dodano obsługę skrętu na rozjeździe (wejście na łuk)
- Dodano obsługę jazdy na wprost przez rozjazd
- Dodano logi debugowania [JUNCTION] z informacjami o:
  - Wejściu na rozjazd (Entering)
  - Kierunku wjazdu (Entry)
  - Ustawieniu iglicy (Switch)
  - Kierunku wyjścia (Exit)
  - Typie przejazdu (Going straight / Turning)

TrackCell
- Rozszerzono GetExitDirection o obsługę SwitchPosition
- Dodano właściwości: StraightConnection, DivergingConnection, CommonStem
- Dodano CurrentSwitchPosition do przechowywania stanu iglicy
- Dodano ToggleSwitch do przełączania iglicy

Movement
- Dodano GetPositionAtEntry do znajdowania pozycji wyjścia z komórki
- Poprawiono przechodzenie przez rozjazdy w Move()
- Zintegrowano obsługę rozjazdów z istniejącym systemem zakrętów

Bug Fixes
- Usunięto duplikat metody GetPositionAtExit (CS0111)
- Poprawiono błąd kompilacji w TrackCell.cs (CS0106)

Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK

Test Results
- Pociąg prawidłowo odczytuje ustawienie iglicy rozjazdu
- Pociąg wybiera właściwy kierunek (prosto / skręt)
- Skręt na rozjeździe płynnie przechodzi w łuk
- Jazda na wprost przez rozjazd działa poprawnie
- Logi [JUNCTION] dostarczają pełnej diagnostyki ruchu

Known gaps / Poza zakresem tego wpisu
- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru
- Brak sekcji blokowych (BlockSection)
- Brak interlockingu
- Brak pełnej fizyki ruchu pociągu
- Brak wykolejenia
- Brak manewrów i sprzęgania w warstwie symulacji
- Brak planowania jazdy

[0.0.6b] — Rozszerzenie logowania prędkości i diagnostyka ruchu
Train System / Diagnostics
- Dodano szczegółowe logi prędkości w Train.Update()
- Dodano oznaczenia: 🟢 START, 🟡 SIGNAL, 🔵 NO_SIGNAL, ⚪ NO_SIGNAL_HISTORY
- Dodano logi przyspieszania (🚀 ACCEL) i hamowania (🛑 BRAKE)
- Dodano logi stałej prędkości (➡️ CONST) i ruchu (🏃 MOVE)
- Dodano wyświetlanie prędkości w m/s i km/h

Signal Detection
- Rozszerzono GetNextSignal o logi wykrywania sygnałów w bieżącej i następnej komórce
- Dodano logi dla sygnałów Warning (S5) i Clear (S2)
- Potwierdzono poprawne działanie wykrywania sygnałów na zakrętach

Curve Movement
- Dodano logi wejścia i wyjścia z zakrętu (CURVE Enter / FINISH CURVE)
- Dodano informacje o środku łuku, kątach i postępie
- Potwierdzono poprawne działanie geometrii zakrętów

Debugger
- Rozszerzono TrainDebugger o informacje o łuku (ArcCenter, Curve progress)
- Dodano wyświetlanie rotacji pojazdów

Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK

Test Results
- Pociąg prawidłowo przyspiesza z parametrem acceleration: 0.8f
- Sygnały wykrywane w bieżącej i następnej komórce
- Zakręty obsługiwane bez utraty prędkości
- Prędkość wzrasta z 3,73 m/s do 5,23 m/s na trasie"

[0.0.6] — System semaforów i rozjazdów - integracja z ruchem pociągów Data: 2026-08-29

Train System / Signal Integration

Dodano pełną integrację semaforów z ruchem pociągów.
Pociąg wykrywa semafory w bieżącej i następnej komórce.
Dodano mechanizm zapamiętywania ostatniego napotkanego semafora (obowiązuje do następnego).
Pociąg płynnie dostosowuje prędkość do aspektu semafora (przyspieszanie/hamowanie).
Dodano parametry hamowania (braking) do VehicleParameters.
Pociąg wykorzystuje parametry pojazdów do obliczania przyspieszania i hamowania.
Dodano SetSignalController w celu przekazania kontrolera semaforów do pociągu.
Dodano GetNextSignal do odczytywania semaforów z mapy.
Dodano GetSpeedFromSignal do mapowania aspektów na prędkość w m/s.
Dodano logi debugowania ruchu i wykrywania semaforów.
Train Physics

Dodano płynną zmianę prędkości z uwzględnieniem parametrów pojazdów.
Prędkość docelowa (targetSpeed) ustalana na podstawie ostatniego semafora.
Hamowanie z wykorzystaniem parametru braking z pojazdów (domyślnie 100 m/s²).
Przyspieszanie z wykorzystaniem parametru acceleration z pojazdów.
Dodano zabezpieczenie przed przekroczeniem maxSpeed pojazdów.
Dodano zatrzymywanie pociągu przy prędkości bliskiej 0.
Signal System

Dodano możliwość ustawiania początkowego aspektu semafora podczas tworzenia.
Dodano SignalController.GetSignalAt do pobierania semafora na konkretnej pozycji.
Dodano SignalController.GetSignalsAt do pobierania wszystkich semaforów w komórce.
Dodano Signal.AvailableAspects do definiowania dostępnych aspektów dla danego semafora.
Dodano Signal.GetAspectName jako metodę rozszerzającą.
Dodano SignalAspectInfo z pełnymi opisami aspektów.
Dodano SignalAspectExtensions z metodami GetName, GetDescription, GetSpeedLimit.
TrainManager / Gameplay

Dodano przekazywanie SignalController do TrainManager.
Dodano automatyczne ustawianie SignalController dla każdego nowego pociągu.
Dodano tworzenie pojazdów przed konstruktorem Train w CreateTestTrain.
Dodano przeciążenie konstruktora Train bez pojazdów (dla TrainManager i Decouple).
Dodano domyślne pojazdy w TrainManager dla przypadków brzegowych.
Bug Fixes

Naprawiono błąd braku pojazdów w konstruktorze Train (CS7036).
Naprawiono resetowanie _lastSignal w SetPosition i SetDirection.
Naprawiono wykrywanie semaforów w GetNextSignal.
Naprawiono parametr braking w VehicleParameters.
Dodano brakujący konstruktor Train(Vector2, TrackConnections, float).
Controls

1 — tor prosty
2 — zakręt
3 — rozjazd
4 — semafor
H — orientacja pozioma (tor prosty)
V — orientacja pionowa (tor prosty)
R — obrót zakrętu / zmiana typu rozjazdu
J — przełącz rozjazd / przełącz semafor (Stop ↔ Clear)
LPM — postaw element toru / semafor
PPM — usuń element toru / otwórz menu rozjazdu lub semafora
MMB — przesuwanie kamery
Kółko myszy — zoom mapy
Escape — zamknij menu radialne
Build

RailDispatchMono.Core — build OK.
RailDispatchMono.DesktopGL — build OK.
Dodano wszystkie nowe klasy semaforów i rozjazdów.
Known gaps / Poza zakresem tego wpisu

Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów.
Ruch pociągu nie obsługuje jeszcze pełnego przebiegu po sieci torowej.
Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru.
Brak obsługi kierunku jazdy i zmiany kierunku na rozjazdach.
Brak sekcji blokowych (BlockSection).
Brak interlockingu.
Brak wykolejenia.
Brak manewrów i sprzęgania w warstwie symulacji.
Brak planowania jazdy.
Brak pasażerów.
Brak harmonogramów.
Brak proceduralnego generowania terenu.

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