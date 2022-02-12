# TODO

All planned changes to this project will be documented in this file.
___

## Functionality

- [ ] Implement hot-reload functionality for `ISettings`.
	- This needs some kind of wrapper class, that holds a (single) `ISettings` instance. Something like **IHotReloadable\<MySettings\>**.
	- Accessing the instance itself, should not happen via an exposed property like **Value** or **Instance**, as this leads to cases where the settings will be accessed in between reloads of the instance. Better would be to access it via a using directive, that returns a copied reference of the instance at a single point in time.
	
- [ ] Create a **SQLite** sink.
- [x] ~~Separate storage from serialization.~~
- [x] ~~Add some kind of version management to the settings instances, so that upgrades can be performed.~~
- [x] ~~Make settings save-able via extension methods of the **_ISettings_** interface.~~

___

## Unit Tests

- ~~Add unit tests for the custom converters.~~
	- [x] ~~FileSystemInfoConverter~~
	- [x] ~~IpAddressConverter~~
	- [x] ~~RegexConverter~~
	- [x] ~~TimeSpanConverter~~

