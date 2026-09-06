# Current state — 0.2.5a

## Cel wydania

0.2.5 zamienia metadane infrastruktury z 0.2.4 w czytelną warstwę operacyjną i wizualną. 0.2.5a jest poprawką bezpieczeństwa wejścia GUI i nie rozszerza zakresu funkcjonalnego infrastruktury.

## Wizualizacja torów

- `TractionSystem.None` — czarny tor, brak elektryfikacji.
- `TractionSystem.DC` — pomarańczowo-czerwony tor.
- `TractionSystem.AC` — niebieski tor.
- `LineClass.Local` → najcieńsza linia.
- `LineClass.Regional` → cienka.
- `LineClass.Mainline` → grubsza.
- `LineClass.Magistral` → najgrubsza.
- `TrackType` dodatkowo delikatnie koryguje grubość.

Kolor bazowy oznacza trakcję, a grubość klasę linii. Nazwane linie otrzymują dodatkowy kolorowy kontur, dzięki czemu nie zasłaniają informacji o trakcji.

Aktywne zaznaczenie torów jest oznaczane białym konturem.

## Zaznaczanie wielu torów

W trybie budowy `None`:

- LPM na istniejącym torze — nowe zaznaczenie;
- Shift+LPM — dodanie toru do zaznaczenia;
- Ctrl+LPM — przełączenie toru w zaznaczeniu.

F11 otwiera edytor nazwanych linii pracujący na tym zaznaczeniu.

## Nazwane linie kolejowe

`RailwayLine` jest player-facing logiczną kolekcją segmentów toru. `RailwayLineManager` obsługuje:

- tworzenie nazwanej linii;
- wybór istniejącej linii;
- dodawanie/usuwanie zaznaczenia z linii;
- usuwanie linii bez usuwania torów;
- masową zmianę `TrackType`, `LineClass`, `TractionSystem`;
- zaznaczenie całego połączonego obszaru przez istniejącą topologię toru.

Linia nie zastępuje bloków, sygnałów, zwrotnic, tras ani rozkładów jazdy.

## Ekran F11

`MyraRailwayLineView` daje jednocześnie:

- pole nazwy i tworzenie linii z zaznaczenia;
- listę istniejących linii;
- wybór i zaznaczenie linii na mapie;
- dodawanie/usuwanie zaznaczenia z wybranej linii;
- cykliczny wybór `TrackType`, `LineClass` i `TractionSystem`;
- masową zmianę parametrów na zaznaczeniu albo całej linii;
- wybór połączonego obszaru torów.

## Ochrona wejścia podczas GUI — 0.2.5a

Ochrona ma teraz dwa poziomy:

1. **GUI zastępujące gameplay root** — `MyraUIManager.IsGameplayOverlayOpen` blokuje `InputManager.Update()` przez cały czas działania tymczasowego okna.
2. **Kliknięcie obsługujące akcję GUI** — `MyraUIManager.GameplayInputConsumedThisFrame` blokuje `InputManager.Update()` również w tej samej klatce, w której przycisk GUI wykonał akcję.

Drugi poziom jest istotny dla przycisków takich jak `Tor prosty`: wcześniej kliknięcie mogło najpierw ustawić `TrackBuildMode.Straight`, a następnie w tej samej klatce zostać ponownie odczytane przez świat jako LPM i postawić tor. W 0.2.5a ten klik jest konsumowany przez GUI.

Dodatkowo poprawiono cykl rootów Myra. Powrót z tymczasowego GUI do głównego rootu gameplayu czyści stan overlay zamiast zapisywać zamykany ekran jako kolejny `_previousRoot`. Dzięki temu po zamknięciu F8/F9/F10/F11 świat ponownie przyjmuje wejście.

F8/F9/F10/F11 nadal resetują `TrackBuilder.Mode` do `None` przed otwarciem GUI. Zamknięcie okna nie pozostawia uzbrojonego trybu budowy.

## Zapis

Named lines są częścią `map.json` od schematu `3`. Zapisy przechowują ID, nazwę, indeks koloru i listę pozycji torów. Usunięcie fizycznego toru automatycznie usuwa jego pozycję ze wszystkich linii.

Stare schematy map nie są obecnie obsługiwane — zgodnie z zasadą braku wymogu kompatybilności w tych wersjach rozwojowych.

## Profil rozgrywki

Pozostaje wariant B: przystępna symulacja. System daje graczowi narzędzia do szybkiego oznaczania i przebudowy parametrów infrastruktury, ale nie naprawia automatycznie tras ani nie podejmuje za gracza decyzji eksploatacyjnych.

## Roadmapa

0.2.5 przygotowuje UX pod 0.3.0: przyszłe utrzymanie i zużycie mogą działać na pojedynczym segmencie albo na całej grupie `RailwayLine`. Ekonomia pozostaje odroczona do 0.4.0.

## Weryfikacja

Nie wykonano kompilacji Windows ani testu live gameplay. Kod został sprawdzony statycznie pod kątem cyklu rootów Myra i konsumpcji akcji GUI; kompilacja/runtime nadal wymaga sprawdzenia lokalnie.
