# Roadmap 0.3.0 → 1.0.0

## Zasada nadrzędna

Rozwój ma dodawać kolejne warstwy bez rozbijania istniejących właścicieli stanu. Dispatcher, bloki, sygnały, zwrotnice, rozkład lokomotywy, `Wagon` jako właściciel pasażera i istniejący model ruchu pozostają punktami integracji.

Profil docelowy to **B — przystępna symulacja**: zależności mają być wiarygodne i czytelne, ale liczba ręcznie obsługiwanych parametrów ma pozostać kontrolowana.

## 0.3.0 — Zarządzanie infrastrukturą

**Cel:** przejście od statycznych parametrów toru do aktywnego zarządzania siecią.

Zakres:

- elektryfikacja AC/DC jako realne ograniczenie eksploatacyjne;
- klasy linii i ich konsekwencje dla ruchu;
- koszty utrzymania infrastruktury;
- zużycie torów i urządzeń;
- modernizacje/naprawy;
- czytelne ostrzeżenia o stanie infrastruktury;
- dalsze wykorzystanie `TrackCell.WearPercent` zamiast nowego modelu torów.

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

Warstwa ekonomiczna ma konsumować dane z infrastruktury i ruchu, nie sterować nimi bezpośrednio.

## 0.5.0 — Rozkład jazdy

**Cel:** przejście od rozkładu operacyjnego lokomotywy do publicznego planu przewozów.

Zakres:

- publiczny rozkład jazdy;
- planowanie kursów;
- koordynacja pociągów;
- konflikty planowe;
- zależność planu od dostępnej infrastruktury;
- zachowanie obecnej zasady: rozkład jest planem, runtime jest obserwacją.

## 0.6.0 — Symulacja pasażerska

**Cel:** modelowanie popytu i decyzji pasażerów.

Zakres:

- generowanie popytu;
- wybór połączenia/środka transportu;
- oczekiwanie i przesiadki;
- zadowolenie;
- wpływ opóźnień i jakości obsługi.

Własność pasażera nadal pozostaje przy konkretnym `Wagon` podczas przejazdu.

## 0.7.0 — Fizyka

**Cel:** wykorzystanie danych infrastruktury w modelu ruchu.

Zakres:

- ograniczenia prędkości na łukach;
- gradienty;
- zależność osiągów od masy i infrastruktury;
- hamowanie;
- hamowanie odzyskowe dla odpowiedniego taboru;
- dalsza kalibracja istniejącego modelu ruchu zamiast jego wymiany.

## 0.8.0 — Kryzysy

**Cel:** sytuacje zakłócające normalną eksploatację.

Zakres:

- awarie infrastruktury;
- awarie taboru;
- pogoda;
- ograniczenia ruchowe;
- zarządzanie kryzysowe;
- priorytety działań gracza.

Zużycie z 0.3.0 ma być jednym z wejść do prawdopodobieństwa awarii.

## 0.9.0 — Sieć

**Cel:** przejście z pojedynczego obszaru mapy do sieci kolejowej.

Zakres:

- połączenia między miastami;
- stacje węzłowe;
- dłuższe trasy;
- przesiadki;
- rozkład jazdy na poziomie sieci;
- przepustowość i ograniczenia infrastruktury.

## 1.0.0 — Full release

**Cel:** zintegrowana wersja gry.

Zakres:

- pełna integracja infrastruktury, ekonomii, rozkładu, pasażerów, fizyki, kryzysów i sieci;
- balans;
- spójny onboarding;
- kampania/scenariusze;
- stabilny zapis gry;
- pełna dokumentacja aktualnego kontraktu.

## Kolejność zależności

```text
0.2.4 infrastruktura danych
        |
        v
0.3.0 zarządzanie infrastrukturą
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

Ta kolejność jest roadmapą architektoniczną, nie obowiązkiem implementowania wszystkich elementów w jednym wydaniu. Każda wersja powinna kończyć się aktualizacją bieżącego snapshotu i changelogu.
