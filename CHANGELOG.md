# Changelog

This file is the high-level release history. Detailed release notes are kept in `docs/changelog/`.

## [0.0.16] — Save slots, Main Menu i runtime persistence
**Data:** 2026-08-31

- Dodano katalogi zapisów z `metadata.json`.
- Save składa się z rozdzielonych plików `map.json`, `trains.json`, `schedules.json`, `passengers.json` i `economy.json`.
- Dodano wersjonowanie danych save (`schemaVersion`).
- Zapis obejmuje pociągi, składy, parametry pojazdów, pozycje, trasy wagonów, pasażerów znajdujących się w wagonach oraz `GameDay`/`GameTime`.
- Dodano Main Menu: `NOWA GRA`, `WCZYTAJ GRĘ`, `USTAWIENIA`, `O GRZE`, `WYJDŹ`.
- Nowa gra tworzy pusty slot; Load wybiera zapis bez dodatkowego potwierdzenia.
- Niepoprawny lub niekompletny zapis jest odrzucany komunikatem zamiast cichego ładowania.
- Zapis rozkładów został przeniesiony do aktywnego slotu.
- Auto-save nie jest jeszcze włączony.
- Istniejący `DepotTrainMenu` pozostaje punktem tworzenia wielu pociągów z jednego depot.

## [0.0.15f] — Stabilizacja systemu pauzy i zarządzania ekranami
**Data:** 2026-08-31

- Naprawiono logikę przełączania pauzy w `GameplayScreen`.
- Dodano uporządkowane `TogglePause()` i ochronę przed wielokrotnym tworzeniem `PauseScreen`.
- Poprawiono obsługę `IsPopup` oraz zarządzanie ekranami przez `ScreenManager`.
- Naprawiono błędy `NullReferenceException` podczas rysowania menu pauzy.
- Poprawiono ładowanie czcionek używanych przez ekran pauzy.
- Dodano diagnostykę przejść stanu pauzy.
- `ESC` poprawnie rozpoczyna i kończy pauzę.
- Menu pauzy jest wyświetlane jako overlay nad rozgrywką.

## [0.0.15d] — Naprawy systemu pauzy
**Data:** 2026-08-31

- Poprawiono logikę pauzy i zarządzanie `PauseScreen`.
- Usunięto możliwość niekontrolowanego dokładania kolejnych ekranów pauzy.
- Poprawiono obsługę overlay oraz przejść ekranu.
- Dodano zabezpieczenia związane z teksturą overlay.

## [0.0.15c] — Save/Load w menu pauzy
**Data:** 2026-08-31

- Przeniesiono `SAVE` i `LOAD` z głównego HUD do menu pauzy.
- Zachowano skróty `F6` i `F7`.
- Uporządkowano `GameplayScreen.cs` po wcześniejszym problemie z formatowaniem klasy w jednej linii.
- Zintegrowano zapis mapy z `DepotController` używanym przez gameplay.

## [0.0.15b] — Map Save / Load
**Data:** 2026-08-31

- Dodano zapis i odczyt infrastruktury mapy przez `map.json`.
- Zapisywane są tory, geometria, połączenia, rozjazdy, semafory, stacje i depoty.
- Format zapisu posiada `schemaVersion`.
- Odczyt odbudowuje obiekty infrastruktury z DTO.
- Zachowywane są stabilne ID stacji, semaforów i depotów.
- Po załadowaniu odbudowywane są bloki torowe.
- Stan pociągów i pasażerów nie jest jeszcze częścią `map.json`.

## [0.0.15a] — Fundament persistence
**Data:** 2026-08-31

- Rozpoczęto system wieloplikowego zapisu stanu gry w JSON.
- Przyjęto rozdzielenie danych na przyszłe pliki: mapa, pociągi i wagony, rozkłady, pozostały stan oraz ekonomia.
- Pierwszym rzeczywiście zaimplementowanym obszarem persistence jest infrastruktura mapy.
- Ekonomia pozostaje przygotowana jako pusty obszar przyszłego zapisu.

## [0.0.15] — Save System
**Data:** 2026-08-31

- Rozpoczęto prace nad persistence stanu gry.
- Przyjęto JSON jako format zapisu.
- Zdefiniowano rozdzielenie danych na osobne obszary zapisu przed przyszłym scaleniem ich w jeden system save.

## [0.0.14c] — Stabilizacja edycji tras wagonów
**Data:** 2026-08-31

- Naprawiono zamykanie menu trasy wagonu w pierwszym `Update()` po otwarciu.
- Pierwszy update po otwarciu menu jest konsumowany.
- Kolejne nowe kliknięcie LPM wykonuje akcję lub zamyka menu.
- PPM zamyka menu.
- Kliknięcia przycisków stacji są obsługiwane niezależnie.
- Dodano aktywny przycisk `S` jako wizualne oznaczenie trybu edycji trasy.
- Zmiana trasy jest zapisywana przez istniejący `ScheduleStorage`.

## [0.0.14b] — Naprawa menu trasy wagonu
**Data:** 2026-08-31

- Naprawiono natychmiastowe zamykanie `WagonRouteMenu` po `S` + LPM.
- Poprawiono obsługę przycisków stacji.
- Potwierdzono budowanie stacji przez `TrackBuildMode.Station`.
- Uporządkowano zapis tras JSON.

## [0.0.14a] — Trwałe trasy wagonów
**Data:** 2026-08-31

- Przeniesiono edycję trasy wagonu do trybu `S` (Schedule).
- Dodano `TrainSchedule` i `ScheduleStorage`.
- Rozkłady są zapisywane jako JSON w katalogu `schedules`.
- Dodano wybór zestawienia pociągu przy Depot.
- Przygotowano format JSON pod przyszłe rozszerzenia rozkładów.

## [0.0.14] — Podstawowy system kolizji
**Data:** 2026-08-31

- Dodano `TrainCollisionController`.
- Pociąg wykrywa inne składy na wybranej drodze torowej.
- Wprowadzono 2-komórkowy odstęp bezpieczeństwa.
- Dodano awaryjne zatrzymanie `RadioStop`.
- Dodano ochronę miejsca spawnowania całego składu.
- Ustalono priorytet: semafor → kolizja → stacja.

## [0.0.13pre] — Debug, zegar i porządkowanie dokumentacji
**Data:** 2026-08-31

- Wprowadzono globalne ograniczenie logów debugowania.
- Ustalono skalowanie czasu symulacji i zachowanie pauzy.
- Ujednolicono dokumentację w jednym katalogu `docs/`.
- Dodano `docs/changelog/` jako miejsce szczegółowych changelogów.

## [0.0.13] — Trasy wagonów
**Data:** 2026-08-31

- Dodano `TrainRoute` przypisany do konkretnego wagonu.
- Trasa przechowuje uporządkowaną listę stacji i aktualny punkt.
- Dodano ograniczanie przyjmowania pasażerów zgodnie z trasą wagonu.
- Dodano edytor trasy wagonu i rozszerzony tooltip.
- Przygotowano model trasy do serializacji JSON.
- Trasa wagonu nie steruje bezpośrednio ruchem lokomotywy.

## [0.0.12c] — Zegar, Depot i kompletne orientacje rozjazdów
**Data:** 2026-08-31

- Naprawiono aktualizację zegara symulacji.
- Poprawiono interakcję z Depot i tworzenie składu.
- Rozszerzono rozjazdy do 12 orientacji.
- Zaktualizowano podgląd i radialne menu rozjazdów.

## [0.0.12b] — HUD i prędkość symulacji
**Data:** 2026-08-31

- Poprawiono przyciski `x1`, `x2`, `x5`.
- Uporządkowano HUD i panel obiektów.
- Depot pozostał elementem świata zamiast elementem HUD.

## [0.0.12a] — Depot, UI i okno desktopowe
**Data:** 2026-08-31

- Dodano Depot jako budynek świata i `DepotController`.
- Dodano programowy renderer Depot.
- Dodano tryb budowy Depot (`9` / NumPad `9`).
- Zwiększono domyślne okno desktopowe do `1600x900`.
- Dodano zmianę rozmiaru okna i zapis ustawienia.

## [0.0.12] — Zegar symulacji i panel obiektów
**Data:** 2026-08-31

- Dodano 24-godzinny `GameClock`.
- Dodano prędkości symulacji `x1`, `x2`, `x5`.
- Dodano panel obiektów z listą pociągów i stacji.
- Dodano podstawowy system tworzenia składów z Depot.
- Rozszerzono informacje o prędkości pociągu.
- Dodano programowy `FloatingTextManager`.

## [0.0.11] — Przebudowa systemu stacji i pasażerów
**Data:** 2026-08-31

- Rozdzielono decyzję o postoju od obsługi pasażerów.
- Dodano `ITrainStopDecision` i `IPassengerService`.
- Rozszerzono model pasażera o bieżącą stację i obsługę wagonową.
- Dodano automatyczne generowanie popytu pasażerskiego.
- Rozszerzono tooltip wagonu o dane pasażerskie i `ServiceRoute`.

## [0.0.10] — Stacje i pasażerowie
**Data:** 2026-08-31

- Dodano model i kontroler stacji.
- Dodano model pasażera i `PassengerManager`.
- Dodano typy wagonów, pojemność pasażerską i obsługę pasażerów wagon po wagonie.
- Dodano przygotowanie `ServiceRoute` pod przyszłe rozdzielanie tras wagonów.
- Naprawiono odliczanie cooldownu bloków i resetowanie semaforów.

## [0.0.9] — Look-ahead semaforów i hamowanie
**Data:** 2026-08-30

- Rozszerzono wykrywanie następnego semafora o kolejne komórki toru.
- Dodano rzeczywistą odległość do sygnału.
- Dodano bezpieczną prędkość i drogę hamowania.
- Pociąg może rozpocząć fizyczne hamowanie przed semaforem STOP.
- Look-ahead obsługuje również przejście przez zakręty.

## [0.0.8b] — Naprawa menu pauzy
**Data:** 2026-08-30

- Poprawiono wyświetlanie i obsługę menu pauzy.
- Dodano diagnostykę `MenuScreen` i `PauseScreen`.
- Uporządkowano obsługę wejścia dla pauzy.

## [0.0.8a] — System pauzy i menu ekranowego
**Data:** 2026-08-30

- Dodano `PauseScreen`.
- Dodano zatrzymywanie symulacji podczas pauzy.
- Rozszerzono `MenuScreen` i `MenuEntry`.
- Dodano nawigację po menu pauzy.

## [0.0.8] — Tooltip pociągu
**Data:** 2026-08-30

- Dodano tooltip pojazdu/pociągu.
- Dodano wykrywanie pojazdu pod kursorem.
- Tooltip pokazuje podstawowe dane składu i pojazdu.

## [0.0.7c] — Debug i automatyczny reset semaforów
**Data:** 2026-08-30

- Dodano centralny `DebugManager`.
- Dodano kategorie logowania i zapis logów do pliku.
- Dodano automatyczny reset semaforów po opuszczeniu bloku.
- Dodano cooldown bloku i śledzenie bloków przez pociąg.

## [0.0.7b] — Refaktoryzacja Train
**Data:** 2026-08-30

- Podzielono `Train` na `Train.cs`, `TrainMovement.cs` i `TrainGeometry.cs`.
- Poprawiono API ruchu, geometrię zakrętów i metody pomocnicze.

## [0.0.7a] — Naprawa połączeń torów prostych
**Data:** 2026-08-29

- Naprawiono `GetExitDirection` dla torów prostych.
- Poprawiono przechodzenie pociągu do kolejnych komórek.
- Dodano diagnostykę połączeń torowych.

## [0.0.7] — Obsługa rozjazdów przez pociągi
**Data:** 2026-08-29

- Dodano przejazd pociągu przez rozjazdy.
- Pociąg uwzględnia `SwitchPosition`.
- Dodano obsługę jazdy prosto i po odchyleniu.
- Rozszerzono `TrackCell` o informacje o rozjeździe.

## [0.0.6b] — Diagnostyka ruchu i prędkości
**Data:** 2026-08-29

- Rozszerzono logowanie prędkości, przyspieszania i hamowania.
- Dodano diagnostykę ruchu po zakrętach.
- Rozszerzono `TrainDebugger`.

## [0.0.6] — Semafory i rozjazdy w ruchu pociągów
**Data:** 2026-08-29

- Zintegrowano semafory z ruchem pociągów.
- Dodano płynne przyspieszanie i hamowanie zależne od aspektu.
- Dodano parametry hamowania pojazdów.
- Dodano podstawową integrację rozjazdów i semaforów z `TrainManager`.

## [0.0.5] — System semaforów i rozjazdów
**Data:** 2026-08-29

- Dodano rozjazdy i ich radialne menu.
- Dodano system semaforów z aspektami.
- Rozszerzono tryby budowania infrastruktury.
- Dodano podstawową integrację sygnalizacji z torami.

## [0.0.2] — Mapa

- Dodano podstawowy model `GameMap` i komórki mapy.
- Dodano typy terenu i renderowanie mapy.
- Dodano kamerę mapy.

## [0.0.0] — Fundament projektu

- Utworzono repozytorium i strukturę projektu.
- Skonfigurowano rozwiązanie .NET/MonoGame.
- Utworzono podstawową strukturę domeny, symulacji, UI i dokumentacji.
