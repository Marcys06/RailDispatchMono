# Stations — 0.0.10 and current baseline

Stations are rectangular world objects placed by the user on existing track cells.

## Current behavior

- Stations can cover more than one cell.
- A station is managed by `StationController` and exposed through `TrainManager`.
- A train stops for passenger service when the stop-decision layer requires it; a station without a required stop does not force braking by itself.
- Passenger exchange is performed during the station dwell phase.
- Station UI exposes station information and waiting passenger counts.
- Station lists in the gameplay HUD can move the camera to a station.

## Architecture

Train stopping and passenger handling are intentionally separate concerns:

- `ITrainStopDecision` decides whether the train should stop.
- `IPassengerService` handles passenger exchange.
- Passenger demand is provided separately so random generation can later be replaced by city/population data.

Future work includes richer platform assignment, station configuration and timetable-driven stopping.
