# RailDispatchMono Documentation

**Documentation baseline: `0.1.2i`**  
**Status: current documentation set**

This directory is intentionally limited to a compact set of authoritative documents. Historical release details belong in `docs/changelog/` and are not duplicated here.

## Start here

1. [01-project-overview.md](01-project-overview.md) — project identity and technology baseline.
2. [02-repository-structure.md](02-repository-structure.md) — source tree and responsibility map.
3. [03-architecture.md](03-architecture.md) — architectural boundaries and dependencies.
4. [04-runtime-lifecycle.md](04-runtime-lifecycle.md) — game startup, update and shutdown flow.
5. [05-screen-system.md](05-screen-system.md) — screen manager and screen lifecycle.
6. [06-input.md](06-input.md) — input modes and controls.
7. [07-game-domain.md](07-game-domain.md) — trains, railway, stations and passengers.
8. [08-settings-localization.md](08-settings-localization.md) — settings and localization.
9. [09-content-platforms.md](09-content-platforms.md) — content and platform targets.
10. [10-development-workflows.md](10-development-workflows.md) — build, test and development workflow.
11. [11-ai-agent-rules.md](11-ai-agent-rules.md) — mandatory rules for AI-assisted development.
12. [12-known-issues-and-cautions.md](12-known-issues-and-cautions.md) — current limitations and known risks.
13. [13-code-index.md](13-code-index.md) — useful implementation entry points.
14. [14-documentation-maintenance.md](14-documentation-maintenance.md) — how this documentation set is maintained.
15. [15-ai-context.md](15-ai-context.md) — compact context packet for future AI sessions.
16. [16-screens-and-ui.md](16-screens-and-ui.md) — current UI/screen inventory.
17. [17-game-map-and-geometry.md](17-game-map-and-geometry.md) — map and geometry rules.
18. [18-platform-hosts.md](18-platform-hosts.md) — platform-host responsibilities.
19. [19-current-state-0.1.2i.md](19-current-state-0.1.2i.md) — authoritative current-state snapshot.

## Version policy

- `0.1.0` is the feature-complete gameplay baseline.
- `0.1.1` restructured the documentation set.
- `0.1.2` is the Myra UI integration series; each `0.1.2x` letter is an independent incremental stage.
- A failed `0.1.2x` stage is not rewritten in place. Fixes are introduced by the next lettered stage.
- From `0.1.x` onward, preserve existing APIs and architecture unless the task explicitly requires a reviewed change.

## Source of truth

When documentation conflicts with code, inspect the current implementation and its call sites first. Update this documentation after confirmed architectural or behavioral changes.
