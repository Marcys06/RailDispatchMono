# [0.0.12b] — Poprawki HUD, Depot i prędkości symulacji

Data: 2026-08-31

## HUD / Simulation Speed

- Poprawiono obsługę przycisków `x1`, `x2` i `x5` w panelu zegara.
- Przyciski prędkości nie powinny być zasłaniane przez element Depot.
- Zmiana mnożnika wpływa na czas gry oraz całą symulację przez `GameClock`.
- Pauza nadal zatrzymuje zegar i symulację.

## Depot

- Budowanie Depot pozostaje dostępne przez klawisz `9` / NumPad `9`.
- Depot jest traktowany jako element świata, a nie element HUD.
- Usunięto założenie, że duży przycisk `DEPOT` powinien zajmować przestrzeń obok zegara; wejście do trybu budowy powinno odbywać się przez `9`.

## UI / Panel obiektów

- Zmniejszono skalę tekstu w prawym panelu listy pociągów i stacji.
- Zachowano układ dwóch zakładek `POCIĄGI` / `STACJE`.
- Zmniejszenie tekstu ma ograniczyć nachodzenie napisów przy większej liczbie informacji.

## FloatingText

- Mechanizm FloatingText pozostaje bez zmian w tej wersji.
- Rozdzielenie `-X` i `+X` na podstawie dokładnego zdarzenia wymiany pasażerskiej pozostaje zaplanowane na późniejszy etap.

## Uwagi

- `0.0.12b` jest poprawką warstwy interfejsu i sterowania istniejącą symulacją.
- Przygotowanie pod `0.0.13` (tworzenie tras) pozostaje bez zmian.
