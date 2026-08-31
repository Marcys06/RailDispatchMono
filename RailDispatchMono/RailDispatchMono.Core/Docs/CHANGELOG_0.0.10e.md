# [0.0.10e] — Wielopolowe stacje: budowanie i UI
**Data:** 2026-08-31

## Station Building

- Dodano wybór rozmiaru stacji w trybie budowania `Station`.
- Klawisz `R` w trybie stacji przełącza kolejno:
  - `1x1`
  - `2x2`
  - `3x3`
  - `4x4`
- Podgląd stacji pokazuje aktualnie wybrany rozmiar.
- Stacja jest zakotwiczona centralnie pod kursorem.
- Podczas budowania sprawdzane jest, czy każde pole obszaru posiada tor.
- Budowa jest odrzucana, jeżeli obszar nachodzi na istniejącą stację.
- Model `Station` nadal przechowuje `Width` i `Height`, więc stacje wielopolowe są obsługiwane także przez system pasażerów i hamowania.

## Station Rendering / UI

- Poprawiono oznaczenie stacji na mapie.
- Dodano wyraźny znak `+` w centrum stacji.
- Znak `+` jest widoczny również dla stacji `1x1`.
- Stacja wielopolowa ma jedno centralne oznaczenie zamiast pustego obszaru.
- Tooltip stacji pokazuje dodatkowo jej rozmiar.
- Tooltip nadal pokazuje liczbę oczekujących pasażerów, liczbę różnych celów, stan obsługi i czas postoju.

## Controls

- `5` — tryb budowania stacji.
- `R` w trybie stacji — zmiana rozmiaru: `1x1 → 2x2 → 3x3 → 4x4 → 1x1`.
- `LPM` — postaw stację o wybranym rozmiarze.
- `Shift + PPM` — usuń stację.

## Validation

- Stacja wielopolowa wymaga toru na każdym zajmowanym polu.
- Nie można utworzyć stacji nachodzącej na inną stację.
- Semafory nadal definiują faktyczny punkt zatrzymania pociągu; stacja nie otrzymuje osobnego `StopPoint`.

## Known Gaps

- Rozmiary są obecnie ograniczone do czterech presetów kwadratowych: `1x1`, `2x2`, `3x3`, `4x4`.
- Nie ma jeszcze prostokątnych rozmiarów typu `2x3` lub `3x5`.
- Brak edycji rozmiaru istniejącej stacji bez jej usunięcia i ponownego postawienia.
