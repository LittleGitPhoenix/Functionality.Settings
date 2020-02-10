# TODO

All planned changes to this project will be documented in this file.
___

## Functionality
___

- [ ] Add some kind of version management to the settings instances, so that upgrades can be performed.
> Preferably via an interface like **IUpgradeableSettings**. This could provide a method **Loaded(ISettings settings, ExpandoObject rawData)**. Later parameter is the raw serialized settings object. With this any kind of migration can be applied to the current instance.
- [x] Make settings save-able via extension methods of the **_ISettings_** interface.