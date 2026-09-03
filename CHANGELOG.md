# Changelog

This file is the high-level release history. Detailed release notes are kept in `docs/changelog/`.

## [0.1.4i] — Train labels, Depot building and coupling preparation
**Data:** 2026-09-03

- Restored visible centered white rolling-stock labels during gameplay and bound the `Arial24` font through `GameplayScreen`.
- Locomotive labels use `EP07`, `EU200`, `SU42`; passenger coaches use `1KL`, `2KL`, `3KL`.
- Labels remain readable in both travel directions.
- Reworked `DepotRenderer` to use world-space coordinates under the existing camera transform, matching the station rendering contract.
- Added a visible 1x1-cell Depot building with outline/entrance details and placement preview.
- Added static coupling metadata: `CouplerType`, `CouplingSpecification` and `Vehicle.Coupling`.
- Default rolling stock exposes screw couplers at both ends.
- Fixed `RuntimeSaveService` to save/load rolling-stock `ShortName` values and to use the current `Locomotive` and `Wagon` constructor signatures.
- Fixed rolling-stock definitions to construct `Locomotive` and `Wagon` with the current `ShortName`-aware signatures.
- Added explicit wagon short labels `1KL`, `2KL`, `3KL` to the rolling-stock definitions used by the Depot and train renderer.
- Removed the accidental duplicate top-level `RailDispatchMono.Core` directory introduced during `0.1.4g`; the build solution continues to reference `RailDispatchMono/RailDispatchMono.Core`.
- Kept runtime save schema version at 1; older saves without `ShortName` remain deserializable.
- Runtime coupling/decoupling, compatibility, connection state, forces and persistence remain explicitly deferred to 0.1.5.
- Updated game-domain, screen/UI and AI-context documentation and detailed `docs/changelog/0.1.4i.md`.
- No `current-state` snapshot was created; `0.1.4i` remains a lettered development stage.

## [0.1.4h] — Speed-dependent braking and RadioStop safety
**Data:** 2026-09-03

- Fixed signal stopping after the `0.1.4f` consist-mass braking change: Stop/StopStation braking now uses the same effective braking rate as actual train movement.
- Speed-restricted signal braking-distance checks also use effective consist braking.
- Preserved the current 0.8-cell Stop target offset and leading-vehicle physical half-length correction.
- RadioStop retains its 3-cell minimum safety distance but expands the protected distance at higher speed using actual braking distance, 0.15 s reaction distance and a 0.8-cell buffer.
- Added `Train.EffectiveBrakingRate` as the shared safety-facing representation of loaded consist braking capability.
- Updated game-domain, UI and AI-context documentation and added detailed `docs/changelog/0.1.4h.md`.
- No `current-state` snapshot was created; `0.1.4h` is a lettered development stage.

## [0.1.4g] — Rolling stock visuals and locomotive power
**Data:** 2026-09-03

- Added differentiated rolling-stock rendering: electric locomotives are red, diesel locomotives are black, and passenger coach variants use distinct blue shades.
- Added centered white rolling-stock labels: `EP07`, `EU200`, `SU42`, `1KL`, `2KL`, `3KL`.
- Labels remain readable when trains travel in either direction.
- Added `LocomotiveParameters` with `PowerMW`.
- Calibrated locomotive power: EP07 2.0 MW, EU200 5.5 MW, SU42 1.2 MW.
- Added nonlinear power/load Vmax model with a 0.006 MW/t supported-mass threshold and 0.55 load exponent.
- EU200 + 10 passenger wagons remains at 200 km/h; SU42 + 5 wagons is approximately 75 km/h and SU42 + 10 wagons approximately 55 km/h.
- Passenger coach catalogue Vmax was raised to 200 km/h so EU200 is not artificially capped by the coach definitions.
- The 0.1.4f nonlinear mass effect on acceleration/braking remains unchanged and independent from the new power/Vmax model.