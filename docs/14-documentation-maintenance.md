# Documentation maintenance

## Purpose

The `docs/` directory preserves architectural context, implementation contracts and development rules. Release history is maintained separately in `CHANGELOG.md` and `docs/changelog/`.

## Current baseline

The current project baseline is `0.1.4pre`.

Only consolidated `pre` milestones receive current-state snapshots:

- `19-current-state-0.1.2pre.md`
- `20-current-state-0.1.3pre.md`
- `21-current-state-0.1.4pre.md`

Lettered stages do not receive current-state files.

## Documentation locations

- `docs/*.md` — maintained architecture, domain, workflow and AI documentation plus consolidated pre-release snapshots.
- `docs/changelog/*.md` — detailed historical release notes.
- `CHANGELOG.md` — high-level release history.

## When to update documentation

Update maintained documentation when a change affects project structure, dependencies, lifecycle, screen routing, input semantics, coordinate systems, settings/persistence, localization, content loading, platform bootstrapping, domain ownership, rendering contracts, diagnostics or public APIs. Every release change also gets a changelog entry.

## Change discipline

Historical lettered stages remain immutable. Do not rewrite `0.1.4i` as if it were a final release. The `0.1.4pre` snapshot consolidates the implemented 0.1.4 line without erasing its historical changelog.

## Audit procedure

For a substantial change:

1. inspect implementation;
2. search usages and constructors;
3. inspect affected platform projects;
4. verify persistence and compatibility contracts;
5. verify rendering/input coordinate assumptions;
6. update affected maintained docs;
7. update `docs/00-index.md` if the documentation map changes;
8. update the current `pre` snapshot when the consolidated state changes;
9. add detailed release notes under `docs/changelog/`;
10. update `CHANGELOG.md`.

## AI handoff

Start with `docs/00-index.md`, then read architecture/lifecycle/screen/domain/AI documents relevant to the task and the current `0.1.4pre` snapshot. Treat source code and call sites as authoritative if any documentation is stale.
