# Documentation maintenance

## Purpose

The `docs/` directory preserves architectural context and development rules. Release history is maintained separately in `CHANGELOG.md` and `docs/changelog/`.

## Current baseline

The current project baseline is `0.1.3pre`.

Only pre-release milestones receive current-state snapshots:

- `19-current-state-0.1.2pre.md`
- `20-current-state-0.1.3pre.md`

Lettered stages do not receive current-state files.

## Documentation locations

- `docs/*.md` — maintained architecture, domain, workflow and AI documentation plus the two pre-release snapshots.
- `docs/changelog/*.md` — detailed historical release notes.
- `CHANGELOG.md` — high-level release history.

## When to update documentation

Update maintained documentation when a change affects project structure, dependencies, lifecycle, screen routing, input semantics, coordinate systems, settings/persistence, localization, content loading, platform bootstrapping, domain ownership or public contracts. Every release change also gets a changelog entry.

## Change discipline

Historical lettered stages remain immutable. Do not create current-state snapshots for lettered stages. The next `pre` milestone becomes the next authoritative current-state snapshot.

## Audit procedure

For a substantial change:

1. inspect implementation;
2. search usages;
3. inspect affected platform projects;
4. verify APIs;
5. update affected maintained docs;
6. update `docs/00-index.md` if the documentation map changes;
7. add detailed release notes under `docs/changelog/`;
8. update `CHANGELOG.md`.

## AI handoff

Start with `docs/00-index.md`, then read architecture/lifecycle/screen/AI documents relevant to the task and the current `pre` snapshot.
