# RailDispatchMono Documentation

**Documentation baseline: `0.1.4e`**  
**Status: current documentation set**

This directory contains the authoritative project documentation. Historical release changes belong in `docs/changelog/` and are not duplicated in current-state snapshots.

## Documentation index

1. [01-project-overview.md](01-project-overview.md) — project identity and technology baseline.
2. [02-repository-structure.md](02-repository-structure.md) — source tree and responsibility map.
3. [03-architecture.md](03-architecture.md) — architectural boundaries and dependencies.
4. [04-runtime-lifecycle.md](04-runtime-lifecycle.md) — game startup, update and shutdown flow.
5. [05-screen-system.md](05-screen-system.md) — screen manager and screen lifecycle.
6. [06-input.md](06-input.md) — input modes and controls.
7. [07-game-domain.md](07-game-domain.md) — trains, rolling stock, railway, stations and passengers.
8. [08-settings-localization.md](08-settings-localization.md) — settings and localization.
9. [09-content-platforms.md](09-content-platforms.md) — content and platform targets.
10. [10-development-workflows.md](10-development-workflows.md) — build, test and development workflow.
11. [11-ai-agent-rules.md](11-ai-agent-rules.md) — mandatory rules for AI-assisted development.
12. [12-known-issues-and-cautions.md](12-known-issues-and-cautions.md) — current limitations and known risks.
13. [13-code-index.md](13-code-index.md) — useful implementation entry points.
14. [14-documentation-maintenance.md](14-documentation-maintenance.md) — documentation maintenance rules.
15. [15-ai-context.md](15-ai-context.md) — compact context packet for future AI sessions.
16. [16-screens-and-ui.md](16-screens-and-ui.md) — current UI/screen inventory.
17. [17-game-map-and-geometry.md](17-game-map-and-geometry.md) — map and geometry rules.
18. [18-platform-hosts.md](18-platform-hosts.md) — platform-host responsibilities.
19. [19-current-state-0.1.2pre.md](19-current-state-0.1.2pre.md) — authoritative `0.1.2pre` snapshot.
20. [20-current-state-0.1.3pre.md](20-current-state-0.1.3pre.md) — authoritative `0.1.3pre` snapshot.

## Version policy

- `0.1.2a`–`0.1.2k` are immutable historical development stages.
- `0.1.2pre` is the authoritative stabilization snapshot for the 0.1.2 series.
- `0.1.3a`–`0.1.3e` are immutable historical development stages.
- `0.1.3pre` is the authoritative snapshot for the 0.1.3 series.
- `0.1.4a`–`0.1.4e` are immutable lettered development stages recorded in changelogs.
- Lettered development stages do not receive separate current-state files.
- Historical changes are documented in `CHANGELOG.md` and `docs/changelog/`.
- If a historical commit has no reliable functional description, document it as `bugfix` rather than inventing behavior.
- Failed lettered stages are not rewritten in place; their corrections belong to a later stage.

## Source of truth

When documentation conflicts with code, inspect the current implementation and its call sites first. Update the affected authoritative documentation after confirmed architectural or behavioral changes. Current-state snapshots exist only for `pre` milestones; detailed change history belongs in the changelog.
