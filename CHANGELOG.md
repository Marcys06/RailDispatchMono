# Changelog

This file contains the high-level release history. The 0.1.5 development sub-milestones have been consolidated because `0.1.5pre` is the final state of the cycle.

## [0.1.5pre] — Final 0.1.5 pre-release
**Data:** 2026-09-03

### Train direction and F7

- Rebuilt `F7` as a non-destructive train travel-direction change.
- `Composition.Vehicles` is never reversed or reordered by F7.
- Vehicle world positions and inter-vehicle distances are preserved at the reversal instant.
- F7 changes only the train's cardinal `Direction` and is accepted only at `0 km/h`.
- The locomotive remains at its existing world position; F7 does not teleport it to the opposite end.
- `Speed` remains a non-negative magnitude.
- `IsReversed` remains persisted travel-state information and does not reconstruct the vehicle list.

### Train movement and curves

- Initial consist spawning now uses vehicle lengths and travel direction so vehicles do not collapse onto one grid coordinate.
- Rigid vehicle offsets preserve straight-track spacing.
- Curve movement follows the travelled trajectory.
- Each vehicle samples its own historical position according to its distance behind the train head.
- Each vehicle derives its rotation from the local tangent of its own trajectory position, so vehicles enter curves sequentially rather than rotating simultaneously.
- The train head continues to use the exact active curve-arc tangent.
- F7 does not independently rotate vehicle graphics.

### Coupling and decoupling

- Physical coupling is represented by concrete vehicle ends and runtime `CouplingConnection` objects.
- Coupler compatibility is determined by `CouplingSpecification` / `CouplerType`.
- Adjacent compatible vehicles in pre-built consists can establish runtime couplings automatically.
- Coupling operates on outer train boundaries and preserves consist order.
- `C` selects the nearest valid coupling candidate and delegates the authoritative operation to `CouplingService`.
- `X` targets the wagon under the cursor and prefers its rear runtime connection when both ends are connected.
- Decoupling is allowed only below `6 km/h`.
- Detached consists preserve vehicle order, internal runtime couplings and physical positions.

### Shunting and safety

- `F6` is manual shunting control toward `3 km/h` while held over a train.
- The former configurable `F6/F7/F8` coupling-speed profiles are removed.
- Coupling uses a fixed `6 km/h` shunting limit.
- `F6` manual shunting bypasses the automatic RadioStop/collision stop path for the targeted train while held.
- RadioStop remains an independent safety mechanism.
- Coupling and decoupling reset affected signal state.

### Persistence

- Saved reversal state is restored without reversing the physical vehicle list or reconstructing vehicle positions from list order.

### Scope

`0.1.5pre` is the final version of the 0.1.5 development cycle. Further features and changes belong to a later version.

The milestone intentionally remains a rigid consist model. Slack action, impact forces, coupling delay, brake-pipe propagation and full longitudinal vehicle dynamics are outside its scope.

The dedicated `RailDispatchMono.Core.Tests` project is not part of the repository.

Detailed final notes: `docs/changelog/0.1.5pre.md`.
