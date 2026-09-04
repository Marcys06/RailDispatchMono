# Documentation maintenance

## Purpose

The `docs/` directory preserves architectural context, implementation contracts and development rules. Release history is maintained separately in `CHANGELOG.md` and `docs/changelog/`.

## Current development line

The current development line is `0.1.6a`. The previous consolidated milestone is `0.1.5pre`.

Current-state snapshots are reserved for consolidated `pre` milestones. Lettered development stages are documented in `docs/changelog/`.

## Documentation locations

- `docs/*.md` — maintained architecture, domain, workflow, AI and current-state documentation.
- `docs/changelog/*.md` — detailed historical/development release notes.
- `CHANGELOG.md` — high-level release history.

## 0.1.6 passenger documentation rule

The station/passenger foundation is implemented and must be documented as existing functionality, not as future work. The authoritative chain is:

`StationController → PassengerManager → IPassengerService / IPassengerDemandProvider → Wagon / TrainRoute`.

When changing this subsystem, update the domain, AI-context, code-index, UI and known-issues documentation when their contracts change. Keep future passenger features clearly separated from implemented behavior.

## 0.1.5 documentation rule

The completed `0.1.5pre` contract includes rigid consist movement, F6 manual shunting, F7 direction reversal, curve trajectory/per-vehicle orientation, rigid coupling/decoupling, rolling-stock catalogue/performance and Depot creation. Do not reintroduce the former F6/F7/F8 coupling-speed selector description.

## Audit procedure

1. inspect implementation;
2. search usages and constructors;
3. inspect affected platform projects;
4. verify persistence and compatibility contracts;
5. verify rendering/input coordinate assumptions;
6. update affected maintained docs;
7. update the current development changelog;
8. update `CHANGELOG.md` when the release history changes.

## Source of truth

If documentation conflicts with code, inspect the current implementation and call sites first. Historical changelog entries are not rewritten to match later behavior.
