# Current State — 0.1.2a

`0.1.2a` is the first infrastructure stage of the Myra UI integration series.

## Implemented

- Added the standard `Myra` NuGet package, version `1.6.5`, to `RailDispatchMono.Core`.
- Added `MyraUIManager` as the shared Myra integration boundary.
- `RailDispatchMonoGame` initializes Myra once during `LoadContent()`.
- A shared Myra `Desktop` is created and retained by `MyraUIManager`.
- Existing `ScreenManager`, screen lifecycle and input architecture remain unchanged.

## Not yet implemented

- Existing Main Menu migration to Myra.
- Settings migration to Myra.
- Pause migration to Myra.
- Myra-backed dialogs/message boxes.
- Myra-specific input routing beyond the future integration boundary.
- Global Myra rendering.

These are intentionally deferred to later `0.1.2x` stages.

## Development rule

Each `0.1.2x` stage is an immutable incremental commit. If a build/runtime test exposes a defect in this stage, do not rewrite `0.1.2a`; implement the correction as `0.1.2b`.
