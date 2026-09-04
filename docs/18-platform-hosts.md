# Platform hosts

## Shared principle

Platform projects are application hosts around the shared `RailDispatchMono.Core` implementation.

The platform host should be responsible for platform lifecycle/bootstrap details and should instantiate/run `RailDispatchMonoGame` rather than reimplementing gameplay.

## Android

`RailDispatchMono.Android/MainActivity.cs` derives from `AndroidGameActivity`.

Its current `OnCreate` sequence is:

1. call the base activity implementation;
2. create `RailDispatchMonoGame`;
3. obtain the MonoGame rendering `View` from the game's service provider;
4. assign that view as the Android activity content view;
5. call `_game.Run()`.

The activity is configured as the main launcher and uses landscape sensor orientation. It also declares configuration-change handling for orientation and keyboard-related changes.

## DesktopGL

The repository contains `RailDispatchMono.DesktopGL/Program.cs`. Treat this as the desktop host entry point. Shared game logic should remain in Core.

## WindowsDX

The repository contains `RailDispatchMono.WindowsDX/Program.cs`. Treat this as the Windows DirectX host entry point. Shared game logic should remain in Core.

## iOS

The repository contains `RailDispatchMono.iOS/Program.cs`. Treat this as the iOS host entry point. Shared game logic should remain in Core.

## Solution membership

The checked-in `RailDispatchMono.slnx` and `RailDispatchMono/RailDispatchMono.sln` currently enumerate only `RailDispatchMono.Core` and `RailDispatchMono.DesktopGL`. The Android, WindowsDX and iOS host projects are still present as independent project files and reference Core. Their absence from the desktop solution is not sufficient evidence that they are obsolete, so they are retained.

## Platform-specific modifications

A platform change normally belongs in the platform project when it concerns:

- application lifecycle;
- platform manifest/declarations;
- native resources;
- platform-specific startup;
- platform-specific settings storage.

A change normally belongs in Core when it concerns:

- gameplay rules;
- shared screens;
- shared input abstractions;
- railway/train domain;
- shared rendering behavior;
- shared settings model.

## Cross-platform caution

Do not infer that all platform projects are behaviorally identical merely because they instantiate the same Core game. Read each host before changing lifecycle-sensitive code.
