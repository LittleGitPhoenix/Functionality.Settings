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
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Phoenix.Functionality.Settings.Cache;

namespace Phoenix.Functionality.Settings.Json.Net
{
	/// <summary>
	/// <see cref="ISettingsManager"/> utilizing <see cref="JsonSerializer"/>>.
	/// </summary>
	public class JsonSettingsManager : ISettingsManager
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private readonly object _lock;
		
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
				if (!preventUpdate && JsonSettingsManager.ShouldSettingsFileBeUpdatedAsync(settingsFile, settings).Result) this.Save(settings, createBackup: true);

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
			
			using (var stream = settingsFile.Open(FileMode.Open, FileAccess.Read))
			{
				// Directly deserialize the file into an instance of TSettings.
				//? What about error handling for (partially) invalid json?
				return JsonSerializer.DeserializeAsync<TSettings>(stream, JsonOptions.Instance, CancellationToken.None).Result;
			}
		}

		/// <summary>
		/// Compares the contents of the <paramref name="settingsFile"/> with the deserialized <paramref name="settings"/> instance.
		/// </summary>
		/// <param name="settingsFile"> The underlying settings file. </param>
		/// <param name="settings"> The deserialized settings instance. </param>
		/// <returns> <c>True</c> if they match, otherwise <c>False</c>. </returns>
		public static async Task<bool> ShouldSettingsFileBeUpdatedAsync(FileInfo settingsFile, object settings)
		{
			var targetJsonDocument = JsonDocument.Parse(await JsonSettingsManager.SerializeAsync(settings));
			var targetData = await JsonSettingsManager.ConvertJsonDocumentToData(targetJsonDocument);

			var actualJsonDocument = JsonDocument.Parse(File.ReadAllText(settingsFile.FullName));
			var actualData = await JsonSettingsManager.ConvertJsonDocumentToData(actualJsonDocument);

			var areEqual = targetData.SequenceEqual(actualData);
			return !areEqual;
		}

		private static async Task<byte[]> ConvertJsonDocumentToData(JsonDocument document)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new Utf8JsonWriter(stream))
				{
					document.RootElement.WriteTo(writer);
					//document.RootElement.WriteValue(writer); //.Net Core 3.0
					await writer.FlushAsync();
				}
				
				stream.Seek(0, SeekOrigin.Begin);
				return stream.ToArray();
			}
		}
		
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
			var jsonString = JsonSettingsManager.SerializeAsync(settings).Result;

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
		private static Task<string> SerializeAsync(object settings)
		{
			// Serialize the given json type back to a string.
			return Task.FromResult(JsonSerializer.Serialize(settings, JsonOptions.Instance));
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