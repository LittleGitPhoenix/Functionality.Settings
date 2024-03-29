# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 3.2.0

:calendar: _2024-03-07_

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_minus_sign: ~~5.0~~ :heavy_check_mark: 6.0 :new: 8.0 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

### References

:large_blue_circle: Phoenix.Functionality.Settings  ~~3.1.0~~ → [**3.3.0**](../../Settings/⬙/CHANGELOG.md#3.3.0)
___

## 3.1.0

:calendar: _2022-05-10_

### Added

- Implemented `ISettingsManager.Delete()`.

### References

:large_blue_circle: Phoenix.Functionality.Settings  ~~3.0.0~~ → [**3.1.0**](../../Settings/⬙/CHANGELOG.md#3.1.0)
___

## 3.0.0

:calendar: _2022-02-12_

### Added

- The new `UsingEncryption` extension method that can be used with the overhauled builder pattern of **Phoenix.Functionality.Settings.Encryption v3.x**.
___

## 2.3.0

:calendar: _2022-01-15_

### Added

- The project now natively supports **.NET 6**.
___

## 2.2.0

:calendar: _2021-12-16_

### Added

- Added new attributes `EncryptForceFollowAttribute` and `EncryptDoNotFollowAttribute` that allow to override the automatic mechanism for checking if properties are nested and need to be recursively inspected. Specifying both for the same property will lead to Armageddon, or maybe only `EncryptDoNotFollowAttribute` taking precedence.

### Fixed

- Detecting **.NET** build-in types failed when running an application published as single-file executable because in those cases neither **Type.Module.FullyQualifiedName** nor **Type.Assembly.Location** are set and therefore cannot be used to check if a type is from the framework itself. Since this cannot be bypassed in another way, now all types whose assembly/module starts with **System.** are treated as build-in. For cases where this leads to undesired behavior, the two new attributes `EncryptForceFollowAttribute` and `EncryptDoNotFollowAttribute` have been created.
___

## 2.1.0

:calendar: _2021-12-11_

### Added

- The **Key** and **Vector** used for en-/decryption can now by specified when constructing an `EncryptSettingsManager`.

### Fixed

- When filtering properties of an `ISettings` class that must be encrypted, proper error handling was missing. In some cases where accessing the values of properties via reflection failed, this broke loading the settings altogether.
- Adding the `Encrypt` attribute to collections was not working as expected. For example attributing a **collection of strings** would not work at all. This has been fixed. Currently the following collections can be attributed:
	- Arrays, both simple (**string\[\]**) and stacked (**string\[\]\[\]**)
	- Lists, both simple (**IList\<string\>**) and stacked (**IList\<IList\<string\>\>**)
	- Custom classes inheriting from **IList<>**
___

## 2.0.0

:calendar: _2021-11-01_

- Initial release

> Version starts with **2.0.0** to keep in line with the rest of the projects and their change to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html).