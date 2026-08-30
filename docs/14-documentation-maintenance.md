# Documentation maintenance

## Purpose

The `docs/` directory is part of the development infrastructure. Its purpose is to preserve architectural context that is otherwise easy for an AI agent to lose between tasks.

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
- public APIs used across subsystems.

## What not to document as fact

Do not document:

- intended future architecture as if it already exists;
- behavior inferred only from a class name;
- behavior that is contradicted by current code;
- obsolete implementation details without marking them obsolete;
- speculative bug explanations as confirmed causes.

## Source references

Documentation should prefer repository-relative paths and exact class/member names. This makes the material useful to an AI even when the repository is checked out at a different location.

## Change discipline

Documentation-only changes must be additive when the task requests preservation of the existing project. Do not delete unrelated project files or rewrite existing documentation outside the requested scope.

## Audit procedure

For a substantial code change:

1. read the target implementation;
2. search usages;
3. inspect affected platform projects;
4. update the relevant document;
5. check the index if files were added/removed/renamed;
6. add a caution entry if the change introduces a non-obvious lifecycle or compatibility constraint.

## AI handoff

An AI agent entering the repository should read, in order:

1. `docs/00-index.md`
2. `docs/01-project-overview.md`
3. `docs/03-architecture.md`
4. `docs/04-runtime-lifecycle.md`
5. `docs/05-screen-system.md`
6. `docs/11-ai-agent-rules.md`

Then read the subsystem-specific document relevant to the task.
