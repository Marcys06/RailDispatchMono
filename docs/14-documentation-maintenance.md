# Documentation maintenance

## Purpose

The `docs/` directory is part of the development infrastructure. Its purpose is to preserve architectural context that is otherwise easy for an AI agent to lose between tasks.

## Current baseline

The current release baseline is `0.1.0`. It is a stabilization/bugfix release following the completed `0.0.16` feature series.

Current-state documentation must describe the executable `0.1.0` behavior. Historical documents and versioned changelogs retain their original version identifiers and must not be rewritten as current-state documentation.

## Documentation locations

- `docs/*.md` — maintained architecture, domain, workflow, AI and current-state documentation.
- `docs/changelog/*.md` — detailed historical release notes.
- `CHANGELOG.md` — high-level release history.

There is one canonical documentation tree. Do not create another `Docs` directory under a project or Core directory.

## When to update documentation

Update documentation whenever a change affects:

- project structure;
- dependency direction;
- application lifecycle;
- screen lifecycle or routing;
- input semantics;
- coordinate systems;
- settings or persistence;
- localization;
- content loading/assets;
- platform bootstrapping;
- domain ownership;
- public APIs used across subsystems;
- save format or save compatibility;
- release scope.

## What not to document as fact

Do not document:

- intended future architecture as if it already exists;
- behavior inferred only from a class name;
- behavior contradicted by current code;
- obsolete implementation details without marking them obsolete;
- speculative bug explanations as confirmed causes.

## Source references

Documentation should prefer repository-relative paths and exact class/member names. This makes the material useful to an AI even when the repository is checked out at a different location.

## Change discipline

Documentation changes must preserve historical release information. Do not delete old changelog files merely because the current baseline has advanced.

When updating current-state documents, replace stale current-version statements rather than duplicating several competing baselines.

## Audit procedure

For a substantial code change:

1. read the target implementation;
2. search usages;
3. inspect affected platform projects;
4. verify the actual public/internal API before changing callers;
5. update the relevant current-state document;
6. update `docs/00-index.md` if the documentation map or current baseline changed;
7. add a detailed entry under `docs/changelog/` for a release change;
8. update `CHANGELOG.md` for the high-level release history;
9. add a caution entry if the change introduces a non-obvious lifecycle or compatibility constraint.

## AI handoff

An AI agent entering the repository should read, in order:

1. `docs/00-index.md`
2. `docs/01-project-overview.md`
3. `docs/03-architecture.md`
4. `docs/04-runtime-lifecycle.md`
5. `docs/05-screen-system.md`
6. `docs/11-ai-agent-rules.md`
7. `docs/15-ai-context.md`

Then read the subsystem-specific document relevant to the task.
