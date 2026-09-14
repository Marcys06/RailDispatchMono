# Current state — 0.3.1

**Data:** 2026-09-14

## Cel wydania

0.3.1 rozwija 0.3.0 z prostego licznika zużycia w aktywny model degradacji infrastruktury. Nadal wykorzystywany jest istniejący `TrackCell.WearPercent` i schema 3 zapisu mapy.

## Degradacja

`InfrastructureMaintenanceManager`:

- nadal używa czasu symulacji;
- rozróżnia tor zajęty przez pociąg i nieużywany;
- zwiększa tempo degradacji wraz ze wzrostem zużycia;
- wprowadza stan `Severe` od 95% zużycia jako rozszerzenie diagnostyczne krytycznego stanu;
- udostępnia naprawę wszystkich, krytycznych i ciężko zużytych odcinków;
- udostępnia listy odcinków według stanu;
- wylicza priorytet inspekcji z uwzględnieniem zużycia, klasy linii, roli toru i zwrotnicy;
- udostępnia najbardziej pilny odcinek.

## Zalecenia eksploatacyjne

`TrackCell` udostępnia teraz pochodne informacje operacyjne:

- `MaintenanceState`;
- `RequiresInspection`;
- `IsCriticalForMaintenance`;
- `IsSeverelyWorn`;
- `MaintenanceSpeedMultiplier`;
- `RecommendedOperationalSpeedKmh`;
- `RecommendedOperationalAxleLoadTons`.

Są to ograniczenia i zalecenia domenowe. Ten release nie wymusza jeszcze automatycznej zmiany prędkości pociągów ani automatycznego zamykania toru.

## Progi

| Zużycie | Stan | Zalecenie prędkości | Zalecenie nacisku osi |
|---:|---|---:|---:|
| 0–59.99% | Good | 100% | 100% |
| 60–84.99% | Warning | 85% | 95% |
| 85–94.99% | Critical | 65% | 85% |
| 95–100% | Severe | 50% | 70% |

Tempo degradacji wynosi odpowiednio 1.0x, 1.2x, 1.7x i 2.5x względem bazowego zużycia.

## Zapis

`MapSaveService` zapisuje mapę jako `0.3.1`, ale nadal używa schema 3. Istniejące zapisy 0.3.0 pozostają kompatybilne, ponieważ format torów nie został zmieniony.

## Granice architektury

Utrzymanie nadal nie przejmuje odpowiedzialności za dispatcher, bloki, sygnały, zwrotnice, rozkład jazdy ani pasażerów. Jest źródłem stanu infrastruktury i zaleceń dla kolejnych warstw symulacji.

## Następny krok

0.3.x powinno wykorzystać te zalecenia do rzeczywistych ograniczeń ruchowych oraz dodać planowanie prac utrzymaniowych przed wejściem w ekonomię 0.4.0.

## Weryfikacja

Zmiany zostały przeprowadzone statycznie względem aktualnych właścicieli stanu. Nie wykonano w tym środowisku natywnego buildu Windows ani live gameplay. Należy zweryfikować kompilację, narastanie zużycia, zapis/odczyt schema 3 oraz działanie F10.
