# Content and platforms

## Content root

`RailDispatchMonoGame` sets `Content.RootDirectory = "Content"`.

The Core project contains a `Content` directory with game assets. Myra 1.6.5 is consumed as a NuGet dependency and does not require a copied project-local Myra asset tree for the standard integration used by this stage.

`ScreenManager` loads shared rendering resources from the content system, including:

- `Fonts/Hud`
- `Sprites/blank`

These asset names are part of the current runtime contract. Renaming an asset requires updating every load site and the content pipeline/output accordingly.

## Platform projects

The repository includes platform-specific hosts. Android contains its platform bootstrap and resources, while desktop hosts provide the desktop MonoGame configuration. Myra is referenced by the shared Core project so UI screens can remain in shared code.

## Content ownership

Shared game assets belong under Core's content area when they are required by shared gameplay/screens. Platform-only assets should stay with the platform project.

Myra itself is a library dependency rather than a duplicated content subtree. Do not copy Myra's source or binaries into the repository unless a later platform-specific requirement explicitly justifies it.

## Content-loading caution

The existing screen/content lifecycle remains authoritative. Myra initialization occurs once in `RailDispatchMonoGame.LoadContent()`. Do not add independent Myra initialization calls to every screen.
