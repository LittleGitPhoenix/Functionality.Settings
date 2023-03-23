# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 3.2.0

:calendar: _2023-03-??_

### Changed

- When loading `ISettings` with the `ISettingsManager.Load` function having the `preventCreation` parameter set to **true**, the new `SettingsUnavailableException` may be thrown if no settings data is available. This can be used for settings migrations. The new exception `SettingsUnavailableException` inherits from `SettingsLoadException` and therefore does not break existing implementations.

___

## 3.1.0

:calendar: _2022-05-10_

### Added

- Settings can now be deleted via the new `ISettingsManager.Delete()` or `ISettings.Delete()` methods.
- The `ISettingsCache` interface and its default implementations now have a `TryRemove()` method.

### Fixed

- Using the extension method `ISettings.Save()` could cause an error if the save process tried to obtain the name of the settings instance via `ISettingsSink.GetSettingsName()` because `ISettings.Save()` used `ISettings` as generic type parameter instead of the specific one of the instance. This has been prevented by making `ISettings.Save()` itself generic.
- Saving settings inside the `ISettingsLayoutChangedNotification.LayoutChanged` callback failed because `ISettings.InitializeExtensionMethods()` was called after the callback was invoked.
___

## 3.0.0

:calendar: _2022-02-12_

> Version **3.x** introduces a new concept off settings handling and made a breaking change necessary. More can be found in the [**README.md**](../../../README.md)

### Added

- The `SettingsManager<TSettingsData>` is now the starting point for loading settings. The generic argument `TSettingsData` defines the type of the data the settings is saved as.

### Changed

- The former `SettingsFileNameAttribute` has been renamed to `SettingsNameAttribute`.
- `GetSettingsFileNameWithoutExtension` has been renamed to `GetSettingsName` and is now an extension method of `ISettingsSink` instead of `ISettingsManager`.
- An `ISettingsManager` can be instructed to not create and save a default `ISettings` instance if no data is available. This behavior can be toggled with the `preventCreation` parameter of the `ISettingsManager.Load` function.
- If deserializing an `ISettings` instance from existing data fails, then now a `SettingsLoadException` is thrown instead of just using a default instance.

### Removed

- ~~Settings.Json.Net~~: In parts this has been replaced by **Settings.Serializers.Json.Net**
- ~~Settings.Json.Newtonsoft~~: This has been deprecated in **v2.0.0** and is now completely removed
___

## 2.1.0

:calendar: _2022-01-15_

### Added

- The project now natively supports **.NET 6**.

### Changed

- The generic type of the `Reload` extension method of an `ISettings` instance now automatically matches the type of the instance. Therefore will no longer throw an `SettingsTypeMismatchException` if those types mismatch.
- The `ISettingsCache` interface now enforces the `ISettings` objects to be reference types (which they should be, as loading `ISettings` via an `ISettingsManager` already enforced this).

### Deprecated

- The `SettingsTypeMismatchException` has been marked obsolete as it is no longer used.
___

## 2.0.0

:calendar: _2021-11-01_

### Changed

- Changed license to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html).

### Removed

- The `SettingsManager` interface does no longer expose an `ISettingsCache`via the `Cache` property. Caching is now an internal implementation detail.
___

## 1.1.0

:calendar: _2020-02-09_

### Added

- Added extension methods `Save` and `Reload` to the `ISettings` interface. Those will only work, if the corresponding settings instance has been linked to an `ISettingsManager` via the `SettingsExtensions.InitializeExtensionMethods` method. When loading settings via any of the provided `SettingsManager` implementations, linking is done automatically.
___

## 1.0.0

:calendar: _2019-10-12_

- Initial release