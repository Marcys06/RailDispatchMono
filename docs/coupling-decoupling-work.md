# Coupling / Decoupling — work tracker

**Milestone:** `0.1.5e`  
**Purpose:** temporary implementation checklist for coupling/decoupling mechanics only.  
**Last reviewed:** 2026-09-03

This file is intentionally temporary. It tracks implementation progress and should be removed or replaced by a permanent design document when the 0.1.5 coupling work is complete.

## Implemented

- [x] `VehicleEnd` with intrinsic `Front` / `Rear` ends.
- [x] Static `CouplingSpecification` exposes coupler type per physical end.
- [x] Runtime `VehicleCouplingState` stores connections independently on vehicles.
- [x] `CouplingConnection` links two concrete vehicles and concrete ends.
- [x] Typed coupling validation/result model.
- [x] Endpoint geometry based on `Train.GetVehicleTransform()` and vehicle `Length`.
- [x] Coupler compatibility check (`Screw`↔`Screw`, `Automatic`↔`Automatic`).
- [x] Maximum coupling distance check.
- [x] End-facing/alignment check.
- [x] Coupling restricted to outer train boundaries.
- [x] Coupling merges two `Train` instances into one while preserving vehicle order.
- [x] Coupling stops both consists via the existing `RadioStop` mechanism and starts the merged train from rest.
- [x] Decoupling splits a consist at the concrete connected vehicle ends.
- [x] Detached section is registered as a new `Train` and starts stopped.
- [x] Existing RadioStop remains independent; coupling does not bypass collision safety.
- [x] `[COUPLING]` diagnostics added to runtime operations.
- [x] `CouplingCandidate` exposes both physical endpoints, measured distance and authoritative validation result.
- [x] `TrainManager.GetCouplingCandidates()` enumerates only outer vehicle ends and delegates validity to `CouplingService`.
- [x] Candidates are sorted by physical endpoint distance for deterministic selection later.
- [x] Added a dedicated `RailDispatchMono.Core.Tests` project to establish automated regression coverage for coupling primitives and consist-order invariants.
- [x] Added tests for default coupler configuration, connection matching/rejection, operation result semantics, composition split order and train construction order.
- [x] Added the test project to `RailDispatchMono.slnx`.
- [x] Added `Coupling` command on `C`, selecting the nearest valid boundary candidate.
- [x] Added separate `Decoupling` command on `X`.
- [x] Added shunting speed profiles `3 / 4 / 5 km/h` on `F6 / F7 / F8`, with `5 km/h` as default.
- [x] Coupling command uses `S14 Rezerwowy 3` as its semantic signal profile without changing global signal physics.

## Still to do

- [ ] Add automated unit tests for compatibility, distance, alignment and boundary validation once test fixtures for track geometry are isolated.
- [ ] Add integration tests for `EP07 + wagons -> decouple -> new locomotive -> couple`.
- [ ] Verify split/merge behavior on curves and at cell boundaries.
- [ ] Verify infrastructure state after merge/split (signals, blocks, stations, collision state) under real gameplay update.
- [ ] Replace automatic nearest-candidate command selection with UI selection of a vehicle and a specific coupling end.
- [ ] Add UI feedback for coupling failure reasons and current 3/4/5 km/h command profile.
- [ ] Add persistence of coupling connections.
- [ ] Verify all rolling-stock definitions use the intended coupling types per end.

## Deferred — not part of first rigid implementation

- [ ] Coupling delay / animation.
- [ ] Slack and draw/compression forces.
- [ ] Impact velocity and coupling shock.
- [ ] Longitudinal train dynamics.
- [ ] Brake-pipe propagation.
- [ ] Automatic coupler-specific behavior beyond static compatibility.

## Current operational target

```text
[EP07]--[1KL]--[1KL]--[1KL]--[1KL]

        decouple

[EP07]--[1KL]--[1KL]   [1KL]--[1KL]

        new locomotive arrives

[1KL]--[1KL]   [EP07]

        couple

[1KL]--[1KL]--[EP07]
```

The target behavior is one runtime `Train` after coupling, with vehicle order preserved exactly and both shunting consists stopped by the existing RadioStop mechanism before the composition changes.

## Command contract

`C` is the temporary coupling command. It does not bypass the authoritative `CouplingService`; it selects only a candidate that already passes geometric/structural validation and whose two trains are at or below the selected 3/4/5 km/h command limit.

`X` is intentionally separate from coupling. It targets the last coupling created by `C`, which gives the current test workflow a deterministic decoupling action without introducing a premature vehicle-selection UI.

## Verification boundary

The repository changes establish automated regression coverage for the pure coupling/consist invariants and add the first runtime command path, but the mechanics have not been executed in a live gameplay scenario here. Build/test execution remains an external verification step in the user's runnable .NET environment.
