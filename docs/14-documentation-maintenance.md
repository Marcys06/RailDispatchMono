# Documentation maintenance

## Purpose

`docs/` preserves maintained architecture, implementation contracts and development rules. Release history is maintained in `CHANGELOG.md` and `docs/changelog/`.

## Current development line

The maintained documentation baseline is `0.1.6e`. The previous consolidated milestone is `0.1.5pre`.

Lettered `0.1.6x` stages are historical development records and belong in `docs/changelog/`. Maintained architecture/domain documents must describe the latest current contract.

## 0.1.6 documentation contract

The passenger foundation is implemented. The authoritative ownership chain is:

`StationController → PassengerManager → IPassengerService → Wagon`

with destination demand supplied through `IPassengerDemandProvider`.

A passenger belongs to a concrete wagon. A train is only an operational grouping of wagons. Coupling and decoupling therefore must not migrate passengers. `CurrentWagonId`, wagon route validation, `Wagon.CanContinueJourneyTo(...)`, transfer-candidate diagnostics and direct wagon restoration on runtime load must remain documented consistently.

The 0.1.6e coupling contract is also authoritative: locomotive insertion rebuilds adjacent runtime connections; merge clears stale connections and rebuilds the chain from vehicle order; coupling candidates use order-preserving `Rear → Front` boundaries; decoupling uses adjacent indices plus the actual runtime connection.

## Documentation locations

- `docs/*.md` — maintained architecture, domain, workflow, AI, UI and current-state documentation;
- `docs/changelog/*.md` — detailed historical/development release notes;
- `CHANGELOG.md` — high-level release history.

## Audit procedure

1. inspect implementation;
2. inspect callers and consumers;
3. inspect constructors and data contracts;
4. inspect save/load compatibility;
5. inspect input/rendering boundaries;
6. update all maintained docs affected by the contract change;
7. update the lettered changelog entry;
8. update `CHANGELOG.md` when release history changes.

## Source of truth

If documentation conflicts with code, inspect current implementation and call sites first. Historical changelog entries are not rewritten to describe later behavior.
