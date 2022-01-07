#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Collections.Concurrent;

namespace Phoenix.Functionality.Settings.Cache;

/// <summary>
/// Implementation of an <see cref="ISettingsCache"/> using weak references.
/// </summary>
public class WeakSettingsCache : ISettingsCache
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	private readonly ConcurrentDictionary<Type, WeakReference<object>> _cache;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	public WeakSettingsCache()
	{
		// Save parameters.

		// Initialize fields.
		_cache = new ConcurrentDictionary<Type, WeakReference<object>>();
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public ICollection<ISettings> GetAllCachedSettings()
	{
		return _cache
			.Values
			.Select
			(
				reference =>
				{
					reference.TryGetTarget(out var cachedSettings);
					return cachedSettings as ISettings;
				}
			)
			.Where(settings => settings != null)
			// ReSharper disable once SuspiciousTypeConversion.Global → The above filter removes all null elements and it is therefor save to cast the collection.
			.Cast<ISettings>()
			.ToArray()
			;
	}

	/// <inheritdoc />
	public bool TryGet<TSettings>(out TSettings? settings) where TSettings : class, ISettings
	{
		var key = WeakSettingsCache.GetKey<TSettings>();
		object? cachedSettings = default;
		var wasLoadedFromCache = _cache.TryGetValue(key, out var cachedReference) && cachedReference.TryGetTarget(out cachedSettings);
		settings = cachedSettings is not null ? (TSettings) cachedSettings : null;
		return wasLoadedFromCache;
	}

	/// <inheritdoc />
	public void AddOrUpdate<TSettings>(TSettings settings) where TSettings : class, ISettings
	{
		var key = WeakSettingsCache.GetKey<TSettings>();
		_cache.AddOrUpdate(key, new WeakReference<object>(settings), (_, _) => new WeakReference<object>(settings));
	}

	/// <inheritdoc />
	public bool TryGetOrAdd<TSettings>(out TSettings settings, Func<TSettings> factory) where TSettings : class, ISettings
	{
		var key = WeakSettingsCache.GetKey<TSettings>();

		if (_cache.TryGetValue(key, out var cachedReference))
		{
			if (cachedReference.TryGetTarget(out var cachedSettings))
			{
				// Get the instance from the cache.
				settings = (TSettings) cachedSettings;
				return true;
			}
			else
			{
				// The settings has been garbage collected, so refresh it with a new instance.
				settings = factory.Invoke();
				cachedReference.SetTarget(settings);
				return false;
			}
		}
		else
		{
			// Nothing has been cached, create a new instance.
			settings = factory.Invoke();
			_cache.TryAdd(key, new WeakReference<object>(settings));
			return false;
		}
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