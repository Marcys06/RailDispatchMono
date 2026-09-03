# Changelog

This file is the high-level release history. Detailed release notes are kept in `docs/changelog/`.

## [0.1.5e] — Coupling command controls
**Data:** 2026-09-03

- Added direct `Coupling` command on `C` using the nearest valid boundary candidate.
- Added separate `Decoupling` command on `X` targeting the last coupling created by the command layer, with deterministic fallback to the first remaining runtime connection.
- Added shunting speed profiles of `3 / 4 / 5 km/h` on `F6 / F7 / F8`; `5 km/h` is the default.
- Coupling command refuses to execute when either participating train exceeds the selected shunting speed.
- Associated the command semantics with signal aspect `S14 Rezerwowy 3` without changing its global signal-speed definition or adding new signal physics.
- Actual coupling/decoupling remains delegated to the existing authoritative `CouplingService`.
- Detailed notes: `docs/changelog/0.1.5e.md`.

## [0.1.5d] — Coupling regression test foundation
**Data:** 2026-09-03

- Added `RailDispatchMono.Core.Tests` targeting .NET 9 and referencing the core project.
- Added regression tests for default screw-coupler configuration.
- Added regression tests for `CouplingConnection` matching in both directions and self-connection rejection.
- Added regression tests for typed coupling operation success/failure semantics.
- Added regression tests confirming exact vehicle order through `TrainComposition.Split()` and `Train` construction.
- Added the test project to `RailDispatchMono.slnx`.
- Runtime coupling mechanics were not executed in a live gameplay scenario as part of this milestone.
- Detailed notes: `docs/changelog/0.1.5d.md`.

## [0.1.5c] — Coupling candidate discovery
**Data:** 2026-09-03

- Added `CouplingCandidate` as a UI-neutral snapshot of a possible physical connection.
- Added `TrainManager.GetCouplingCandidates(Train)` as the authoritative discovery entry point for future UI/input code.
- Candidate data exposes both concrete vehicle ends, endpoint positions, measured distance and the authoritative `CouplingCheckResult`.
- Candidate discovery considers only outer vehicle ends, matching the current rigid coupling rule.
- Candidates are sorted by endpoint distance for deterministic selection.
- Detailed notes: `docs/changelog/0.1.5c.md`.

## [0.1.5b] — Coupling stop behavior
**Data:** 2026-09-03

- Coupling now stops both participating trains through the existing `RadioStop` mechanism before changing composition.
- The merged train starts from `0 m/s` instead of inheriting shunting momentum.
- Decoupling stops the original train before splitting the composition.
- The newly created detached train is registered at `0 m/s` and receives `RadioStop`.
- Vehicle order and concrete vehicle-end coupling state remain unchanged by this stage.
- Detailed notes: `docs/changelog/0.1.5b.md`.

## [0.1.5a] — Rigid coupling and decoupling foundation
**Data:** 2026-09-03

- Added intrinsic `VehicleEnd` (`Front` / `Rear`) and runtime per-vehicle coupling state.
- Added runtime `CouplingConnection` linking two concrete vehicle ends.
- Added typed coupling validation and operation results with explicit failure reasons.
- Added endpoint geometry derived from the existing vehicle transforms and vehicle `Length`.
- Added coupling-distance and end-alignment validation.
- Added static coupler compatibility checks.
- Coupling is currently restricted to outer train boundaries and preserves vehicle order when two trains merge.
- Decoupling now splits a consist at the concrete connected vehicle boundary and registers the detached section as a new `Train`.
- Added `[COUPLING]` diagnostics.
- Kept RadioStop independent from coupling; safety stopping is not bypassed by coupling mechanics.
- Added temporary `docs/coupling-decoupling-work.md` tracker covering only coupling/decoupling mechanics.
- Deferred UI, persistence, automated tests, coupling animation/delay, slack, forces, impact dynamics and brake-pipe propagation.
- Build was not run in this environment.

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
