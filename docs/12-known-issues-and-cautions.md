# Known issues and cautions

## Current baseline: 0.0.13pre

- Route creation is planned for `0.0.13` and is not part of this mini-update.
- Train movement supports the current track network, curves and junctions, but full route planning is not implemented yet.
- Signal lookahead/braking is implemented; full interlocking remains future work.
- Passenger simulation is quasi-individual and wagon-specific; full transfer routing is future work.
- Depots are world buildings and are the preparation point for future route creation.
- Game time runs at 5× wall-clock time at x1. x2/x5 multiply game-time progression further.
- The fixed 5× clock scale does not multiply train velocity, acceleration, braking or travelled distance. x2/x5 still change the normal simulation delta as before.
- Debug output is globally limited to 30 emitted messages per second. Excess diagnostics are intentionally dropped.
- There is one canonical documentation tree: `docs/`. Do not recreate `RailDispatchMono.Core/Docs`.

## Timing rule

When changing simulation timing, do not feed the 5× clock presentation scale into train movement. `GameClock.Update()` advances displayed game time at 5×, but returns the normal simulation delta multiplied only by the selected x1/x2/x5 simulation speed.
