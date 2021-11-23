# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 2.1.0 (2021-23-11)

### Changed

- If the value of a **RegEx** setting property is **null**, then the pattern **.\*** (anything) will be used.
- The project now fully uses [nullable reference types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references).

### Fixed

- A possible deadlock scenario when updating a settings file based on a changed class layout has been addressed.
___

## 2.0.0 (2021-11-01)

### Added

- Both the `FileInfoConverter` and the `DirectoryInfoConverter` now support up to two parent folders as relative path.

### Changed

- Changed license to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html).

### References

:large_blue_circle: Phoenix.Functionality.Settings ~~1.1.0~~ → **2.0.0**
:large_blue_circle: System.Text.Json ~~4.7.0~~ → **5.0.2**
___

## 1.1.0 (2020-02-09)

### Changed

- Calling **_SettingsExtensions.InitializeExtensionMethods_** any time a settings instance has been loaded, so that the extension methods can properly operate.
___

## 1.0.0 (2019-10-12)

- Initial release