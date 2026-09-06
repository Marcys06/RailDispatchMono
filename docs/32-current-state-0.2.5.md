# Current state — 0.2.5

## Cel wydania

0.2.5 zamienia metadane infrastruktury z 0.2.4 w czytelną warstwę operacyjną i wizualną. Nadal nie dodaje ekonomii ani pełnej fizyki infrastruktury.

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
- masową zmianę `TrackType`, `LineClass` i `TractionSystem`;
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

## Zapis

Named lines są częścią `map.json` od schematu `3`. Zapisy przechowują ID, nazwę, indeks koloru i listę pozycji torów. Usunięcie fizycznego toru automatycznie usuwa jego pozycję ze wszystkich linii.

Stare schematy map nie są obecnie obsługiwane — zgodnie z zasadą braku wymogu kompatybilności w tych wersjach rozwojowych.

## Profil rozgrywki

Pozostaje wariant B: przystępna symulacja. System daje graczowi narzędzia do szybkiego oznaczania i przebudowy parametrów infrastruktury, ale nie naprawia automatycznie tras ani nie podejmuje za gracza decyzji eksploatacyjnych.

## Roadmapa

0.2.5 przygotowuje UX pod 0.3.0: przyszłe utrzymanie i zużycie mogą działać na pojedynczym segmencie albo na całej grupie `RailwayLine`. Ekonomia pozostaje odroczona do 0.4.0.

## Weryfikacja

Nie wykonano kompilacji Windows ani testu live gameplay. Po pobraniu należy sprawdzić F11, wielokrotne zaznaczanie, kontury linii, zapis/odczyt schematu 3 oraz zachowanie istniejących bloków, sygnałów, zwrotnic i F6.
