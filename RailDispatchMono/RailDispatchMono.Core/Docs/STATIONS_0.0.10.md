# Stations — 0.0.10

## User placement

Stations can now be placed by the user directly on existing track cells.

### Controls

- `5` / `NumPad5` — station placement mode
- `LPM` — place a station on an existing track cell
- `Shift + PPM` — remove a station
- `1` — straight track mode
- `2` — curve mode
- `3` — junction mode
- `4` — signal mode

A station cannot be placed on a cell without track or when another station already occupies the cell.

## Naming

The first implementation assigns automatic names:

- `Stacja 1`
- `Stacja 2`
- `Stacja 3`
- etc.

The `Station.Name` property remains editable by code and is intended to become part of the future station configuration UI.

## Rendering

Stations have a dedicated `StationRenderer`. The renderer draws a compact station marker and a placement preview. It is intentionally separate from track rendering so the station visual representation can be expanded later without coupling it to track geometry.

## Simulation integration

Stations use the existing `TrainManager.StationController`, so stations created through the editor are immediately visible to the station simulation layer.

The existing station controller remains responsible for:

- detecting a train at a station,
- braking for the next passenger-service station,
- stopping the train,
- dwell time,
- passenger alighting,
- passenger boarding.

## Future work

- station configuration menu,
- custom station names,
- platform count and platform assignment,
- station capacity,
- route/platform selection,
- station ownership and infrastructure,
- richer station rendering,
- transfer handling,
- timetable-driven station stops.
