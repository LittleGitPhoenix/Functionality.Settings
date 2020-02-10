#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Phoenix.Functionality.Settings.Cache;

namespace Phoenix.Functionality.Settings.Json.Newtonsoft
{
	/// <summary>
	/// <see cref="ISettingsManager"/> utilizing <c>JSON.Net</c>.
	/// </summary>
	public class JsonSettingsManager : ISettingsManager
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private readonly object _lock;

		///// <summary> <see cref="TypeValueConverter"/> used for implicit conversion of values to a concrete type. </summary>
		//private static readonly TypeValueConverter TypeValueConverter;

		/// <summary>
		/// The directory where all setting files reside.
		/// </summary>
		/// <remarks> By default this is a folder named '.settings' in the <see cref="Directory.GetCurrentDirectory"/>. </remarks>
		private readonly DirectoryInfo _baseDirectory;
		
		#endregion

		#region Properties
		
		/// <inheritdoc />
		public ISettingsCache Cache { get; }

		#endregion

		#region Enumerations

		enum SettingsOrigin
		{
			Default,
			Cache,
			File
		}

		#endregion

		#region (De)Constructors

		//static JsonSettingsManager()
		//{
		//	TypeValueConverter = new TypeValueConverter();
		//}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks> As base folder for the settings files a folder named '.settings' in the <see cref="Directory.GetCurrentDirectory"/> will be used. </remarks>
		public JsonSettingsManager() : this(null, null) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="cache"> An <see cref="ISettingsCache"/> used for caching all loaded settings instances. </param>
		/// <remarks> As base folder for the settings files a folder named '.settings' in the <see cref="Directory.GetCurrentDirectory"/> will be used. </remarks>
		public JsonSettingsManager(ISettingsCache cache) : this(null, cache) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="baseDirectory"> The directory where all setting files reside. </param>
		/// <param name="cache"> An <see cref="ISettingsCache"/> used for caching all loaded settings instances. If this is <c>Null</c> then <see cref="NoSettingsCache"/> will be used. </param>
		public JsonSettingsManager(DirectoryInfo baseDirectory, ISettingsCache cache)
		{
			// Save parameters.
			_baseDirectory = baseDirectory ?? new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), ".settings"));
			this.Cache = cache ?? new NoSettingsCache();

			// Initialize fields.
			_lock = new object();

			// Create the settings base folder.
			_baseDirectory.Create();
		}

		/// <summary>
		/// Creates a new <see cref="JsonSettingsManagerBuilder"/>.
		/// </summary>
		/// <returns> A new <see cref="JsonSettingsManagerBuilder"/>. </returns>
		public static IDirectoryJsonSettingsManagerBuilder Construct()
		{
			return new JsonSettingsManagerBuilder();
		}

		#endregion

		#region Methods
		
		#region Load

		/// <inheritdoc />
		public TSettings Load<TSettings>(bool bypassCache = false, bool preventUpdate = false) where TSettings : class, ISettings, new()
		{
			lock (_lock)
			{
				// Get a new instance or a cached version of the settings.
				var (origin, settings, settingsFile) = this.GetSettings<TSettings>(bypassCache);
				
				// If the settings where loaded from cache, directly return the instance.
				if (origin == SettingsOrigin.Cache) return settings;

				// If the settings are the default instance, then save those to a file if necessary and return it.
				if (origin == SettingsOrigin.Default)
				{
					if (!preventUpdate) JsonSettingsManager.Save(settings, settingsFile);
					return settings;
				}

				// Save the changed settings if necessary.
				if (!preventUpdate && JsonSettingsManager.ShouldSettingsFileBeUpdated(settingsFile, settings)) this.Save(settings, createBackup: true);

				// Setup usage of the special extension methods.
				settings.InitializeExtensionMethods(this);

				// Return the loaded settings.
				return settings;
			}
		}

		private (SettingsOrigin Origin, TSettings Settings, FileInfo SettingsFile) GetSettings<TSettings>(bool bypassCache) where TSettings : class, ISettings, new()
		{
			SettingsOrigin origin;
			TSettings settings;
			FileInfo settingsFile = this.GetSettingsFile<TSettings>();

			if (!bypassCache && this.Cache.TryGet(out settings))
			{
				origin = SettingsOrigin.Cache;
			}
			else
			{
				// Check if the settings file is available.
				if (!settingsFile.Exists)
				{
					// NO: Create a default instance.
					origin = SettingsOrigin.Default;
					settings = JsonSettingsManager.GetDefaultInstance<TSettings>();
				}
				else
				{
					// YES: Deserialize it.
					origin = SettingsOrigin.File;
					settings = JsonSettingsManager.Deserialize<TSettings>(settingsFile);
				}

				// Update the cache if needed.
				if (!bypassCache) this.Cache.AddOrUpdate(settings);
			}

			return (origin, settings, settingsFile);
		}

		/// <summary>
		/// Deserializes the given file into an <see cref="System.Dynamic.ExpandoObject"/>.
		/// </summary>
		/// <param name="settingsFile"> The settings file to deserialize. </param>
		/// <returns> The deserialized settings object. </returns>
		private static TSettings Deserialize<TSettings>(FileInfo settingsFile) where TSettings : class, ISettings, new()
		{
			if (settingsFile == null) throw new ArgumentNullException(nameof(settingsFile));
			if (!settingsFile.Exists) throw new ArgumentException($"The settings file '{settingsFile.FullName}' does not exist.", nameof(settingsFile));

			//void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
			//{
			//	// To continue further deserialization of the file, tell Json.NET that this error has been handled.
			//	args.ErrorContext.Handled = true;
			//}

			////var serializer = new JsonSerializer()
			////{
			////	MissingMemberHandling = MissingMemberHandling.Ignore,
			////	DateFormatHandling = DateFormatHandling.IsoDateFormat,
			////	ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
			////	ObjectCreationHandling = ObjectCreationHandling.Replace, // This options lets Json.Net override lists that are filled with initial data instead of adding deserialized items to the existing list.
			////	TypeNameHandling = TypeNameHandling.None,                // If the settings object is more complex, it may be necessary to use 'TypeNameHandling.Auto' here.
			////	Converters =
			////	{
			////		new Newtonsoft.Json.Converters.StringEnumConverter(), // Save enumerations with their name rather than their number.
			////		new Phoenix.Functionality.Settings.Json.CustomJsonConverters.FileSystemInfoConverter(),
			////		new Phoenix.Functionality.Settings.Json.CustomJsonConverters.TimeSpanConverter(),
			////		new Phoenix.Functionality.Settings.Json.CustomJsonConverters.RegexConverter(),
			////	}
			////};
			//var serializer = JsonSerializer.Create(JsonOptions.Instance);
			//serializer.Error += HandleDeserializationError;

			//try
			//{
			//	using (StreamReader streamReader = settingsFile.OpenText())
			//	using (JsonReader jsonReader = new JsonTextReader(reader: streamReader))
			//	{
			//		//// Directly deserialize the file into an expando object.
			//		//var tempExpando = serializer.Deserialize<System.Dynamic.ExpandoObject>(jsonReader);
			//		//return tempExpando;

			//		// Directly deserialize the file into an instance of TSettings.
			//		var instance = serializer.Deserialize<TSettings>(jsonReader);
			//		return instance;
			//	}
			//}
			//finally
			//{
			//	serializer.Error -= HandleDeserializationError;
			//}

			using (var reader = settingsFile.OpenText())
			{
				var json = reader.ReadToEnd();
				return JsonConvert.DeserializeObject<TSettings>(json, JsonOptions.Instance);
			}
		}

		/// <summary>
		/// Compares the contents of the <paramref name="settingsFile"/> with the deserialized <paramref name="settings"/> instance.
		/// </summary>
		/// <param name="settingsFile"> The underlying settings file. </param>
		/// <param name="settings"> The deserialized settings instance. </param>
		/// <returns> <c>True</c> if they match, otherwise <c>False</c>. </returns>
		private static bool ShouldSettingsFileBeUpdated(FileInfo settingsFile, object settings)
		{
			var target = JObject.Parse(JsonConvert.SerializeObject(settings, JsonOptions.Instance));
			var actual = JObject.Parse(File.ReadAllText(settingsFile.FullName));
			var areIdentical = JToken.DeepEquals(target, actual);
			return !areIdentical;
		}

		//private static bool TransferContent(object source, object destination)
		//{
		//	var properties = destination
		//		.GetType()
		//		.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
		//		.Where(info => info.CanWrite)
		//		.ToList()
		//		;

		//	bool somethingChanged = false;
		//	foreach (var property in properties)
		//	{
		//		somethingChanged |= JsonSettingsManager.TransferPropertyValue(property, source, destination);
		//	}
		//	return somethingChanged;
		//}

		//private static bool TransferPropertyValue(PropertyInfo property, object source, object destination)
		//{
		//	// If no source is available then obviously nothing will be transferred, but the settings need to be re-saved.
		//	if (source is null) return true;

		//	// Get current value and type.
		//	var currentValue = property.GetValue(destination);
		//	var currentValueType = property.PropertyType;
		//	//var currentIsNullable = !currentValueType.IsValueType || Nullable.GetUnderlyingType(currentValueType) != null;

		//	// Get new value and type.
		//	object newValue;
		//	Type newValueType;
		//	if (source is System.Dynamic.ExpandoObject)
		//	{
		//		bool hasNewValue = ((IDictionary<string, object>) source).TryGetValue(property.Name, out newValue);

		//		// If no source is available then obviously nothing will be transferred, but the settings need to be re-saved.
		//		if (!hasNewValue) return true;

		//		// If the source is an expando object, than set its type to match the destination.
		//		newValueType = currentValueType;
		//	}
		//	else
		//	{
		//		var sourceProperty = source.GetType().GetProperty(property.Name);

		//		// If no source is available then obviously nothing will be transferred, but the settings need to be re-saved.
		//		if (sourceProperty is null) return true;

		//		newValue = sourceProperty.GetValue(source);

		//		// Use the type of the source property.
		//		newValueType = sourceProperty.PropertyType;
		//	}

		//	//// Try to create a more concrete new value.
		//	//newValue = JsonSettingsManager.TypeValueConverter.ConvertValue(newValue, currentValueType);

		//	// If the destination and the source type are not the same (anymore) then nothing will be transferred, but the settings need to be re-saved.
		//	if (!currentValueType.IsAssignableFrom(newValueType)) return true;

		//	var directReplace = !(currentValueType.IsClass && property.GetCustomAttribute<NestedSettingsAttribute>() != null);
		//	if (directReplace)
		//	{
		//		if (!Object.Equals(currentValue, newValue))
		//		{
		//			JsonSettingsManager.ApplyPropertyValue(property, destination, newValue);
		//		}
		//		return false;
		//	}
		//	else
		//	{
		//		// Recursively process this property.
		//		return JsonSettingsManager.TransferContent(newValue , currentValue);
		//	}
		//}

		//private static void ApplyPropertyValue(PropertyInfo property, object containingInstance, object newValue)
		//{
		//	var propertyType = property.PropertyType;

		//	try
		//	{
		//		// Try to directly set the value.
		//		property.SetValue(containingInstance, newValue);
		//	}
		//	catch (Exception ex)
		//	{
		//		// Try it via a TypeConverter.
		//		var converter = TypeDescriptor.GetConverter(propertyType);
		//		newValue = converter.ConvertTo(newValue, propertyType);
		//		property.SetValue(containingInstance, newValue);
		//	}
		//}

		#endregion

		#region Save

		/// <inheritdoc />
		public void Save<TSettings>(TSettings settings, bool createBackup = default) where TSettings : ISettings
		{
			lock (_lock)
			{
				JsonSettingsManager.Save(settings, this.GetSettingsFile<TSettings>(), createBackup);
			}
		}

		private static void Save<TSettings>(TSettings settings, FileInfo settingsFile, bool createBackup = default) where TSettings : ISettings
		{
			// Create a backup.
			if (createBackup) JsonSettingsManager.CreateBackup(settingsFile);
			
			// Serialize the settings instance into a json string.
			var jsonString = JsonSettingsManager.Serialize(settings);

			// Save the json string to file.
			JsonSettingsManager.Save(jsonString, settingsFile);
		}

		/// <summary>
		/// Saves the <paramref name="jsonString"/> to the <paramref name="settingsFile"/>.
		/// </summary>
		/// <param name="jsonString"> The json string to save. </param>
		/// <param name="settingsFile"> The <see cref="FileInfo"/> reference where the json string will be saved. </param>
		/// <remarks> If the passed json string is null, empty or contains only whitespaces and the file exists, it will be deleted. </remarks>
		private static void Save(string jsonString, FileInfo settingsFile)
		{
			if (String.IsNullOrWhiteSpace(jsonString))
			{
				if (settingsFile.Exists) settingsFile.Delete();
				return;
			}

			// Check if the directory of the file exists. Trying to create a file, whose directory is also not existing, will throw an exception.
			if (!settingsFile.Directory.Exists) settingsFile.Directory.Create();

			using (var fileStream = settingsFile.Open(FileMode.Create))
			{
				var jsonData = System.Text.Encoding.UTF8.GetBytes(jsonString);
				fileStream.Write(jsonData, offset: 0, count: jsonData.Length);
			}
		}

		/// <summary>
		/// Serializes the <paramref name="settings"/> instance into a json string.
		/// </summary>
		/// <param name="settings"> The concrete settings instance. </param>
		/// <returns> The serialized json string or <c>NULL</c>. </returns>
		private static string Serialize(object settings)
		{
			// Serialize the given json type back to a string.
			//using (StringWriter stringWriter = new StringWriter())
			//using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
			//{
			//	// Setup tab based formatting.
			//	jsonTextWriter.Indentation = 1;
			//	jsonTextWriter.IndentChar = Convert.ToChar(9); // TAB
			//	jsonTextWriter.Formatting = Newtonsoft.Json.Formatting.Indented;

			//	// Instantiate the serializer and let it work its magic.
			//	var serializer = JsonSerializer.Create(JsonOptions.Instance);
			//	serializer.Serialize(jsonWriter: jsonTextWriter, value: settings);
			//	return stringWriter.ToString();
			//}

			return JsonConvert.SerializeObject(settings, Formatting.Indented, JsonOptions.Instance);
		}

		/// <summary>
		/// Creates a backup of the <paramref name="settingsFile"/>. The backup will be suffixed by 'yyyy-MM-dd_HH-mm-ss'. Existing backup files will be overridden.
		/// </summary>
		/// <param name="settingsFile"> The settings file of which to create a backup. </param>
		private static void CreateBackup(FileInfo settingsFile)
		{
			settingsFile.Refresh();
			if (settingsFile.Exists)
			{
				// Create a backup of the current file.
				var backupFolder = Path.Combine(settingsFile.DirectoryName, ".backup");
				var backupFile = $"{Path.GetFileNameWithoutExtension(settingsFile.Name)}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}{settingsFile.Extension}";

				Directory.CreateDirectory(backupFolder);
				settingsFile.CopyTo(Path.Combine(backupFolder, backupFile), overwrite: true);
			}
		}

		#endregion

		#region Helper
		
		/// <summary>
		/// Creates a default settings instance.
		/// </summary>
		/// <typeparam name="TSettings"> The settings type. </typeparam>
		/// <returns> A new instance of <typeparamref name="TSettings"/>. </returns>
		private static TSettings GetDefaultInstance<TSettings>() where TSettings : class, ISettings, new()
		{
			// Create a default instance.
			return new TSettings();
		}

		/// <summary>
		/// Gets a reference to the settings file.
		/// </summary>
		/// <typeparam name="TSettings"> The settings type. </typeparam>
		/// <returns> A <see cref="FileInfo"/> reference to the settings file. </returns>
		private FileInfo GetSettingsFile<TSettings>() where TSettings : ISettings
			=> JsonSettingsManager.GetSettingsFile(this.GetSettingsFileNameWithoutExtension<TSettings>(), _baseDirectory);

		/// <summary>
		/// Gets a reference to the settings file.
		/// </summary>
		/// <param name="settingsName"> The name of the settings file. </param>
		/// <param name="settingsDirectory"> The base directory of the settings. </param>
		/// <returns> A <see cref="FileInfo"/> reference to the settings file. </returns>
		private static FileInfo GetSettingsFile(string settingsName, DirectoryInfo settingsDirectory)
		{
			var fileName = $"{settingsName}.json";
			return new FileInfo(Path.Combine(settingsDirectory.FullName, fileName));
		}
		
		#endregion

		#endregion
	}
}