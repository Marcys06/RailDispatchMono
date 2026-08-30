# RailDispatchMono documentation

This directory is the canonical project documentation intended for both humans and AI coding agents.

## Documentation map

- `01-project-overview.md` — project purpose, technology and high-level boundaries.
- `02-repository-structure.md` — solution/project and source tree orientation.
- `03-architecture.md` — architectural responsibilities and dependency rules.
- `04-runtime-lifecycle.md` — MonoGame startup, update, draw and shutdown flow.
- `05-screen-system.md` — `GameScreen`, `ScreenManager`, transitions and input routing.
- `06-input.md` — input state, coordinate transformation and touch/desktop considerations.
- `07-game-domain.md` — railway, train, vehicle and rendering-domain components discovered in the source tree.
- `08-settings-localization.md` — settings and localization boundaries.
- `09-content-platforms.md` — Content pipeline and platform projects.
- `10-development-workflows.md` — safe procedures for adding and modifying features.
- `11-ai-agent-rules.md` — concise rules for AI agents working on this repository.
- `12-known-issues-and-cautions.md` — implementation details that can easily mislead an AI.
- `13-code-index.md` — source-file index and purpose map.
- `14-documentation-maintenance.md` — rules for keeping this documentation synchronized with code.

## Source-of-truth policy

The implementation is authoritative. Documentation must never invent behavior that is not present in source code. When documentation and code disagree, treat the code as current behavior and update the relevant documentation.

## Important current facts

- The default branch is `master`.
- The shared game implementation is in `RailDispatchMono/RailDispatchMono.Core`.
- `RailDispatchMono.Core.csproj` targets `net9.0`, enables nullable reference types, and references `MonoGame.Framework.Native` version `3.8.*`.
- `RailDispatchMonoGame` derives from `Microsoft.Xna.Framework.Game`.
- `ScreenManager` is a `DrawableGameComponent` and owns the active screen collection plus shared drawing/input infrastructure.
- `GameScreen` is the base abstraction for individual screens.
- The current desktop game bootstrap sets a 1280x720 preferred backbuffer and a fixed 60 FPS timestep.
