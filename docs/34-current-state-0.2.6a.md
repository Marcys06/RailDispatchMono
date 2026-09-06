# Current state — 0.2.6a

**Data:** 2026-09-06

## Cel wydania

0.2.6a jest poprawką UX i zgodności dokumentacji po 0.2.6. Przywraca pełną dostępność funkcji opisanych dla F11 i porządkuje ekran tak, aby żadna grupa operacji nie była tracona przez szerokość lub wysokość pojedynczego wiersza.

## F11 — kompletny edytor nazwanych linii

`MyraRailwayLineView` jest modalnym ekranem zarządzania `RailwayLine`. Wszystkie funkcje pozostają dostępne:

- podanie nazwy i utworzenie linii z aktualnego zaznaczenia;
- wybór istniejącej linii;
- zmiana nazwy wybranej linii;
- zaznaczenie torów wybranej linii na mapie;
- usunięcie nazwanej linii bez usuwania fizycznego toru;
- dodanie aktualnego zaznaczenia do wybranej linii;
- usunięcie aktualnego zaznaczenia z wybranej linii;
- masowa zmiana `TrackType` na zaznaczeniu albo całej wybranej linii;
- masowa zmiana `LineClass` na zaznaczeniu albo całej wybranej linii;
- masowa zmiana `TractionSystem` na zaznaczeniu albo całej wybranej linii;
- wybór całego połączonego obszaru istniejących torów;
- wyczyszczenie aktualnego zaznaczenia;
- lista wszystkich zapisanych linii z operacjami na każdej pozycji.

### Dostępność UI

Poprzedni układ umieszczał zbyt wiele akcji w poziomych wierszach, a listę linii w wysokim obszarze. Na części rozdzielczości dolne funkcje były poza użytecznym obszarem ekranu.

W 0.2.6a:

- cały ekran F11 ma przewijalną zawartość;
- lista zapisanych linii ma własny obszar przewijania;
- operacje wyboru obszaru, członkostwa i masowych zmian są rozdzielone na osobne sekcje;
- `TrackType`, `LineClass` i `TractionSystem` mają niezależny wybór oraz przyciski zastosowania do zaznaczenia i linii;
- status operacji pozostaje widoczny pod listą;
- przyciski nie są usuwane ani zastępowane automatyzacją.

## Zasady danych

`RailwayLineManager` pozostaje właścicielem grup linii i operacji masowych. `RailwayLine` jest wyłącznie organizacyjnym zbiorem segmentów toru. Linie nie zastępują bloków, semaforów, zwrotnic, tras ani rozkładów jazdy.

Masowa zmiana infrastruktury wywołuje istniejące `TrackCell.SetInfrastructure`. Nie zmienia geometrii toru, zajętości bloków ani logiki ruchu pociągów.

## Zaznaczenie

F11 korzysta z selekcji należącej do `InputManager`:

- LPM — nowe zaznaczenie;
- Shift+LPM — dodanie;
- Ctrl+LPM — przełączenie.

`ZAZNACZ POŁĄCZONY OBSZAR` wykorzystuje istniejącą topologię torów przez `RailwayLineManager.CollectConnectedTracks`.

## Pozostałe UI i sterowanie

- F6 — `Wymuś przejazd`;
- F8 — edycja infrastruktury pojedynczego toru;
- F9 — edycja rozkładu lokomotywy;
- F10 — diagnostyka infrastruktury i ruchu;
- F11 — kompletny edytor nazwanych linii.

Modalny lock Myra/gameplay pozostaje wymagany: kliknięcia w F11 nie mogą przechodzić do `InputManager` i oddziaływać na mapę.

## Zapis

Nazwane linie pozostają zapisywane w `map.json` schema 3: ID, nazwa, indeks koloru i pozycje torów. Usunięcie fizycznego toru usuwa jego pozycję z linii.

## Zakres poza wydaniem

0.2.6a nie dodaje ekonomii, automatycznego ustawiania zwrotnic/semaforów, automatycznej naprawy tras ani przebudowy fizyki ruchu.

## Weryfikacja

Nie wykonano kompilacji Windows ani live gameplay w tym środowisku. Zmiana została wykonana statycznie na podstawie bieżącego `MyraRailwayLineView`, `RailwayLineManager` i dokumentacji 0.2.5/0.2.6. Lokalnie należy sprawdzić przewijanie F11, wszystkie akcje masowe, członkostwo linii, listę linii oraz brak przepuszczania kliknięć do świata.
