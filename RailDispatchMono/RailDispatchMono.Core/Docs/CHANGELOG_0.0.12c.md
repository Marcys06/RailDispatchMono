# [0.0.12c] — Naprawa zegara, Depot i kompletne orientacje rozjazdów
Data: 2026-08-31

## Game Clock

- Naprawiono aktualizację zegara gry — czas jest aktualizowany w każdej aktywnej klatce symulacji.
- Kliknięcie x1/x2/x5 nie zatrzymuje już aktualizacji zegara w danej klatce.
- Zegar nadal korzysta z `GameClock` i skaluje czas przez x1, x2 oraz x5.
- Pauza nadal zatrzymuje zegar razem z symulacją.

## Depot

- Naprawiono otwieranie panelu Depot po kliknięciu wcześniej postawionego budynku.
- Kliknięcie Depot otwiera teraz okno wyboru domyślnego składu.
- `WYBIERZ I USTAW` przełącza w tryb wyboru miejsca na torze.
- Po wskazaniu toru tworzony jest domyślny skład: lokomotywa + 2 wagony.
- `9` / NumPad `9` nadal służy do budowania nowych Depotów.

## Junctions / Rozjazdy

- Rozszerzono model z 8 do 12 możliwych orientacji rozjazdu.
- Obsługiwane są wszystkie warianty wspólnego ramienia z trzema możliwymi parami wyjść, m.in. `S->EW`, `N->EW`, `E->NS`, `W->NS`.
- Zaktualizowano budowanie rozjazdów.
- Zaktualizowano podgląd rozjazdu przed postawieniem.
- Zaktualizowano radialne menu rozjazdu do 12 pozycji.
- Zachowano rozdzielenie `StraightConnection`, `DivergingConnection` i `CommonStem`, dzięki czemu logika przejazdu pociągu pozostaje niezależna od orientacji.

## UI

- Radialne menu rozjazdów zostało powiększone i otrzymało mniejsze etykiety, aby 12 wariantów było czytelnych.

## Build

- Zaktualizowano kod przygotowany pod build `RailDispatchMono.Core` i `RailDispatchMono.DesktopGL`.
