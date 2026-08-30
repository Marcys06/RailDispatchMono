# Settings and localization

## Settings model

`RailDispatchMonoSettings` is in `Core/Settings` and implements `INotifyPropertyChanged`.

Current properties are:

- `FullScreen` (`bool`)
- `Language` (`int`)
- `ParticleEffect` (`ParticleEffectType`)

The current default language field value is `2`, with a source comment stating that this currently means English. Do not hard-code assumptions about numeric language IDs elsewhere; use the project's localization conventions when they are available.

Property setters raise `PropertyChanged` only when the value actually changes.

## Settings storage

The repository contains platform-oriented storage classes including:

- `DesktopSettingsStorage`
- `MobileSettingsStorage`
- `ConsoleSettingsStorage`

These classes indicate that persistence is separated by platform. When modifying persistence, inspect the existing storage interface/base contract and all consumers before creating a new file format or storage service.

## Localization

Localization is a first-class concern in the project structure. Treat language selection as application/game configuration, not as a property of an individual screen.

When adding user-facing text:

1. determine whether the project already has a localization lookup mechanism;
2. add the string to the existing localization source;
3. avoid scattering literal translated strings through gameplay code;
4. avoid using numeric language IDs without consulting the existing mapping.

## Settings change flow

The expected conceptual flow is:

```text
UI / screen
   -> settings property
   -> PropertyChanged
   -> interested subsystem(s)
   -> platform persistence where applicable
```

The exact subscribers and persistence calls must be verified from current call sites before documenting a more specific runtime sequence.

## AI caution

Do not replace `INotifyPropertyChanged` with a new event system simply to add a setting. Extend the existing model and follow the current persistence mechanism.
