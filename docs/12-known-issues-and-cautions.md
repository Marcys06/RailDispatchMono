# Known issues and cautions

## Current baseline: 0.0.13pre

- Route creation is still planned for `0.0.13` and is not part of this mini-update.
- Train movement is functional on the currently supported track network, including curves and junctions, but full route planning is not implemented yet.
- Signal lookahead/braking is implemented; full interlocking remains future work.
- Passenger simulation is quasi-individual and wagon-specific; transfers are prepared conceptually but not yet implemented as a full passenger-routing system.
- Depots are buildings used as the preparation point for future train route creation.
- The game clock runs at 5× wall-clock time at x1. x2/x5 multiply game-time progression further. This clock-only scale does not change physical train speed or distance calculations.
- Debug output is globally limited to 30 emitted messages per second. A flood of low-value diagnostics can therefore be intentionally dropped.
- There is one canonical documentation tree: `docs/`. Do not recreate `RailDispatchMono.Core/Docs`.

## Safety rule for changes

When changing simulation timing, do not multiply train movement by `GameClock.BaseTimeScale`. The accelerated clock is a representation of simulation time; physical movement continues to receive the normal simulation delta used by the train system.
