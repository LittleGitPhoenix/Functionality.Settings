#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Phoenix.Functionality.Settings.Cache;

namespace Phoenix.Functionality.Settings;

/// <summary>
/// Common implementation of an <see cref="ISettingsManager"/> using an <see cref="ISettingsSink"/> for persistent storage and an <see cref="ISettingsSerializer"/> for (de)serializing <see cref="ISettings"/> instances.
/// </summary>
/// <typeparam name="TSettingsData"> The type of the data that is used to store the settings outside of the application life cycle. </typeparam>
public class SettingsManager<TSettingsData> : ISettingsManager
	where TSettingsData : class
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	private readonly object _lock;

	private readonly ISettingsSink<TSettingsData> _sink;

	private readonly ISettingsSerializer<TSettingsData> _serializer;

	private readonly ISettingsCache _cache;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="sink"> The <see cref="ISettingsSink{TSettingsData}"/> </param>
	/// <param name="serializer"> The <see cref="ISettingsSerializer{TSettingsData}"/> </param>
	/// <param name="cache"> Optional <see cref="ISettingsCache"/> </param>
	public SettingsManager
	(
		ISettingsSink<TSettingsData> sink,
		ISettingsSerializer<TSettingsData> serializer,
		ISettingsCache? cache = null
	)
	{
		// Save parameters.
		_sink = sink;
		_serializer = serializer;
		_cache = cache ?? new NoSettingsCache();

		// Initialize fields.
		_lock = new object();
	}

	/// <summary>
	/// Starts creating a <see cref="ISettingsManager"/> via builder pattern.
	/// </summary>
	public static ISettingsSinkBuilder<TSettingsData> Create() => new SettingsManagerBuilder<TSettingsData>();

	#endregion

	#region Methods

	/// <inheritdoc />
	public TSettings Load<TSettings>(bool bypassCache = false, bool preventUpdate = false, bool throwIfNoDataIsAvailable = false)
		where TSettings : class, ISettings, new()
	{
		lock (_lock)
		{
			// Check the cache for an existing instance and imitatively return it.
#if NETFRAMEWORK || NETSTANDARD2_0
			if (!bypassCache && _cache.TryGet<TSettings>(out var settings) && settings is not null) return settings;
#else
			if (!bypassCache && _cache.TryGet<TSettings>(out var settings)) return settings;
#endif

			// Get the data.
			var settingsData = _sink.Retrieve<TSettings>(throwIfNoDataIsAvailable);
			//? Should the sink automatically create a default instance if nothing is available and NO error occurred?
			//* Could be better and seems to follow separation of concerns.
			if (settingsData is null)
			{
				// Create a new settings instance.
				settings = this.GetAndSaveDefaultInstance<TSettings>(preventUpdate);
			}
			else
			{
				// Convert the data into a settings instance.
				settings = _serializer.Deserialize<TSettings>(settingsData, out var rawData);

				//? Should a default instance be created, if the existing data could not be serialized?
				//* It seems reasonable, to throw the exception instead of using default values.
				// If the data could not be serialized, then create a default instance.
				if (settings is null)
				{
					settings = this.GetAndSaveDefaultInstance<TSettings>(preventUpdate);
				}
				//else if (!preventUpdate && !_serializer.AreIdentical(settings, settingsData))
				else if (!preventUpdate)
				{
					// Check if the settings instance differs from the data, so that the data can be updated.
					var areIdentical = false;
					try
					{
						areIdentical = _serializer.AreIdentical(settings, settingsData);
					}
					catch (SettingsException ex)
					{
						throw new SettingsLoadException(ex.Message, ex.InnerException);
					}

					if (!areIdentical)
					{
						// ReSharper disable once SuspiciousTypeConversion.Global → This is okay. 'ISettingsLayoutChangedNotification' is supposed to be added together with the 'ISettings' interface.
						if (rawData is not null && settings is ISettingsLayoutChangedNotification layoutChangedSettings)
						{
							layoutChangedSettings.LayoutChanged(rawData);
						}
						this.Save(settings, createBackup: true);
					}
				}
			}

			// Setup usage of the special extension methods.
			settings.InitializeExtensionMethods(this);

			// Update the cache if needed.
			if (!bypassCache) _cache.AddOrUpdate(settings);

			// Return the instance.
			return settings;
		}
	}

	/// <inheritdoc />
	public virtual void Save<TSettings>(TSettings settings, bool createBackup = default)
		where TSettings : ISettings
	{
		lock (_lock)
		{
			// Convert the settings instance back to data.
			var settingsData = _serializer.Serialize(settings);

			// Save the data.
			_sink.Store<TSettings>(settingsData);
		}
	}

	#region Helper

	/// <summary>
	/// Creates a default settings instance and saves it to the persistency storage.
	/// </summary>
	/// <typeparam name="TSettings"> The settings type. </typeparam>
	/// <param name="saveSettings"> Flag if the created instance should be saved or not. </param>
	/// <returns> A new instance of <typeparamref name="TSettings"/>. </returns>
	internal virtual TSettings GetAndSaveDefaultInstance<TSettings>(bool saveSettings) where TSettings : class, ISettings, new()
	{
		// Create a new settings instance.
		var settings = GetDefaultInstance<TSettings>();

		// Save the instance if necessary.
		if (!saveSettings) this.Save(settings);

		// Return the instance.
		return settings;
	}

	/// <summary>
	/// Creates a default settings instance.
	/// </summary>
	/// <typeparam name="TSettings"> The settings type. </typeparam>
	/// <returns> A new instance of <typeparamref name="TSettings"/>. </returns>
	private static TSettings GetDefaultInstance<TSettings>() where TSettings : class, ISettings, new() => new();

	#endregion

	#endregion
}