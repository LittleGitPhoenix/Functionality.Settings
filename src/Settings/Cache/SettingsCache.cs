#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Phoenix.Functionality.Settings.Cache
{
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

		private readonly ConcurrentDictionary<Type, ISettings> _cache;

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
			_cache = new ConcurrentDictionary<Type, ISettings>();
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public ICollection<ISettings> GetAllCachedSettings()
		{
			return _cache.Values.ToArray();
		}

		/// <inheritdoc />
		public bool TryGet<TSettings>(out TSettings settings) where TSettings : ISettings
		{
			var key = SettingsCache.GetKey<TSettings>();
			var wasLoadedFromCache = _cache.TryGetValue(key, out var cachedSettings);
			settings = (TSettings) cachedSettings;
			return wasLoadedFromCache;
		}

		/// <inheritdoc />
		public void AddOrUpdate<TSettings>(TSettings settings) where TSettings : ISettings
		{
			var key = SettingsCache.GetKey<TSettings>();
			_cache.AddOrUpdate(key, settings, (_, __) => settings);
		}

		/// <inheritdoc />
		public bool TryGetOrAdd<TSettings>(out TSettings settings, Func<TSettings> factory) where TSettings : ISettings
		{
			var key = SettingsCache.GetKey<TSettings>();

			var wasLoadedFromCache = _cache.TryGetValue(key, out var cachedSettings);
			if (!wasLoadedFromCache)
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
}