# RailDispatchMono Documentation

**Documentation baseline: `0.1.7a`**  
**Previous consolidated milestone: `0.1.6pre`**

This directory contains maintained project documentation. Historical release notes belong in `docs/changelog/`; source code and current call sites remain authoritative if documentation conflicts with implementation.

## Documentation index

1. [01-project-overview.md](01-project-overview.md) — project identity and technology baseline.
2. [02-repository-structure.md](02-repository-structure.md) — source tree and responsibility map.
3. [03-architecture.md](03-architecture.md) — architectural boundaries and dependencies.
4. [04-runtime-lifecycle.md](04-runtime-lifecycle.md) — game startup, update and shutdown flow.
5. [05-screen-system.md](05-screen-system.md) — screen manager and screen lifecycle.
6. [06-input.md](06-input.md) — input modes and controls.
7. [07-game-domain.md](07-game-domain.md) — railway, trains, rolling stock, stations, passengers, schedules and coupling.
8. [08-settings-localization.md](08-settings-localization.md) — settings and localization.
9. [09-content-platforms.md](09-content-platforms.md) — content and platform targets.
10. [10-development-workflows.md](10-development-workflows.md) — build and development workflow.
11. [11-ai-agent-rules.md](11-ai-agent-rules.md) — mandatory rules for AI-assisted development.
12. [12-known-issues-and-cautions.md](12-known-issues-and-cautions.md) — current limitations and known risks.
13. [13-code-index.md](13-code-index.md) — implementation entry points.
14. [14-documentation-maintenance.md](14-documentation-maintenance.md) — documentation maintenance rules.
15. [15-ai-context.md](15-ai-context.md) — compact context packet for future AI sessions.
16. [16-screens-and-ui.md](16-screens-and-ui.md) — current UI/screen inventory.
17. [17-game-map-and-geometry.md](17-game-map-and-geometry.md) — map and geometry rules.
18. [18-platform-hosts.md](18-platform-hosts.md) — platform-host responsibilities.
19. [19-current-state-0.1.2pre.md](19-current-state-0.1.2pre.md) — historical `0.1.2pre` snapshot.
20. [20-current-state-0.1.3pre.md](20-current-state-0.1.3pre.md) — historical `0.1.3pre` snapshot.
21. [21-current-state-0.1.4pre.md](21-current-state-0.1.4pre.md) — historical `0.1.4pre` snapshot.
22. [22-current-state-0.1.5pre.md](22-current-state-0.1.5pre.md) — historical `0.1.5pre` snapshot.
23. [23-current-state-0.1.6pre.md](23-current-state-0.1.6pre.md) — historical `0.1.6pre` snapshot.
24. [24-current-state-0.1.7a.md](24-current-state-0.1.7a.md) — authoritative current `0.1.7a` snapshot.

## Current 0.1.7a focus

Wagons can permanently own repeating loop timetables. The player defines a base route such as `A-B-C-D`; runtime expands it to `A-B-C-D-C-B-A`. Arrival and departure are explicit control-point times for every point, including the return direction.

Timetable state belongs to the wagon and therefore survives coupling and decoupling as part of the wagon's runtime state. The timetable does not command locomotives: movement remains governed by signals and dispatcher actions.

The existing `S` wagon route menu is the timetable editor. It keeps route editing in the established workflow and adds full-loop arrival/departure control-point entry.

Runtime save schema is `2`; schedule definition and runtime state are persisted with each wagon in `trains.json`.

## Version policy

- `0.1.7a` is the current development snapshot.
- `0.1.6pre` and earlier consolidated/lettered snapshots are historical.
- Historical release notes remain immutable.
- Maintained architecture/domain documentation is updated when the current contract changes.
