# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 2.1.0 (2021-12-11)

### Added

- The **Key** and **Vector** used for en-/decryption can now by specified when constructing an `EncryptSettingsManager`.

### Fixed

- When filtering properties of an `ISettings` class that must be encrypted, proper error handling was missing. In some cases where accessing the values of properties via reflection failed, this broke loading the settings altogether.
- Adding the `Encrypt` attribute to collections was not working as expected. For example attributing a **collection of strings** would not work at all. This has been fixed. Currently the following collections can be attributed:
	- Arrays, both simple (**string[]**) and stacked (**string[][]**)
	- Lists, both simple (**IList\<string\>**) and stacked (**IList\<IList\<string\>\>**)
	- Custom classes inheriting from **IList<>**
___

## 2.0.0 (2021-11-01)

- Initial release

> Version starts with **2.0.0** to keep in line with the rest of the projects and their change to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html).