# Content and platforms

## Content root

`RailDispatchMonoGame` sets `Content.RootDirectory = "Content"`.

The Core project contains a `Content` directory with game assets. The source tree currently includes a sprite font (`Arial24.spritefont`) and background assets, among other content.

`ScreenManager` loads shared rendering resources from the content system, including:

- `Fonts/Hud`
- `Sprites/blank`

These asset names are part of the current runtime contract. Renaming an asset requires updating every load site and the content pipeline/output accordingly.

## Platform projects

The repository includes platform-specific hosts. Android contains:

- `MainActivity.cs`
- `AndroidManifest.xml`
- Android resource XML files
- density-specific launcher/splash images
- `RailDispatchMono.Android.csproj`

Other platform projects exist in the solution tree. Their purpose is to provide platform bootstrap/configuration around the shared Core implementation.

## Platform flags

The current `RailDispatchMonoGame` implementation exposes static flags:

- `IsMobile => false`
- `IsDesktop => true`

These flags are consumed by input initialization. If a platform host requires different values, the shared implementation must be reviewed carefully because the current properties are hard-coded in Core.

## Content ownership

Shared game assets belong under Core's content area when they are required by shared gameplay/screens. Platform-only assets should stay with the platform project.

Do not duplicate a shared asset into each platform project without a concrete platform requirement.

## Content-loading caution

There are two relevant mechanisms in the current source: the game class explicitly calls `_gameplay.LoadContent(Content)`, while `ScreenManager` has a component-level `LoadContent()` implementation that initializes shared drawing resources and invokes `LoadContent()` on registered screens. Changes to content loading must account for both paths to avoid duplicate loading or lifecycle regressions.
