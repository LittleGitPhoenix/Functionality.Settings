# Phoenix.Functionality.Settings

Contains assemblies that provide a common way for accessing application settings.
___

# Table of content

[toc]
___

# General principle

Settings should be represented as a simple poco class within an application, so that the program can access its defined properties. In the background, those settings are stored in some kind of hopefully persistent storage. This storage could be text files of different formats like the commonly used **json** files or a **database**. This settings library provides tools that handle the synchronization of stored settings with settings classes of an application.
___

# Terminology

|  | Explanation |
| :- | :- |
| [**`ISettings`**](#Settings) | This empty interface identifies settings classes that should be handled by this library. It is therefore mandatory that those classes implement it. |
| [**`ISettingsManager`**](#SettingsManager) | A manager is responsible for loading and saving `ISettings` instances thus making them accessible in the application. |
| [**`ISettingsSink`**](#Implementations-of-ISettingsSink) | A sink is responsible for retrieving settings from or storing them into a storage system. |
| [**`ISettingsSerializer`**](#Implementations-of-ISettingsSerializer) | A serializer is responsible for converting the settings data into an `ISettings` class or the other way around. |
| [**`ISettingsCache`**](#Implementations-of-ISettingsCache) | A cache may be used by a manger to improve performance. |
___

# SettingsManager

The main implementation of an `ISettingsManager` is the generic `SettingsManager<TSettingsData>`. It is the starting point for `ISettings` handling with this library. Its generic argument `TSettingsData` defines the type of the data that is used to store the settings outside of the applications life cycle. After it has been created, it is the only class required to handle settings.

The `SettingsManager<TSettingsData>` internally utilizes a generic sink of type `ISettingsSink<TSettingsData>` and a generic serializer of type `ISettingsSerializer<TSettingsData>`. The generic argument is the same for each instance of the manager. For a simple (and probably common example) lets assume that this generic argument would be of type **String**. This means that the sink has the responsibility to retrieve or store the settings as a simple string, whereas the serializer has the responsibility to (de)serialize settings to or from a string. Where the string is stored or how the string is (de)serialized is up to the specific implementation that was used to build the manager.

## Process flow

### Loading

The **SettingsManager** requests the **SettingsSink** to load settings data from its storage system. It then passes that data to the **SettingsSerializer** so it is converted into an `ISettings` class.

### Saving

The **SettingsManager** instructs the **SettingsSerializer** to convert an `ISettings`  class into settings data. This data is then passed to the **SettingsSink** for storing it to its storage system.

## Creation

The easiest way to build an `SettingsManager<TSettingsData>` is using the fluent syntax (also called builder pattern).

Here is the most basic example showing how to create a new manager. Obviously the respective sink, serializer and cache have to be created beforehand.

``` csharp
var settingsManager = SettingsManager<string>
	.Create()
	.AddSink(someSink)
	.AddSerializer(someSerializer)
	.AddCache(someCache)
	.Build()
	;
```
Since sinks, serializers and caches can provide their own fluent syntax, a more complete example could look like this:

``` csharp
using Phoenix.Functionality.Settings;
using Phoenix.Functionality.Settings.Cache;
using Phoenix.Functionality.Settings.Encryption;
using Phoenix.Functionality.Settings.Serializers.Json.Net;
using Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;
using Phoenix.Functionality.Settings.Sinks.File;

var fileExtension = ".json";
var baseDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), ".settings"));
var settingsManager = SettingsManager<string>
	.Create()
	.UsingFileSink(fileExtension, baseDirectory)
	.UsingJsonSerializer()
		.WithFileInfoConverter(baseDirectory)
		.WithDirectoryInfoConverter(baseDirectory)
		.WithIpAddressConverter()
		.WithRegexConverter()
		.WithTimeSpanConverter()
		.WithDefaultSerializerOptions()
	.UsingWeakCache()
	.UsingEncryption()
	.Build()
	;
```
___

# Settings

Settings are represented as classes implementing the empty `ISettings` interface. The purpose of that interface is a more straightforward way to identify settings throughout an application. It is also used for extension methods, that otherwise would have to extend the **Object** base class, which would be less ideal.


## Attributes

For modifying certain behavior to or add more information to settings, the following attributes are available:

| Attribute Name | Description |
| :- | :- |
| `SettingsNameAttribute` | Defines a custom name for an `ISettings` class that may be used to store the settings. By default the name equals the type name. |
| `SettingsDescriptionAttribute` | A custom description for an `ISettings` class or one of its properties. |


## Saving settings

To make saving or reloading `ISettings` instances as easy as possible, some convenient extension methods are available. In order for those methods to properly function, it has to be known which `ISettingsManager` is responsible for handling the settings instance. To provide this manager, a method that is hidden from **IntelliSense** named `SettingsExtensions.InitializeExtensionMethods` must be invoked after the instance was created. When using the `SettingsManager<TSettingsData>`, it will automatically be called for each loaded settings instance.

- `ISettings.Save`

  This saves the current settings instance to the data store.

  ```c#
  var settings = settingsManager.Load<Settings>();
  settings.InitializeExtensionMethods(settingsManager); // Not neded if using SettingsManager<TSettingsData>
  settings.MyProperty = "Change";
  settings.Save();
  ```

- `ISettings.Reload`

	This reloads and returns the settings. Note that the original settings passed to the extension method will stay unchanged.
	
	```c#
	var settings = settingsManager.Load<Settings>();
	settings.InitializeExtensionMethods(settingsManager); // Not neded if using SettingsManager<TSettingsData>
	var newSettings = settings.Reload();
	```
___

# Implementations of `ISettingsSink`

Specific implementations of `ISettingsSink`s are provided as separate NuGet packages. Currently the following are available.

## Sinks.File

| .NET Framework | .NET Standard | .NET |
| :-: | :-: | :-: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 :heavy_check_mark: 6.0 |

This sink uses the normal file system to store `Isettings` as file. The file name is created from the settings class and suffixed with a configurable extension. The name of the file itself can be changed by attributing the settings class with the `SettingsFileNameAttribute` (see [Attributes](#Attributes)).

**Creation** via fluent syntax is supported.

``` c#
var settingsManager = SettingsManager<string>
	.Create()
	// -->
	.UsingFileSink(fileExtension, baseDirectory)
	// <--
	.AddSerializer(...)
	.AddCache(...)
	.Build()
	;
```
___

# Implementations of `ISettingsSerializer` 

Specific implementations of `ISettingsSerializer`s are provided as separate NuGet packages. Currently the following are available.

## Serializers.Json.Net

This serializer converts to and from the common **JSON** format. Serialization and deserialization is leveraged to **System.Text.Json**.

**Creation** via fluent syntax is supported.

``` c#
var settingsManager = SettingsManager<string>
	.Create()
	.AddSink(...)
	// -->
	.UsingJsonSerializer()
		.WithFileInfoConverter()
		.WithDirectoryInfoConverter()
		.WithIpAddressConverter()
		.WithRegexConverter()
		.WithTimeSpanConverter()
		.WithDefaultSerializerOptions()
	// <--
	.AddCache(...)
	.Build()
	;

```

### Custom Converters

The package provides special converters that allow for some common types to be used directly within settings classes.

- `FileInfoConverter` and `DirectoryInfoConverter`

  Those converters support relative path if a _base directory_ has been specified when creating the converters. If settings are loaded and saved, the converter checks if the path of any **FileInfo** or **DirectoryInfo** property could be expressed as a path relative to the specified _base directory_.

  <div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #37ff00; background-color: #37ff0020' >
  	<span style='margin-left:1em; text-align:left'>
      	<b>Information</b>
      </span>
      <br>
  	<div style='margin-left:1em; margin-right:1em;'>
  		Relative path are only supported with up to three parent folders (e.g <b>../../.../MyFolder/Some.file</b>). Other files will be saved as absolute path.
      </div>
  </div>

- `IpAddressConverter`

- `RegexConverter`

- `TimeSpanConverter`

  The string representation of a **TimeSpan** is in **milliseconds**.

- `VersionConverter`
___

# Implementations of `ISettingsCache`

Each `ISettingsManager` should have an internal cache, so that loading the same settings class multiple times, will always return the same instance. The following simple cache classes are available. Each of them implements the `ISettingsCache`interface.

## NoSettingsCache

This variant obviously does not cache anything. It is the default cache used by `SettingsManager<TSettingsData>` if not specified otherwise.

## SettingsCache

This cache stores references to all loaded `ISetting` instances.

## WeakSettingsCache

This cache stores all loaded `ISetting`s instances as **WeakReference**. This allows those instances to be collected by the garbage collector.
___

# Encryption

| .NET Framework | .NET Standard | .NET |
| :-: | :-: | :-: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 :heavy_check_mark: 6.0 |

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

```c#
var settingsManager = new SettingsManager().ApplyEncryption();
```

It also provides support for the builder pattern of the `SettingsManager<TSettingsData>`.

```c#
var settingsManager = SettingsManager<string>
	.Create()
	.AddSink(...)
	.AddSerializer(...)
	.AddCache(...)
#if !DEBUG
	// -->
	.UsingEncryption()
	// <--
#endif
	.Build()
	;
```

When loading settings, the `EncryptSettingsManager` traverses all properties of the corresponding settings instance searching for the `EncryptAttribute` to decrypt their values, so accessing them from code will always return meaningful data. The `EncryptSettingsManager` handles nested properties as well as certain collection types and recursively traverses them as well.

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #ffd200; background-color: #ffd20020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Advice</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		To only recursively traverse user-created types the <i>EncryptSettingsManager</i> must distingusih those types from the <b>.NET</b> build-in types. To make this possible (especially within apps that are published as single-file executable) the manager will not traverse into  types that come from assemblies whose name starts with <b>System</b>.<br>The <b><i>EncryptForceFollowAttribute</i></b> can be used in cases where this may be undesireable, to force the <i>EncryptSettingsManager</i> to traverse the attributed property nevertheless.<br>The <b><i>EncryptDoNotFollowAttribute</i></b> on the other side will instruct the manager to not traverse the attributed property.
    </div>
</div>

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #ff0000; background-color: #ff000020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Warning</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		Encryption is automatically applied when a settings instance is <b>loaded</b>. This means, that loading encrypts the value in the source of the settings. It is therefore recommended to only use encryption in release builds via a preprocessor directives.
    </div>
</div>

___

# Authors

* **Felix Leistner**: _v1.x_ - _v3.x_ 