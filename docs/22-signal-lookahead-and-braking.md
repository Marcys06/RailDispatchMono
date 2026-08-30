# Signal look-ahead and braking

## Problem fixed

The train previously searched for a signal only in its current and next map cell. The braking-distance calculation therefore received the distance to the current cell boundary rather than the real distance to the next signal.

This could cause a train to begin braking only after passing a STOP signal.

## Current behaviour

`Train.GetNextSignal()` now follows the track route ahead until it finds the first signal matching the train direction. The search is bounded to avoid infinite traversal on malformed/cyclic track data.

The returned distance includes:

- the remaining distance to the current cell boundary;
- every traversed track cell before the signal.

For STOP/STOP_STATION, the train uses the available distance to calculate a safe target speed from the braking rate.

For restrictive permissive aspects such as Speed40 or Speed100, the train remains at its current speed until the calculated braking distance is reached, then reduces speed toward the signal limit.

## Important limitation

The current route traversal follows the first available continuation at junctions. It does not yet resolve route selection from block reservations/switch state. The block controller remains the authoritative occupancy and automatic signal-control system; route-aware signal look-ahead should be integrated with block reservations when junction routing is implemented.

## Diagnostic expectation

When testing a STOP signal, the train should start reducing its target speed before entering the signal cell. The exact braking point depends on current speed, braking rate, reaction-time margin and distance to the signal.
