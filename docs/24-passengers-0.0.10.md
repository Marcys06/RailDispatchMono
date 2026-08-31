# Passengers — 0.0.10 and current baseline

Passengers are quasi-individual entities with an origin station and destination station.

## Current model

- Passenger state is tracked explicitly.
- Passengers can wait at a station or travel inside a wagon.
- Each passenger belongs to a specific wagon while travelling.
- Wagon capacity is handled independently for each wagon.
- The model is prepared for future coupling/decoupling: passengers remain assigned to their wagon, so a detached wagon can carry its own passenger subset.
- Passenger transfers are not yet a full routing system; a passenger whose wagon cannot continue toward the destination can return to station waiting state.

## Passenger generation

Stations can use a demand provider abstraction. The current implementation supports random demand and is intentionally structured so a future city/population system can provide demand without changing passenger service APIs.
