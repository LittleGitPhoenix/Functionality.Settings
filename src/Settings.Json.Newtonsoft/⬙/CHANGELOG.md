# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

> **Deprecated**
>
> This `ISettingsManager` has been deprecated. All of the required functionality that **Newtonsoft.Json** provides is now available from the **.NET** build in **System.Text.Json** class. So **Phoenix.Functionality.Settings.Json.Net** can be used as a drop-in replacement.

___

## 2.0.0 (2021-11-01)

### Changed

- Changed license to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html).

### References

:large_blue_circle: Phoenix.Functionality.Settings ~~1.1.0~~ → **2.0.0**
:large_blue_circle: Newtonsoft.Json ~~12.0.3~~ → **13.0.1**
___

## 1.1.0 (2020-02-09)

### Changed

- Calling **_SettingsExtensions.InitializeExtensionMethods_** any time a settings instance has been loaded, so that the extension methods can properly operate.
___

## 1.0.0 (2019-10-12)

- Initial release