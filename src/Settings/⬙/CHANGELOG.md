# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 2.0.0 (2021-11-01)

### Changed

- Changed license to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html).

### Removed

- The `SettingsManager` interface does no longer expose an `ISettingsCache`via the `Cache` property. Caching is now an internal implementation detail.
___

## 1.1.0 (2020-02-09)

### Added

- Added extension methods `Save` and `Reload` to the `ISettings` interface. Those will only work, if the corresponding settings instance has been linked to an `ISettingsManager` via the `SettingsExtensions.InitializeExtensionMethods` method. When loading settings via any of the provided `SettingsManager` implementations, linking is done automatically.
___

## 1.0.0 (2019-10-12)

- Initial release