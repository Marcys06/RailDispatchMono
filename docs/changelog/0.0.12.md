# [0.0.12] — Zegar symulacji, panel obiektów, Depot i diagnostyka prędkości
**Data:** 2026-08-31

## Simulation Clock
- Dodano 24-godzinny zegar symulacji `00:00–23:59`.
- Zegar jest oparty o `GameTime` i nie zależy od liczby klatek.
- Dodano prędkości symulacji `x1`, `x2`, `x5`.
- Przyspieszenie czasu wpływa na całą pętlę symulacji pociągów i stacji.
- Istniejący system pauzy zatrzymuje zegar i symulację.

## Object Panel
- Dodano boczny panel z zakładkami `POCIĄGI` i `STACJE`.
- Lista pociągów pokazuje skrócone ID i bieżącą prędkość.
- Lista stacji pokazuje nazwę oraz liczbę oczekujących pasażerów.
- Kliknięcie pociągu centruje kamerę na jego pozycji.
- Kliknięcie stacji centruje kamerę na środku jej obszaru.

## Depot / Train Spawning
- Dodano przycisk `DEPOT` otwierający panel wyboru składu.
- Pierwsza wersja udostępnia domyślny skład: lokomotywa + 2 wagony.
- Po wybraniu składu użytkownik wskazuje istniejący tor na mapie, na którym pociąg zostaje utworzony.
- Model spawnowania pozostaje niezależny od przyszłego systemu tras z `0.0.13`.

## Train Speed UI
- Dodano `Train.TargetSpeed` jako bieżącą prędkość docelową wynikającą z następnego semafora.
- Dodano `Train.MaxSpeed` jako Vmax całego składu.
- Vmax składu jest ograniczone przez najwolniejszy pojazd.
- Dodano `Train.EffectiveTargetSpeed`, czyli minimum pomiędzy ograniczeniem infrastruktury a Vmax składu.
- Tooltip pociągu pokazuje bieżącą prędkość, docelową prędkość oraz Vmax składu.

## FloatingText
- Dodano programowy system `FloatingTextManager`.
- Powiadomienia są renderowane bez dodatkowych assetów graficznych.
- Tekst jest przypisany do konkretnego wagonu.
- Powiadomienie unosi się i płynnie zanika przez fade-out.
- Format zmian liczby pasażerów: `+X` dla wsiadających oraz `-X` dla wysiadających.

## Architecture
- Zegar został wydzielony do `GameClock`, aby późniejsze systemy rozkładów jazdy mogły korzystać z jednego źródła czasu symulacji.
- Właściwości prędkości pociągu zostały wystawione niezależnie od UI, przygotowując model pod przyszłe trasy, coupling/decoupling oraz ograniczenia odcinkowe.
- Depot jest punktem wejścia do przyszłego systemu tworzenia i przypisywania tras.

## Build
- Zmiany zapisane w repozytorium z oznaczeniem `0.0.12`.

## Known Gaps / Poza zakresem
- Pełny system tras pociągów — planowany na `0.0.13`.
- Rozkłady jazdy — planowane na `0.0.12` jako kolejny etap prac.
- Pełne rozdzielenie komunikatów `+X` i `-X` na poziomie pojedynczej operacji pasażerskiej wymaga dalszego spięcia z `IPassengerService`.
