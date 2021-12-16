# Phoenix.Functionality.Settings

Contains assemblies that provide a common way for accessing application settings.
___

# General principle

Settings are represented as a simple poco class within an application, so that the program can access its defined properties. In the background, those settings are stored in some kind of external storage. This storage could be text files of different formats like the commonly used **xml** or **json** files. A typical settings provider now has the task of synchronizing the externally stored settings with the settings class of the application.
___

# Usage

To provide settings for an application, a simple class containing the relevant properties has to be created. To use the class with this library, it must implement the empty `ISettings` interface. Loading and saving the settings is the responsibility of an `ISettingsManager`. The specific implementation of such an `ISettingsManager` handles the details of synchronizing the data source and the settings class.

Here is an example showing how an `ISettingsManager` is used to obtain a settings instance:

``` csharp
class MySettings : ISettings {}

var settingsManager = new ISettingsManager(); // Obviously this has to be replace with a specific class.
var settings = settingsManager.Load<MySettings>();
```
___

# SettingsManager Implementations

Specific implementations of `ISettingsManager` are provided as separate NuGet packages. Currently the following are available.
___

## Json.Net

| .NET Framework | .NET Standard | .NET |
| :-: | :-: | :-: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 |

This settings manager uses json files as storage. Serialization and deserialization is based on **System.Text.Json**. The file name of the settings file must equal the class name suffixed by **.json**. This can be overridden by `SettingsFileNameAttribute`.

**Creation** via fluent syntax is supported.

``` csharp
var settingsManager = JsonSettingsManager
                      .Construct()
                                            
                      .UseDefaultDirectory()                          // "[ApplicationFolder]/.settings" is used.
                      .WithDirectory(string settingsDirectoryPath)    // The specified path is used.
                      .WithDirectory(DirectoryInfo settingsDirectory) // The specified directory is used.
                      
                      .WithoutCache()                                 // Do not use file caching.
                      .WithCache()                                    // Use normal file caching.
                      .WithWeakCache()                                // Files are only cached as long as something still references them
                      
                      .Build();
```

### Custom Converters

The **Phoenix.Functionality.Settings.Json.Net** provides special converters that allow for some common types to be used directly within settings classes. Those converters convert types to and from strings, so that they can be stored in a settings file.

- `FileInfoConverter`

	Relative path are only supported with two parent folders (e.g **../.../MyFolder/Some.file**). Other files will be saved as absolute path.

- `DirectoryInfoConverter`

	Relative path are only supported with two parent folders (e.g **../.../MyFolder**). Other directories will be saved as absolute path.

- `IpAddressConverter`

- `RegexConverter`

- `TimeSpanConverter`

	The string representation is in milliseconds.

## Json.Newtonsoft

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #ff0000; background-color: #ff000020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Deprecated</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		This <i>ISettingsManager</i> has been deprecated. All of the required functionality that <b>Newtonsoft.Json</b> provides is now available from the <b>.NET</b> build in <b>System.Text.Json</b> class. So <b>Phoenix.Functionality.Settings.Json.Net</b> can be used as a drop-in replacement.
    </div>
</div>

| .NET Framework | .NET Standard | .NET |
| :-: | :-: | :-: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 |

This settings manager uses json files as storage. Serialization and deserialization is based on **Newtonsoft.Json**. The file name of the settings file must equal the class name suffixed by **.json**. This can be overridden by `SettingsFileNameAttribute`.

> **Restrictions:**  
> The Newtonsoft (de)serializer prevents assemblies that have been loaded dynamically via **System.Runtime.Loader.AssemblyLoadContext** from getting unloaded, due to not properly released references.

**Creation** via fluent syntax is supported.
``` csharp
var settingsManager = JsonSettingsManager
                      .Construct()
                                            
                      .UseDefaultDirectory()                          // "[ApplicationFolder]/.settings" is used.
                      .WithDirectory(string settingsDirectoryPath)    // The specified path is used.
                      .WithDirectory(DirectoryInfo settingsDirectory) // The specified directory is used.
                      
                      .WithoutCache()                                 // Do not use file caching.
                      .WithCache()                                    // Use normal file caching.
                      .WithWeakCache()                                // Files are only cached as long as something still references them
                      
                      .Build();
```
___

# Caching

Each settings manager should provide an internal cache, so that loading the same settings class multiple times, will always return the same instance. The following simple cache classes are available. Each of them implements the `ISettingsCache`interface.

## NoSettingsCache

This variant obviously does not cache anything. It is the default cache used by all available `ISettingsManager`s if not specified otherwise.

## SettingsCache

This cache stores references to all loaded `ISetting`s instances and can be queried by an `ISettingsManager`.

## WeakSettingsCache

This cache stores all loaded `ISetting`s instances as **WeakReference**. This allows settings instances to be collected by the garbage collector.

___

# Attributes

For modifying certain behavior to or add more information about settings, the following attributes are available:

| Attribute Name | Description |
| :- | :- |
| `SettingsFileNameAttribute` | Defines a custom name for an `ISettings` class, that may be used to find the proper data source. |
| `SettingsDescriptionAttribute` | A custom description for an `ISettings` class or one of its properties. |
___

# Saving settings

To make saving or reloading `ISettings` instances as easy as possible, some convenient extension methods are available. In order for those methods to properly function, it has to be known which `ISettingsManager` is responsible for (de)serializing the settings instance. To provide this manager, a method that is currently hidden from **IntelliSense** named `SettingsExtensions.InitializeExtensionMethods` must be invoked immediately after the instance was created by its `ISettingsManager`. When using one of the above `ISettingsManager` implementations, this method will automatically be called upon loading a settings instance.

- `ISettings.Save`

	This saves the current settings instance to the data store.

- `ISettings.Reload`

	This reloads and returns the settings. Note that the original settings passed to the extension method will stay unchanged.

___

# Encryption

| .NET Framework | .NET Standard | .NET |
| :-: | :-: | :-: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 |

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #ff0000; background-color: #ff000020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Warning</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		Please note, that this is not a cryptografic centered assembly. Therefore you shoul probably not try and use it to save your BitCoin mnemonic.
    </div>
</div>

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #37ff00; background-color: #37ff0020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Information</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		Encryption is based on <b>AesManaged</b>. The key and vector used to create the symetric encryptor can be specified with an overload of the <i>ApplyEncryption</i> extension method.
    </div>
</div>

Some information stored in setting files (like database connection strings) may contain sensitive data and should therefore be encrypted.  This can be achieved via the special `EncryptSettingsManager` . It is a decorator for any other `ISettingsManager` and handles de- or encryption of properties attributed with the `EncryptAttribute`.

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #37ff00; background-color: #37ff0020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Information</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
        Currently only <b>String</b> or <b>Object</b> properties supported. Those can also be wrapped inside arrays or lists.
    </div>
</div>
To use the `EncryptSettingsManager` simply call the extension method `ApplyEncryption` on any other `ISettingsManager` instance.

```csharp
var settingsManager = JsonSettingsManager
	.Construct()
	.UseDefaultDirectory()
	.WithCache()
	.Build()
#if RELEASE
	.ApplyEncryption()
#endif
```

When loading settings, the `EncryptSettingsManager` traverses all properties of the corresponding settings instance searching for the `EncryptAttribute` to decrypt their values, so accessing them from code will always return meaningful data. The `EncryptSettingsManager` handles nested properties as well as certain collection types and recursively traverses them as well.

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #ffd200; background-color: #ffd20020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Advice</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		To only recursively traverse user-created types the <i>EncryptSettingsManager</i> must distingusih those types from the <b>.NET</b> build-in types. To make this possible (especially within apps that are published as single-file executable) the manager will not traverse into  types that come from assemblies whose name starts with <b>System</b>.<br><br>The <b><i>EncryptForceFollowAttribute</i></b> can be used in cases where this may be undesireable, to force the <i>EncryptSettingsManager</i> to traverse the attributed property nevertheless.<br><br>The <b><i>EncryptDoNotFollowAttribute</i></b> on the other side will instruct the manager to not traverse the attributed property.
    </div>
</div>

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #ff0000; background-color: #ff000020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Warning</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		Encryption is automatically applied when a settings instance is <b>loaded</b>. This means, that loading encrypts the value in the source of the settings. It is therefore recommended to only use encryption in release builds like shown in the above example.
    </div>
</div>


___

# Authors

* **Felix Leistner**: _v1.x_ - _v2.x_ 