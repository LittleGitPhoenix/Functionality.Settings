#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Collections.Concurrent;

namespace Phoenix.Functionality.Settings.Cache;

/// <summary>
/// Implementation of an <see cref="ISettingsCache"/> using strong references.
/// </summary>
public class SettingsCache : ISettingsCache
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	private readonly ConcurrentDictionary<Type, object> _cache;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	public SettingsCache()
	{
		// Save parameters.

		// Initialize fields.
		_cache = new ConcurrentDictionary<Type, object>();
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public ICollection<ISettings> GetAllCachedSettings()
	{
		return _cache.Values.OfType<ISettings>().ToArray();
	}

	/// <inheritdoc />
	public bool TryGet<TSettings>(out TSettings? settings) where TSettings : class, ISettings
	{
		var key = SettingsCache.GetKey<TSettings>();
		_cache.TryGetValue(key, out var cachedSettings);
		settings = cachedSettings is not null ? (TSettings) cachedSettings : null;
		return settings is not null;
	}

	/// <inheritdoc />
	public void AddOrUpdate<TSettings>(TSettings settings) where TSettings : class, ISettings
	{
		var key = SettingsCache.GetKey<TSettings>();
		_cache.AddOrUpdate(key, settings, (_, __) => settings);
	}

	/// <inheritdoc />
	public bool TryGetOrAdd<TSettings>(out TSettings settings, Func<TSettings> factory) where TSettings : class, ISettings

	{
		var key = SettingsCache.GetKey<TSettings>();
		var wasLoadedFromCache = _cache.TryGetValue(key, out var cachedSettings);
		if (!wasLoadedFromCache || cachedSettings is null)
		{
			cachedSettings = factory.Invoke();
			_cache.TryAdd(key, cachedSettings);
		}
		settings = (TSettings) cachedSettings;
		return wasLoadedFromCache;
	}

	/// <summary>
	/// Gets the value from <typeparamref name="TSettings"/> used as key in the underlying cache.
	/// </summary>
	private static Type GetKey<TSettings>()
	{
		var type = typeof(TSettings);
		return type;
	}

	#endregion
}