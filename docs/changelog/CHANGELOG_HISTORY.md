# Changelog

## [0.0.9] — System przewidywania semaforów i hamowania przed sygnałem
**Data:** 2026-08-30

### Train System / Signal Look-Ahead
- Rozszerzono system wykrywania semaforów poza bieżącą i następną komórkę toru.
- Pociąg wyszukuje najbliższy semafor znajdujący się przed nim w aktualnym kierunku jazdy.
- Dodano przechodzenie po kolejnych komórkach torowych w celu znalezienia następnego semafora.
- Uwzględniono rzeczywistą odległość pociągu od wykrytego semafora zamiast odległości wyłącznie do granicy bieżącej komórki.
- Pociąg może przewidywać sygnał znajdujący się kilka lub więcej komórek przed nim.
- Semafor znajdujący się za pociągiem nie jest traktowany jako następny sygnał.
- Zachowano obsługę torów prostych, zakrętów i istniejącej logiki przejazdu przez rozjazdy.

### Signal / Braking Integration
- Zintegrowano rzeczywistą odległość do semafora z obliczaniem bezpiecznej prędkości.
- Dla sygnału STOP pociąg wyznacza maksymalną bezpieczną prędkość na podstawie:
  - aktualnej odległości do semafora,
  - dostępnego współczynnika hamowania.
- Zastosowano zależność: `v_safe = sqrt(2 * a * distance)`
- Pociąg rozpoczyna hamowanie przed semaforem, jeżeli aktualna prędkość wymaga redukcji w celu zatrzymania przed sygnałem.
- Dla semaforów z ograniczeniem prędkości pociąg rozpoczyna hamowanie dopiero po wejściu w odpowiednią strefę hamowania.
- Nie występuje już sytuacja, w której pociąg rozpoczyna reakcję na STOP dopiero po jego minięciu, o ile fizycznie posiada wystarczającą drogę hamowania.
- Zachowano fizyczne ograniczenie hamowania — pociąg nie może natychmiast zmienić prędkości z powodu pojawienia się sygnału.

### Braking Distance
- Dodano wykorzystanie drogi hamowania do określania momentu rozpoczęcia redukcji prędkości.
- Droga hamowania jest zależna od aktualnej prędkości, prędkości docelowej i parametrów hamowania.
- Dla ograniczeń prędkości zastosowano zależność: `s = (v² - v_target²) / (2a)`
- Pociąg może kontynuować jazdę z aktualną prędkością, jeżeli znajduje się poza strefą wymaganego hamowania.
- Po wejściu w strefę hamowania prędkość jest redukowana płynnie zgodnie z istniejącym systemem fizyki ruchu.

### Track Traversal / Route Look-Ahead
- Dodano przechodzenie przez kolejne elementy sieci torowej podczas wyszukiwania semafora.
- Look-ahead wykorzystuje kierunek jazdy pociągu do określenia kolejnych komórek.
- Uwzględniono możliwość przechodzenia przez zakręty podczas wyszukiwania kolejnego sygnału.
- Wyszukiwanie semafora zostało oddzielone od samego przemieszczania pociągu.
- Przygotowano podstawę pod przyszłe wykorzystanie rzeczywistej trasy wynikającej z systemu blokowego i ustawienia rozjazdów.

### Debug / Diagnostics
- Ograniczono zależność diagnostyki hamowania od logów wykonywanych przy każdej klatce.
- Zachowano możliwość diagnostyki wykrywania semafora i parametrów hamowania za pomocą istniejącego `DebugManager`.
- Logi mogą być wykorzystane do sprawdzania:
  - wykrytego następnego semafora,
  - kierunku jazdy,
  - odległości do semafora,
  - aktualnej prędkości,
  - docelowej prędkości,
  - rozpoczęcia hamowania.

### Documentation
- Dodano dokumentację systemu przewidywania semaforów i hamowania.
- Opisano sposób obliczania odległości do następnego sygnału.
- Opisano zależność pomiędzy odległością, prędkością i hamowaniem.
- Opisano różnicę pomiędzy reakcją na STOP a reakcją na ograniczenie prędkości.
- Udokumentowano obecne ograniczenia systemu wyszukiwania trasy.

### Bug Fixes
- Naprawiono zbyt późną reakcję pociągu na semafor STOP znajdujący się kilka komórek przed pociągiem.
- Naprawiono używanie odległości do granicy bieżącej komórki jako zastępczej odległości do semafora.
- Poprawiono zachowanie hamowania na trasach, na których semafor znajduje się za zakrętem.
- Pociąg nie musi już znajdować się bezpośrednio przy semaforze, aby rozpocząć reakcję na jego aspekt.

### Test Results
- Potwierdzono wykrywanie semaforów znajdujących się dalej niż jedna komórka od pociągu.
- Potwierdzono rozpoczęcie hamowania przed semaforem STOP.
- Potwierdzono płynne zmniejszanie prędkości podczas zbliżania się do STOP.
- Potwierdzono zatrzymanie pociągu przed sygnałem przy prędkości umożliwiającej fizyczne zatrzymanie.
- Potwierdzono działanie mechanizmu podczas jazdy po łuku.
- Potwierdzono zachowanie ograniczeń prędkości innych niż STOP.
- Potwierdzono ręczne przełączanie semafora za pomocą klawisza J.

### Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK

### Known Gaps / Poza zakresem tego wpisu
- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów.
- Look-ahead nie wykorzystuje jeszcze pełnego systemu rezerwacji trasy.
- Przy rozgałęzieniach wybór dalszej trasy nie jest jeszcze w pełni powiązany z systemem rezerwacji bloków.
- Brak pełnego interlockingu.
- Brak wykolejenia.
- Brak manewrów i sprzęgania w warstwie symulacji.
- Brak planowania jazdy.
- Brak pasażerów.
- Brak harmonogramów.
- Brak pełnego modelu przebiegu pociągu po całej sieci torowej.

---

## [0.0.8b] — System pauzy - naprawa wyświetlania menu
**Data:** 2026-08-30

### Pause System / Bug Fixes
- Naprawiono wyświetlanie opcji menu pauzy (Resume, Quit).
- Dodano logi debugowania w MenuScreen.Draw() i UpdateMenuEntryLocations().
- Poprawiono wywołanie base.Draw() w PauseScreen.
- Zaktualizowano MenuScreen o szczegółowe logi diagnostyczne.

### Screen Management
- Dodano właściwość InputState w ScreenManager (publiczny dostęp).
- Poprawiono obsługę wejścia dla menu pauzy.
- Zaktualizowano GameplayScreen o korzystanie z InputState.

### Input System
- Dodano metodę IsPauseKeyJustPressed() w InputState.
- Zintegrowano InputState z GameplayScreen.Update().

### Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK
- Usunięto projekty Android i iOS z rozwiązania (tylko DesktopGL).

### Controls
- `Escape` / `P` — włącz / wyłącz pauzę (Pause Menu)
- `Strzałki GÓRA` / `DÓŁ` (lub `W` / `S`) — nawigacja po pozycjach menu pauzy
- `Enter` / `Space` — zatwierdzenie wybranej opcji w menu

### Known Gaps / Poza zakresem tego wpisu
- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów.
- Ruch pociągu nie obsługuje jeszcze pełnego przebiegu po sieci torowej.
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru.
- Brak obsługi kierunku jazdy i zmiany kierunku na rozjazdach.
- Brak interlockingu.
- Brak wykolejenia.
- Brak manewrów i sprzęgania w warstwie symulacji.
- Brak planowania jazdy.
- Brak pasażerów.
- Brak harmonogramów.

---

## [0.0.8a] — System pauzy i wsparcie menu ekranowego
**Data:** 2026-08-30

### Game Loop / Pause System
- Dodano pełny ekran pauzy (PauseScreen.cs) wstrzymujący aktualizacje symulacji.
- Zaktualizowano GameplayScreen o obsługę stanu pauzy i nakładki menu.
- Zoptymalizowano pętlę gry w RailDispatchMonoGame.cs pod kątem przełączania ekranów.

### UI / Screen Management & Fonts
- Dodano czcionki Hud.spritefont oraz zaktualizowano Arial24.spritefont.
- Rozbudowano MenuScreen.cs oraz MenuEntry.cs do obsługi dynamicznych pozycji menu.
- Poprawiono zarządzanie i rysowanie elementów interfejsu użytkownika w PauseScreen.
- Dodano obsługę nawigacji po menu w PauseScreen (wznowienie gry, opcje, powrót do menu).

### Input Manager
- Zaktualizowano InputManager.cs o obsługę wejścia dla ekranu pauzy i nawigacji menu.
- Dodano wykrywanie klawiszy aktywacji pauzy oraz poruszania się po pozycjach menu.

### Controls
- `Escape` / `P` — włącz / wyłącz pauzę (Pause Menu)
- `Strzałki GÓRA` / `DÓŁ` (lub `W` / `S`) — nawigacja po pozycjach menu pauzy
- `Enter` / `Space` — zatwierdzenie wybranej opcji w menu

### Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK
- Zintegrowano PauseScreen.cs i zaktualizowano MenuScreen.cs

### Known Gaps / Poza zakresem tego wpisu
- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów.
- Ruch pociągu nie obsługuje jeszcze pełnego przebiegu po sieci torowej.
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru.
- Brak obsługi kierunku jazdy i zmiany kierunku na rozjazdach.
- Brak interlockingu.
- Brak wykolejenia.
- Brak manewrów i sprzęgania w warstwie symulacji.
- Brak planowania jazdy.
- Brak pasażerów.
- Brak harmonogramów.

---

## [0.0.8] — Train UI Update (Tooltip)
**Data:** 2026-08-30

### Train UI / Tooltip System
- Dodano tooltip wyświetlający się po najechaniu kursorem na pociąg.
- Tooltip pojawia się obok kursora (ekran), nie w świecie gry.
- Tooltip znika automatycznie po odsunięciu myszy.
- Dodano wykrywanie pojazdu pod kursorem w TrainRenderer.
- Dodano metodę GetVehicleAtPosition do sprawdzania kolizji z pojazdami.
- Dodano metodę GetTrainAtPosition jako wrapper dla GetVehicleAtPosition.

### Dane wyświetlane w tooltipie
- Typ pojazdu (LOKOMOTYWA / WAGON)
- ID pociągu (skrócone do 8 znaków)
- Prędkość w m/s i km/h
- Masa pojazdu w kg
- Długość pojazdu w metrach
- Liczba pojazdów w składzie
- Kierunek jazdy (North, East, South, West)

### Wygląd tooltipa
- Tło: czerwone dla lokomotywy, niebieskie dla wagonu.
- Ramka: jaśniejszy odcień koloru tła.
- Pierwsza linia (typ pojazdu): żółty tekst.
- Reszta tekstu: biały.
- Tooltip dostosowuje pozycję aby nie wychodzić poza ekran.

### TrainRenderer
- Dodano SetTrainManager do przekazywania referencji.
- Dodano GetVehicleAtPosition z parametrem detectionRadius (domyślnie 0.6f).
- Dodano GetTrainAtPosition jako metodę pomocniczą.
- Poprawiono błąd kompilacji CS0118 (Train jako namespace vs typ).

### GameplayScreen
- Dodano _tooltipFont do przechowywania czcionki Arial24.
- Dodano _pixel do rysowania tła i ramki tooltipa.
- Dodano metodę DrawTooltip do rysowania tooltipa.
- Zaktualizowano LoadContent o inicjalizację tooltipa.
- Zaktualizowano Draw o wywołanie DrawTooltip.

### Controls
- Brak nowych klawiszy — tooltip działa automatycznie po najechaniu myszą.

### Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK
- Naprawiono błąd CS0118 w TrainRenderer.cs

### Known Gaps / Poza zakresem tego wpisu
- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów.
- Ruch pociągu nie obsługuje jeszcze pełnego przebiegu po sieci torowej.
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru.
- Brak obsługi kierunku jazdy i zmiany kierunku na rozjazdach.
- Brak interlockingu.
- Brak wykolejenia.
- Brak manewrów i sprzęgania w warstwie symulacji.
- Brak planowania jazdy.
- Brak pasażerów.
- Brak harmonogramów.

---

## [0.0.7c] — System debugowania i automatyczne resetowanie semaforów
**Data:** 2026-08-30

### Debug System
- Utworzono centralny system debugowania (DebugManager.cs).
- Dodano kategorie debugowania: General, Block, Signal, Train, TrainMovement, Camera, Input, Render, Map, TrackBuilder, UI, Performance, Error, All.
- Logi zapisywane do pliku debug_log_*.txt w folderze gry.
- Dodano możliwość włączania/wyłączania poszczególnych kategorii debugowania.
- Dodano klawisze skrótów do sterowania debugowaniem:
  - `F1` — przełącz kategorie BLOCK
  - `F2` — przełącz kategorie SIGNAL
  - `F3` — przełącz kategorie TRAIN
  - `F4` — przełącz kategorie TRAIN_MOVEMENT
  - `F5` — przełącz wszystkie kategorie (ON/OFF)
  - `F12` — zapisz logi do pliku
- Dodano metody skrótu dla każdej kategorii: DebugManager.Block(), DebugManager.Signal(), DebugManager.Train(), DebugManager.Input(), DebugManager.Render().
- Dodano historię logów (ostatnie 1000 wpisów).
- Dodano możliwość czyszczenia historii logów.

### Block System / Automatic Signal Reset
- Dodano automatyczne resetowanie semaforów po opuszczeniu bloku przez pociąg.
- Dodano mechanizm cooldown (0.5s) po opuszczeniu bloku przed zmianą semafora.
- Zaktualizowano UpdateSignals() w BlockController:
  - Blok zajęty -> nie zmieniaj semafora
  - Blok w cooldown -> ustaw STOP
  - Następny blok zajęty -> ustaw WARNING
  - Blok wolny -> resetuj na Clear
- Dodano OnTrainExited() w Block do obsługi opuszczenia bloku przez pociąg.
- Dodano ResetEntrySignals() w Block do resetowania semafora na wejściu.
- Dodano StartCooldown() w Block do rozpoczęcia odliczania cooldown.
- Zaktualizowano UpdateOccupancy() do prawidłowego przypisywania pociągów do bloków.

### Signal System
- Dodano możliwość ręcznego przełączania semaforów (klawisz J).
- Dodano menu wyboru aspektów (PPM na semaforze).
- Semafor automatycznie resetuje się na Clear po opuszczeniu bloku.

### Train System / Block Tracking
- Dodano śledzenie bloków przez pociąg (CheckBlockChange).
- Dodano SetBlockController() do przekazywania kontrolera bloków do pociągu.
- Zaktualizowano TrainMovement o powiadamianie BlockController o zmianie pozycji.

### Controls
- `F1` — przełącz kategorie BLOCK
- `F2` — przełącz kategorie SIGNAL
- `F3` — przełącz kategorie TRAIN
- `F4` — przełącz kategorie TRAIN_MOVEMENT
- `F5` — przełącz wszystkie kategorie
- `F12` — zapisz logi do pliku
- `J` — przełącz semafor (Stop ↔ Clear) / przełącz rozjazd

### Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK
- Dodano DebugManager.cs
- Dodano using RailDispatchMono.Core.Game.Debug we wszystkich plikach

### Known Gaps / Poza zakresem tego wpisu
- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów.
- Ruch pociągu nie obsługuje jeszcze pełnego przebiegu po sieci torowej.
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru.
- Brak obsługi kierunku jazdy i zmiany kierunku na rozjazdach.
- Brak interlockingu.
- Brak wykolejenia.
- Brak manewrów i sprzęgania w warstwie symulacji.
- Brak planowania jazdy.
- Brak pasażerów.
- Brak harmonogramów.

---

## [0.0.7b] — Refaktoryzacja klasy Train na częściowe pliki
**Data:** 2026-08-30

### Train System / Code Refactoring
Przeprowadzono refaktoryzację klasy Train, dzieląc ją na trzy osobne pliki częściowe (partial), aby poprawić czytelność i utrzymanie kodu:

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

### Bug Fixes
- Poprawiono błędy kompilacji CS1061 (brakujące metody).
- Poprawiono błędy kompilacji CS0103 (brakujące konteksty).
- Dodano brakującą publiczną metodę GetCurrentCell() dla TrainManager.
- Dodano brakującą publiczną metodę GetDistanceToBoundary() dla debuggera.
- Poprawiono sygnatury wywołań GetNextCell() z jawnym parametrem direction.
- Poprawiono sygnatury wywołań GetPositionAtEntry() z jawnym parametrem direction.
- Dodano brakującą metodę ValidateDirection().
- Dodano brakującą metodę VectorToDirection().

### Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK

### Test Results
- Wszystkie metody publiczne są dostępne dla TrainManager i TrainDebugger.
- Pociąg prawidłowo porusza się po torach prostych, zakrętach i rozjazdach.
- System sygnalizacji działa poprawnie.
- Trajektoria pojazdów jest prawidłowo obliczana.

### Known Gaps / Poza zakresem tego wpisu
- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów.
- Ruch pociągu nie obsługuje jeszcze pełnego przebiegu po sieci torowej.
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru.
- Brak obsługi kierunku jazdy i zmiany kierunku na rozjazdach.
- Brak sekcji blokowych (BlockSection).
- Brak interlockingu.
- Brak wykolejenia.
- Brak manewrów i sprzęgania w warstwie symulacji.
- Brak planowania jazdy.
- Brak pasażerów.
- Brak harmonogramów.

---

## [0.0.7a] — Naprawa wykrywania połączeń w prostych torach
**Data:** 2026-08-29

### Bug Fixes
- Naprawiono GetExitDirection w TrackCell.cs dla prostych torów.
- Metoda GetExitDirection zwracała TrackConnections.None dla Straight.
- Dodano poprawne obliczanie przeciwnego połączenia: Connections & ~entrySide.
- Naprawiono zatrzymywanie pociągu na (88,89) mimo prawidłowych połączeń.
- Usunięto błędne zwracanie None dla torów prostych w GetExitDirection.

### Train System
- Poprawiono EnterNextCell w Train.cs.
- Dodano GetOppositeDirection(Direction) zamiast Direction.
- Prawidłowe wykrywanie połączeń przy wejściu do następnej komórki.
- Dodano logi diagnostyczne w EnterNextCell.

### TrackBuilder
- Dodano ręczne budowanie brakujących torów w CreateTestTrack().
- Dodano logi diagnostyczne dla torów na dolnej prostej.
- Potwierdzono poprawne Connections = East, West dla wszystkich torów.

### Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK

### Test Results
- Tory na dolnej prostej mają poprawne Connections = East, West.
- Pociąg prawidłowo przechodzi przez komórkę (88,89).
- Pociąg kontynuuje jazdę po całej trasie.
- Zakręty obsługiwane poprawnie.

### Known Gaps / Poza zakresem tego wpisu
- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów.
- Ruch pociągu nie obsługuje jeszcze pełnego przebiegu po sieci torowej.
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru.
- Brak obsługi kierunku jazdy i zmiany kierunku na rozjazdach.
- Brak sekcji blokowych (BlockSection).
- Brak interlockingu.
- Brak wykolejenia.
- Brak manewrów i sprzęgania w warstwie symulacji.
- Brak planowania jazdy.
- Brak pasażerów.
- Brak harmonogramów.

---

## [0.0.7] — Obsługa rozjazdów przez pociągi
**Data:** 2026-08-29

### Train System / Junction Handling
- Dodano pełną obsługę rozjazdów (Junction) w ruchu pociągu.
- Pociąg odczytuje ustawienie iglicy (SwitchPosition: Straight / Diverging).
- Pociąg wybiera właściwy kierunek wyjścia z rozjazdu na podstawie ustawienia iglicy.
- Dodano obsługę skrętu na rozjeździe (wejście na łuk).
- Dodano obsługę jazdy na wprost przez rozjazd.
- Dodano logi debugowania [JUNCTION] z informacjami o:
  - Wejściu na rozjazd (Entering)
  - Kierunku wjazdu (Entry)
  - Ustawieniu iglicy (Switch)
  - Kierunku wyjścia (Exit)
  - Typie przejazdu (Going straight / Turning)

### TrackCell
- Rozszerzono GetExitDirection o obsługę SwitchPosition.
- Dodano właściwości: StraightConnection, DivergingConnection, CommonStem.
- Dodano CurrentSwitchPosition do przechowywania stanu iglicy.
- Dodano ToggleSwitch do przełączania iglicy.

### Movement
- Dodano GetPositionAtEntry do znajdowania pozycji wyjścia z komórki.
- Poprawiono przechodzenie przez rozjazdy w Move().
- Zintegrowano obsługę rozjazdów z istniejącym systemem zakrętów.

### Bug Fixes
- Usunięto duplikat metody GetPositionAtExit (CS0111).
- Poprawiono błąd kompilacji w TrackCell.cs (CS0106).

### Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK

### Test Results
- Pociąg prawidłowo odczytuje ustawienie iglicy rozjazdu.
- Pociąg wybiera właściwy kierunek (prosto / skręt).
- Skręt na rozjeździe płynnie przechodzi w łuk.
- Jazda na wprost przez rozjazd działa poprawnie.
- Logi [JUNCTION] dostarczają pełnej diagnostyki ruchu.

### Known Gaps / Poza zakresem tego wpisu
- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów.
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru.
- Brak sekcji blokowych (BlockSection).
- Brak interlockingu.
- Brak pełnej fizyki ruchu pociągu.
- Brak wykolejenia.
- Brak manewrów i sprzęgania w warstwie symulacji.
- Brak planowania jazdy.

---

## [0.0.6b] — Rozszerzenie logowania prędkości i diagnostyka ruchu
**Data:** 2026-08-29

### Train System / Diagnostics
- Dodano szczegółowe logi prędkości w Train.Update().
- Dodano oznaczenia: 🟢 START, 🟡 SIGNAL, 🔵 NO_SIGNAL, ⚪ NO_SIGNAL_HISTORY.
- Dodano logi przyspieszania (🚀 ACCEL) i hamowania (🛑 BRAKE).
- Dodano logi stałej prędkości (➡️ CONST) i ruchu (🏃 MOVE).
- Dodano wyświetlanie prędkości w m/s i km/h.

### Signal Detection
- Rozszerzono GetNextSignal o logi wykrywania sygnałów w bieżącej i następnej komórce.
- Dodano logi dla sygnałów Warning (S5) i Clear (S2).
- Potwierdzono poprawne działanie wykrywania sygnałów na zakrętach.

### Curve Movement
- Dodano logi wejścia i wyjścia z zakrętu (CURVE Enter / FINISH CURVE).
- Dodano informacje o środku łuku, kątach i postępie.
- Potwierdzono poprawne działanie geometrii zakrętów.

### Debugger
- Rozszerzono TrainDebugger o informacje o łuku (ArcCenter, Curve progress).
- Dodano wyświetlanie rotacji pojazdów.

### Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK

### Test Results
- Pociąg prawidłowo przyspiesza z parametrem acceleration: 0.8f.
- Sygnały wykrywane w bieżącej i następnej komórce.
- Zakręty obsługiwane bez utraty prędkości.
- Prędkość wzrasta z 3,73 m/s do 5,23 m/s na trasie.

---

## [0.0.6] — System semaforów i rozjazdów - integracja z ruchem pociągów
**Data:** 2026-08-29

### Train System / Signal Integration
- Dodano pełną integrację semaforów z ruchem pociągów.
- Pociąg wykrywa semafory w bieżącej i następnej komórce.
- Dodano mechanizm zapamiętywania ostatniego napotkanego semafora (obowiązuje do następnego).
- Pociąg płynnie dostosowuje prędkość do aspektu semafora (przyspieszanie/hamowanie).
- Dodano parametry hamowania (braking) do VehicleParameters.
- Pociąg wykorzystuje parametry pojazdów do obliczania przyspieszania i hamowania.
- Dodano SetSignalController w celu przekazania kontrolera semaforów do pociągu.
- Dodano GetNextSignal do odczytywania semaforów z mapy.
- Dodano GetSpeedFromSignal do mapowania aspektów na prędkość w m/s.
- Dodano logi debugowania ruchu i wykrywania semaforów.

### Train Physics
- Dodano płynną zmianę prędkości z uwzględnieniem parametrów pojazdów.
- Prędkość docelowa (targetSpeed) ustalana na podstawie ostatniego semafora.
- Hamowanie z wykorzystaniem parametru braking z pojazdów (domyślnie 100 m/s²).
- Przyspieszanie z wykorzystaniem parametru acceleration z pojazdów.
- Dodano zabezpieczenie przed przekroczeniem maxSpeed pojazdów.
- Dodano zatrzymywanie pociągu przy prędkości bliskiej 0.

### Signal System
- Dodano możliwość ustawiania początkowego aspektu semafora podczas tworzenia.
- Dodano SignalController.GetSignalAt do pobierania semafora na konkretnej pozycji.
- Dodano SignalController.GetSignalsAt do pobierania wszystkich semaforów w komórce.
- Dodano Signal.AvailableAspects do definiowania dostępnych aspektów dla danego semafora.
- Dodano Signal.GetAspectName jako metodę rozszerzającą.
- Dodano SignalAspectInfo z pełnymi opisami aspektów.
- Dodano SignalAspectExtensions z metodami GetName, GetDescription, GetSpeedLimit.

### TrainManager / Gameplay
- Dodano przekazywanie SignalController do TrainManager.
- Dodano automatyczne ustawianie SignalController dla każdego nowego pociągu.
- Dodano tworzenie pojazdów przed konstruktorem Train w CreateTestTrain.
- Dodano przeciążenie konstruktora Train bez pojazdów (dla TrainManager i Decouple).
- Dodano domyślne pojazdy w TrainManager dla przypadków brzegowych.

### Bug Fixes
- Naprawiono błąd braku pojazdów w konstruktorze Train (CS7036).
- Naprawiono resetowanie _lastSignal w SetPosition i SetDirection.
- Naprawiono wykrywanie semaforów w GetNextSignal.
- Naprawiono parametr braking w VehicleParameters.
- Dodano brakujący konstruktor Train(Vector2, TrackConnections, float).

### Controls
- `1` — tor prosty
- `2` — zakręt
- `3` — rozjazd
- `4` — semafor
- `H` — orientacja pozioma (tor prosty)
- `V` — orientacja pionowa (tor prosty)
- `R` — obrót zakrętu / zmiana typu rozjazdu
- `J` — przełącz rozjazd / przełącz semafor (Stop ↔ Clear)
- `LPM` — postaw element toru / semafor
- `PPM` — usuń element toru / otwórz menu rozjazdu lub semafora
- `MMB` — przesuwanie kamery
- `Kółko myszy` — zoom mapy
- `Escape` — zamknij menu radialne

### Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK
- Dodano wszystkie nowe klasy semaforów i rozjazdów.

### Known Gaps / Poza zakresem tego wpisu
- Usuwanie toru nie aktualizuje jeszcze połączeń sąsiednich elementów.
- Ruch pociągu nie obsługuje jeszcze pełnego przebiegu po sieci torowej.
- Pociąg nie zatrzymuje się jeszcze automatycznie przed końcem istniejącego toru.
- Brak obsługi kierunku jazdy i zmiany kierunku na rozjazdach.
- Brak sekcji blokowych (BlockSection).
- Brak interlockingu.
- Brak wykolejenia.
- Brak manewrów i sprzęgania w warstwie symulacji.
- Brak planowania jazdy.
- Brak pasażerów.
- Brak harmonogramów.
- Brak proceduralnego generowania terenu.

---

## [0.0.5] — System semaforów i rozjazdów
**Data:** 2026-08-29

### Railway / Track Building- Dodano tryby budowania: `3` = rozjazd (Junction), `4` = semafor (Signal).
- Rozszerzono TrackBuildMode o Junction i Signal.
- Dodano TrackCell jako reprezentację pojedynczego elementu toru na mapie.
- Dodano automatyczne logiczne łączenie nowo postawionego toru z istniejącymi sąsiadami.

### Junctions (Rozjazdy)
- Dodano rozjazdy jako element toru z możliwością przełączania kierunku.
- Dodano JunctionType definiujący 8 typów rozjazdów.
- Dodano przełączanie rozjazdu między prostym a odchylonym torem (SwitchPosition: Straight, Diverging).
- Dodano interakcję z rozjazdami: kliknięcie prawym przyciskiem myszy otwiera radialne menu wyboru typu rozjazdu.
- Dodano możliwość przełączania rozjazdu klawiszem J.
- Rozjazdy renderowane z aktywną iglicą w kolorze pomarańczowym (skręt) lub zielonym (prosto).
- Dodano JunctionRadialMenu jako okrągłe menu do wyboru typu rozjazdu.

### Signals (Semafor)
- Dodano pełny system semaforów kolejowych z 10 aspektami:
  - S1a (Stop) — Stój, przejazd zabroniony
  - S1b (StopStation) — Stój (stacja), przejazd zabroniony
  - S2 (Clear) — Jazda z Vmax, droga wolna
  - S5 (Warning) — Ostrzeżenie, następny semafor stój
  - S6 (Speed100) — Jazda ≤ 100 km/h
  - S10 (Speed40) — Jazda ≤ 40 km/h
  - S12-S15 (Reserve1-4) — Rezerwy
- Dodano SignalController do zarządzania semaforami na mapie.
- Dodano SignalRenderer do rysowania semaforów z kolorami zależnymi od aspektu.
- Dodano SignalRadialMenu — okrągłe menu do wyboru aspektu semafora (po kliknięciu prawym przyciskiem).
- Dodano SignalDirectionMenu — menu wyboru kierunku przy stawianiu semafora (gdy tor ma więcej niż jedno połączenie).
- Dodano SignalSelectionMenu — menu wyboru semafora gdy na jednej pozycji jest ich więcej.
- Dodano obsługę klawisza J do szybkiego przełączania semafora między Stop a Clear.
- Stawianie semafora w trybie Signal (klawisz 4) na istniejącym torze.
- Semafor przechowuje dostępne aspekty (AvailableAspects) dla danego egzemplarza.
- Dodano możliwość rozszerzania aspektów przez modyfikację SignalAspects.cs.

### UI / Input
- Dodano InputManager jako centralną obsługę wejścia (mysz, klawiatura).
- Dodano obsługę trybów budowania za pomocą klawiszy numerycznych (1-4).
- Dodano radialne menu dla rozjazdów (JunctionRadialMenu) i semaforów (SignalRadialMenu).
- Dodano czcionkę Arial24 dla menu.

### Controls
- `1` — tor prosty
- `2` — zakręt
- `3` — rozjazd
- `4` — semafor
- `H` — orientacja pozioma (tor prosty)
- `V` — orientacja pionowa (tor prosty)
- `R` — obrót zakrętu / zmiana typu rozjazdu
- `J` — przełącz rozjazd / przełącz semafor (Stop ↔ Clear)
- `LPM` — postaw element toru / semafor
- `PPM` — usuń element toru / otwórz menu rozjazdu lub semafora
- `MMB` — przesuwanie kamery
- `Kółko myszy` — zoom mapy
- `Escape` — zamknij menu radialne

### Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK
- Dodano wszystkie nowe klasy semaforów i rozjazdów.

### Known Gaps / Poza zakresem tego wpisu
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

---

## [0.0.4] — Podstawowy system torów i pociągów
**Data:** 2026-08-20

### Railway / Track Building
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

### Train System
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

---

## [0.0.3] — Mapa i kamera
**Data:** 2026-08-15

### Map System
- Dodano GameMap z siatką terenu.
- Dodano MapPosition, MapCell, TerrainType.
- Dodano MapRenderer renderujący widoczny fragment mapy.

### Camera System
- Dodano sterowanie kamerą: przesuwanie środkowym przyciskiem myszy.
- Dodano zoom kółkiem myszy.

### Controls
- `MMB` — przesuwanie kamery
- `Kółko myszy` — zoom mapy

### Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK

---

## [0.0.2] — Podstawowa infrastruktura
**Data:** 2026-08-10

### Infrastructure
- Utworzono repozytorium GitHub.
- Skonfigurowano projekt w Visual Studio (.NET 9.0, MonoGame).
- Utworzono strukturę rozwiązania.
- Dodano podstawowe projekty:
  - RailDispatchMono.Core
  - RailDispatchMono.DesktopGL

### Build
- RailDispatchMono.Core — build OK
- RailDispatchMono.DesktopGL — build OK

---

## [0.0.1] — Dokumentacja
**Data:** 2026-08-05

### Documentation
- Dodano dokumentację:
  - VISION.md — wizja projektu
  - GAMEPLAY.md — mechaniki gry
  - RAILWAY.md — system torów
  - TRAIN_SYSTEM.md — system pociągów
  - SCHEDULES.md — rozkłady jazdy
  - PASSENGERS.md — pasażerowie
  - MAP.md — mapa i teren
  - UI.md — interfejs użytkownika
  - ARCHITECTURE.md — architektura oprogramowania
  - DATA_MODEL.md — model danych
  - TECHNICAL.md — szczegóły techniczne
  - DEVELOPMENT.md — proces rozwoju
  - ROADMAP.md — plan rozwoju

---

## [0.0.0] — Fundament projektu
**Data:** 2026-08-01

### Initialization
- Inicjalizacja repozytorium.
- Przygotowanie struktury projektu.
- Określenie celów i zakresu projektu.
