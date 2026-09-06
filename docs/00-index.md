# RailDispatchMono Documentation

**Documentation baseline: `0.2.4`**  
**Previous consolidated milestone: `0.2.3`**

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
24. [24-current-state-0.1.7a.md](24-current-state-0.1.7a.md) — historical `0.1.7a` snapshot.
25. [25-current-state-0.1.7c.md](25-current-state-0.1.7c.md) — historical `0.1.7c` snapshot.
26. [26-current-state-0.1.7d.md](26-current-state-0.1.7d.md) — historical `0.1.7d` snapshot.
27. [27-current-state-0.2.0.md](27-current-state-0.2.0.md) — historical `0.2.0` snapshot.
28. [28-current-state-0.2.1.md](28-current-state-0.2.1.md) — historical `0.2.1` snapshot.
29. [29-current-state-0.2.2.md](29-current-state-0.2.2.md) — historical `0.2.2` snapshot.
30. [30-current-state-0.2.3.md](30-current-state-0.2.3.md) — historical `0.2.3` snapshot.
31. [31-current-state-0.2.4.md](31-current-state-0.2.4.md) — authoritative `0.2.4` snapshot.
32. [roadmap-0.3.0-to-1.0.0.md](roadmap-0.3.0-to-1.0.0.md) — planned development line from infrastructure management through full release.

## Current 0.2.4 focus

0.2.4 adds infrastructure metadata to existing track segments without replacing the existing railway graph. Track geometry, operational track type, line class, traction and wear are separate concerns.

Track traction is `None`, `DC` or `AC`. Electric locomotives declare supported electrical systems; diesel locomotives remain independent of electrification. No automatic route repair or automatic infrastructure decisions are introduced.

F8 opens the infrastructure editor for the track under the cursor. It changes type, line class and traction without changing geometry, block occupancy, signals or switches. F6 remains the dispatcher override, F9 remains the locomotive timetable editor and F10 remains diagnostics.

`map.json` schema is `2` and persists the new infrastructure metadata. Old saves are not required to remain compatible.

## Roadmap

- `0.3.0` — infrastructure management: active electrification, classes, maintenance, wear;
- `0.4.0` — economy: revenue, costs, budget, investments;
- `0.5.0` — public timetable and coordination;
- `0.6.0` — passenger demand, satisfaction and modal choice;
- `0.7.0` — richer physics: curves, gradients, braking recovery;
- `0.8.0` — failures, weather and crisis management;
- `0.9.0` — inter-city network and hub stations;
- `1.0.0` — integrated full release, balance and campaign.

See [roadmap-0.3.0-to-1.0.0.md](roadmap-0.3.0-to-1.0.0.md) for the dependency plan.

## Version policy

- `0.2.4` is the current development snapshot.
- `0.2.3`, `0.2.2`, `0.2.1`, `0.2.0` and earlier snapshots are historical.
- Historical release notes remain immutable.
- Maintained architecture/domain/UI documentation is updated whenever the current contract changes.
