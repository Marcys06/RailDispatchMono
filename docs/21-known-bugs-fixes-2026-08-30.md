# Known bug fixes — 2026-08-30

## High priority — automatic semaphore release

### Required behavior

When a train leaves a block controlled by an entry semaphore:

1. the block must become unoccupied;
2. a 3.0 second delay starts;
3. during the delay the semaphore aspect is not changed;
4. after the delay, **only `Clear -> Stop` may be performed automatically**;
5. `Stop -> Clear` and other `Stop -> permissive` transitions remain manual;
6. an already-manually-selected non-`Clear` aspect is not overwritten by the automatic rule;
7. if a train re-enters the block during the delay, the pending automatic stop is cancelled.

### Implementation

`BlockController` now detects the occupied -> free transition using `_previousOccupancy` and keeps a one-shot `_pendingAutomaticStops` entry per block. `Block.CoolDownDuration` is 3 seconds.

The previous implementation continuously forced `Clear` on free blocks and therefore violated the manual-only rule for `Stop -> permissive` aspects.

## Medium priority — train movement physics

The existing movement loop already applies `VehicleParameters.Acceleration` and `VehicleParameters.Braking`. The parameter properties are now effective rates calculated by the coefficient model:

```text
a = m^(d * x^0.9) * k
```

Where:

- `m` = vehicle mass;
- `x` = `MassCoefficient`;
- `d` = acceleration or braking coefficient;
- `k` = `TechnicalCondition`, clamped to `[0.5, 1.5]`.

`VehicleParameters` exposes both the original coefficients (`AccelerationCoefficient`, `BrakingCoefficient`) and the effective rates (`Acceleration`, `Braking`). This means the existing `Train.Update()` movement code can use the new model without duplicating the physics calculation.

For compatibility with the existing project data, braking values above 10 are interpreted as legacy percentage-like values and divided by 100 before entering the coefficient formula. Existing values such as `braking: 100` therefore become a coefficient of `1.0` rather than producing an unusably large exponent.

The default mass coefficient is `0.01`. It is deliberately a gameplay tuning coefficient because the requested formula is not dimensionally an SI physics equation.

## Low priority — track removal and neighbour connections

The current `TrackBuilder.Remove(MapPosition)` already calls `UpdateNeighborsAfterRemoval()` and removes the corresponding connection from each adjacent track. `RemoveConnection()` also removes a neighbour when its connection mask becomes `None`.

Therefore this item is **already implemented in the current `master` source** and should not be reimplemented blindly.

The remaining architectural caution is that removal currently operates through `TrackBuilder.Remove()`. Any future direct call to `GameMap.RemoveTrack()` bypasses neighbour maintenance. New gameplay code should therefore use `TrackBuilder.Remove()` when removing a user-visible piece of track.

## Verification checklist

- [ ] A free block at startup does not automatically force its signal to Stop.
- [ ] Occupied -> free starts exactly one 3-second timer.
- [ ] Re-entry during the timer cancels the pending stop.
- [ ] After 3 seconds, `Clear` changes once to `Stop`.
- [ ] `Stop` is never automatically changed to `Clear`, `Warning`, `Speed40`, etc.
- [ ] Train acceleration uses the effective coefficient model.
- [ ] Train braking uses the effective coefficient model.
- [ ] Technical condition stays within `[0.5, 1.5]`.
- [ ] Track removal through `TrackBuilder.Remove()` updates neighbours.
