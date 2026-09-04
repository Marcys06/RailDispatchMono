# RailDispatchMono Documentation

**Documentation baseline: `0.1.6a` development line**  
**Previous consolidated milestone: `0.1.5pre`**

This directory contains maintained project documentation. Historical release notes belong in `docs/changelog/`; source code and call sites remain authoritative if documentation conflicts with implementation.

## Documentation index

1. [01-project-overview.md](01-project-overview.md) — project identity and technology baseline.
2. [02-repository-structure.md](02-repository-structure.md) — source tree and responsibility map.
3. [03-architecture.md](03-architecture.md) — architectural boundaries and dependencies.
4. [04-runtime-lifecycle.md](04-runtime-lifecycle.md) — game startup, update and shutdown flow.
5. [05-screen-system.md](05-screen-system.md) — screen manager and screen lifecycle.
6. [06-input.md](06-input.md) — input modes and controls.
7. [07-game-domain.md](07-game-domain.md) — railway, trains, rolling stock, stations, passengers and coupling.
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
22. [22-current-state-0.1.5pre.md](22-current-state-0.1.5pre.md) — authoritative consolidated `0.1.5pre` snapshot.

## Current 0.1.6a focus

The 0.1.6 line starts from the completed rigid-consist and movement work of `0.1.5pre` and the existing station/passenger foundation.

The passenger vertical slice is already present:

`Station → passenger generation → waiting → train arrival → alighting → boarding → dwell → departure`.

Current passenger scope is intentionally basic: random destination demand, fixed origin/destination, three passenger states, wagon capacity/route acceptance and station dwell. Transfers, advanced demand, passenger economics and persistent passenger state are not implemented.

## Version policy

- Consolidated `pre` snapshots become historical when a later development line starts.
- Lettered stages such as `0.1.6a` are development records in `docs/changelog/`.
- Historical stages are not rewritten to reflect later implementation.
- Maintained architecture/domain documentation is updated when the current contract changes.
