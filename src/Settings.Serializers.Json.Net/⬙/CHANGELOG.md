# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 3.3.0

:calendar: _2023-03-28_

### Fixed

- The custom `EnumConverter` did not work for **Nullable** enumerations. Using such lead to an **InvalidOperationException**. This was due to the **System.Text.Json.JsonSerializer** not passing null values to the custom converter at all (see [here](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-6-0#handle-null-values)).

### Removed

- The already obsolete enumeration `IWriteOutOptions` **as comment** and **as property** have been removed.
___

## 3.2.0

:calendar: _2022-12-30_

### Added

- Added new custom converter `EnumConverter` that can write-out all values of enumeration properties during serialization.

### Fixed

- The `VersionConverter` could not be added via builder pattern because of missing extension methods.
___

## 3.1.0

:calendar: _2022-05-10_

### Added

- Added new custom converter `VersionConverter`.

### Fixed

- Comparing the structure of a settings instance with the underlying data potentially threw a `SettingsLoadException` because no serialization options where used for this process (e.g. no trailing comma handling). Now the `JsonSettingsSerializer` uses the same **JsonSerializerOptions** options for all its operations.

### References

:large_blue_circle: Phoenix.Functionality.Settings  ~~3.0.0~~ → **3.1.0**
___

## 3.0.0

:calendar: _2022-02-12_

- Initial release

> Versioning starts with **3.0.0** to keep in line with the **Phoenix.Functionality.Settings** project.