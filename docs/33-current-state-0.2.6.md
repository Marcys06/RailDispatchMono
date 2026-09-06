# Current state — 0.2.6

## Cel wydania

0.2.6 porządkuje HUD gameplayu i usuwa niejednoznaczność między budowaniem infrastruktury a zaznaczaniem torów.

## Tryb zaznaczania

HUD ma teraz jawny przycisk `ZAZNACZANIE / BRAK TRYBU [0]`. Kliknięcie ustawia `TrackBuildMode.None`. W tym trybie:

- LPM na istniejącym torze — nowa selekcja;
- Shift+LPM — dodanie toru;
- Ctrl+LPM — przełączenie toru.

F11 wykorzystuje bieżące zaznaczenie do zarządzania `RailwayLine`.

## Nowy HUD

HUD jest podzielony na cztery funkcjonalne obszary:

- górny pasek: zegar, dzień, prędkość symulacji, stan infrastruktury;
- lewy panel: tryb zaznaczania i narzędzia budowy;
- środek: wybrany pociąg, dispatcher, timetable i akcje operacyjne;
- prawy panel: ruch, pociągi i stacje;
- dolny panel: wagony/pasażerowie i skróty.

Przyciski budowy są schowane pod `NARZĘDZIA BUDOWY`, dzięki czemu tryb zaznaczania jest domyślnym, widocznym działaniem zamiast niejawnego `None`.

## Zasady operacyjne

HUD nie przejmuje decyzji za gracza. Nie ustawia zwrotnic ani semaforów automatycznie. F6 nadal jest wymuszeniem przejazdu, a RadioStop pozostaje nadrzędny.

## Ochrona wejścia

Tymczasowe ekrany Myra pozostają modalne względem świata. Podczas GUI `InputManager.Update()` nie powinien przetwarzać wejścia mapy. Reset `TrackBuilder.Mode` przy otwieraniu F8/F9/F10/F11 pozostaje aktywny.

## Weryfikacja

Nie wykonano Windows build ani live gameplay w tym środowisku. Po pobraniu należy sprawdzić przycisk zaznaczania, Shift/Ctrl selection, F11, wszystkie przyciski HUD oraz brak budowy toru przy obsłudze GUI.
