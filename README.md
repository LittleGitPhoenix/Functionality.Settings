# Phoenix.Functionality.Settings

| .NET Framework | .NET Standard | .NET Core |
| :-: | :-: | :-: |
| :heavy_check_mark: 4.6.1 | :heavy_check_mark: 2.0 | :heavy_check_mark: 2.0 |

Contains assemblies that provide a common way for accessing application settings.
___

# General principle

Settings are represented as a simple poco class within an application, so that the program can access its defined properties. In the background, those settings are stored in some kind of external storage. This storage could be text files of different formats like the commonly used xml or json files. A typical settings provider now has the task of synchronizing the externally stored settings with the settings class of the application.

# Usage

To provide settings for an application, a simple class containing the relevant properties has to be created. To use the class with this settings library, it must implement the empty **_ISettings_** interface. Loading and saving the settings is the responsibility of an **_ISettingsManager_**. The concrete implementation of such an **_ISettingsManager_** handles the details of synchronizing the data source and the settings class.

Here is an example showing how an **_ISettingsManager_** is used to obtain a settings instance:

``` csharp
var settingsManager = new ISettingsManager(); // Obviously this has to be replace with a concrete class.
var settings = settingsManager.Load<Settings>();
```

Each settings manager should provide an internal **_ISettingsCache_**, so that loading the same settings class multiple times, will always return the same instance.

# ISettingsManager Implementations

Concrete implementations of **_ISettingsManager_** are provided as separate NuGet packages. Currently the following are available.
___

**_Json.Net_**

| .NET Framework | .NET Standard | .NET Core |
| :-: | :-: | :-: |
| :heavy_check_mark: 4.6.1 | :heavy_check_mark: 2.0 | :heavy_check_mark: 2.0 |

This settings manager uses json files as storage. Serialization and deserialization is based on **_System.Text.Json_**. The file name of the settings file must equal the class name suffixed by ".json". This can be overridden by **_SettingsFileNameAttribute_**.

> **Restrictions:**  
Currently only dictionaries of type **Dictionary<string, TPrimitiveType>** can be (de)serialization by this manager.>

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

**_Json.Newtonsoft_**

| .NET Framework | .NET Standard | .NET Core |
| :-: | :-: | :-: |
| :heavy_check_mark: 4.6.1 | :heavy_check_mark: 2.0 | :heavy_check_mark: 2.0 |

This settings manager uses json files as storage. Serialization and deserialization is based on **_Newtonsoft.Json_**. The file name of the settings file must equal the class name suffixed by ".json". This can be overridden by **_SettingsFileNameAttribute_**.

> **Restrictions:**  
The Newtonsoft (de)serializer prevents dynamically via **_System.Runtime.Loader.AssemblyLoadContext_** loaded assemblies to get properly unloaded, due to not properly released references.>

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

# Attributes

For modifying certain behavior to or add more information about settings, the following attributes are available:

- **_SettingsFileNameAttribute_**

Defines a custom name for an **_ISettings_** class, that may be used to find the proper data source.

- **_SettingsDescriptionAttribute_**

A custom description for an **_ISettings_** class or one of its properties.

# Extension Methods

To make using **_ISettings_** classes as easy as possible, some convenient extension methods are available. In order for those methods to properly function, it has to be known which **_ISettingsManager_** is responsible for (de)serializing the settings instance. To provide this manager, a method that is currently hidden from **IntelliSense** named **_SettingsExtensions.InitializeExtensionMethods(this ISettings settings, ISettingsManager settingsManager)_** must be invoked prior to using any of the other methods. When using one of the above **_ISettingsManager_** implementations, this method will automatically be called upon loading a settings instance.

- **_ISettings.Save_**

This saves the current settings instance to the data store.

- **_ISettings.Reload_**

This reloads and returns the settings. Note that the original settings passed to the extension method will stay unchanged.

# Authors

* **Felix Leistner** - _Initial release_