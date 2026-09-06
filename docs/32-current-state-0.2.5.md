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
- `TrackType` dodatkowo delikatnie koryguje grubość: bocznica jest wizualnie lżejsza, tor zasadniczy zachowuje pełną grubość.

Kolor oznacza trakcję, a grubość klasę linii. Dzięki temu oba wymiary są widoczne jednocześnie bez dokładania kolejnych ikon na każdy segment.

## Grupy szlaków / linii kolejowych

Dodano domenowy `RailwayLine` i `RailwayLineManager`.

Grupa jest logiczną kolekcją segmentów toru i nie zastępuje:

- bloków;
- sygnałów;
- zwrotnic;
- topologii ruchowej;
- rozkładów jazdy.

Manager udostępnia przygotowane operacje:

- tworzenie grupy z listy segmentów;
- wyszukiwanie grupy zawierającej segment;
- przypisywanie/odłączanie segmentów;
- masową zmianę `TrackType`, `LineClass` i `TractionSystem` dla całej grupy;
- zebranie połączonego obszaru torów metodą flood-fill od wskazanego segmentu.

To jest podstawa pod edycję masową: gracz będzie mógł traktować np. `Wrocław–Opole` jako jeden szlak i zmienić jego parametry jednym działaniem, zamiast edytować każdy segment osobno.

## TrackType

W 0.2.5 pozostaje rozróżnieniem eksploatacyjnym `Mainline`, `Secondary`, `Siding`, `Platform`. Jego główną funkcją jest teraz czytelność mapy i przygotowanie do przyszłych zasad infrastruktury. Nie dodano sztucznych ograniczeń ruchu tylko dlatego, że segment ma inny typ.

## Profil rozgrywki

Pozostaje wariant B: przystępna symulacja. System pokazuje konsekwencje i daje narzędzia masowej konfiguracji, ale nie podejmuje za gracza decyzji o przebudowie sieci.

## Roadmapa

0.2.5 przygotowuje UX pod 0.3.0: przyszłe utrzymanie i zużycie mogą działać na pojedynczym segmencie albo na całej grupie `RailwayLine`. Ekonomia pozostaje odroczona do 0.4.0.

## Weryfikacja

Nie wykonano kompilacji Windows ani testu live gameplay. Po pobraniu należy sprawdzić renderowanie wszystkich trzech stanów trakcji, różnice grubości klas linii, zapis/odczyt oraz zachowanie istniejących bloków, sygnałów, zwrotnic i F6.
