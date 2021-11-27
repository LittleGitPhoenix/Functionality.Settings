# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 2.2.0 (2021-11-27)

### Changed

- The relative path feature introduced in version [*2.0.0*](#2.0.0-(2021-11-01)) now uses the directory of the settings file as base path to determine if a path is relative or not. Previously the base path was the working directory of the application and therefore in most cases the applications root folder itself. Problems arose, if the working directory was changed. Then a relative path would point to the wrong directory or file. To circumvent this and get a more streamlined experience, relative path are now relative to the folder of the settings file that specified them. In addition to this change, the amount of relative folders has been increased to **three** to compensate that the initial folder is now the settings folder itself.

___

## 2.1.0 (2021-11-23)

### Changed

- If the value of a **RegEx** setting property is **null**, then the pattern **.\*** (anything) will be used.
- The project now fully uses [nullable reference types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references).

### Fixed

- A possible deadlock scenario when updating a settings file based on a changed class layout has been addressed.
___

## 2.0.0 (2021-11-01)

### Added

- Both the `FileInfoConverter` and the `DirectoryInfoConverter` now support up to **two** parent folders as relative path. This feature is only available if running at least **.NET Standard 2.1**. Everything below is restricted to absolute path.

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