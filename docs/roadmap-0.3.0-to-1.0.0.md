# Roadmap 0.3.0 → 1.0.0

## Zasada nadrzędna

Rozwój ma dodawać kolejne warstwy bez rozbijania istniejących właścicieli stanu. Dispatcher, bloki, sygnały, zwrotnice, rozkład lokomotywy, `Wagon` jako właściciel pasażera i istniejący model ruchu pozostają punktami integracji.

Profil docelowy to **B — przystępna symulacja**: zależności mają być wiarygodne i czytelne, ale liczba ręcznie obsługiwanych parametrów ma pozostać kontrolowana.

## 0.3.0 — Zarządzanie infrastrukturą

**Stan: aktywowane 2026-09-14.**

- zużycie torów napędzane czasem symulacji;
- szybsze zużycie odcinków aktualnie używanych przez pociągi;
- współczynniki `LineClass`, `TrackType` i `TractionSystem`;
- diagnostyka F10 i naprawy;
- istniejący `TrackCell.WearPercent` oraz schema 3 zapisu.

## 0.3.1 — Degradacja i priorytety utrzymania

**Stan: zaimplementowana 2026-09-14.**

- przyspieszona degradacja zużytych odcinków;
- stan `Severe` od 95%;
- priorytet inspekcji zależny od zużycia i znaczenia odcinka;
- filtrowanie infrastruktury według stanu;
- identyfikacja najbardziej pilnego odcinka;
- zalecane ograniczenia prędkości i nacisku osi;
- naprawy `Critical` i `Severe`;
- brak zmiany schema 3 i kompatybilność zapisów 0.3.0.

### Pozostałe 0.3.x

- egzekwowanie ograniczeń eksploatacyjnych w ruchu pociągów;
- planowane okna prac utrzymaniowych;
- koszty utrzymania infrastruktury;
- modernizacje i wymiana infrastruktury;
- rozszerzenie utrzymania na urządzenia infrastruktury.

Poza zakresem: pełna ekonomia przedsiębiorstwa.

## 0.4.0 — Ekonomia

**Cel:** budżet przedsiębiorstwa kolejowego.

Zakres:

- przychody z przewozów;
- koszty eksploatacji i utrzymania;
- budżet;
- inwestycje;
- finansowanie rozwoju infrastruktury;
- konsekwencje decyzji inwestycyjnych.

## 0.5.0 — Rozkład jazdy

**Cel:** publiczny plan przewozów i koordynacja kursów.

## 0.6.0 — Symulacja pasażerska

**Cel:** popyt, wybór połączeń, oczekiwanie, przesiadki i zadowolenie.

## 0.7.0 — Fizyka

**Cel:** krzywizny, gradienty, osiągi, hamowanie i odzysk energii.

## 0.8.0 — Kryzysy

**Cel:** awarie infrastruktury i taboru, pogoda, ograniczenia ruchowe i zarządzanie kryzysowe.

## 0.9.0 — Sieć

**Cel:** miasta, stacje węzłowe, długie trasy, przesiadki i przepustowość sieci.

## 1.0.0 — Full release

**Cel:** pełna integracja, balans, onboarding, kampania/scenariusze, stabilny zapis i aktualna dokumentacja.

## Kolejność zależności

```text
0.2.4 infrastruktura danych
        |
        v
0.3.0 zużycie i utrzymanie
        |
        v
0.3.1 degradacja i ograniczenia
        |
        +----> 0.4.0 ekonomia
        |
        +----> 0.7.0 fizyka
        |
        +----> 0.8.0 kryzysy
        |
        v
0.5.0 publiczny rozkład
        |
        v
0.6.0 pasażerowie
        |
        v
0.9.0 sieć
        |
        v
1.0.0 integracja
```

Każda wersja kończy się aktualizacją snapshotu, changelogu i dokumentacji bieżącego kontraktu.
