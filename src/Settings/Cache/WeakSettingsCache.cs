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
	/// Implementation of an <see cref="ISettingsCache"/> using weak references.
	/// </summary>
	public class WeakSettingsCache : ISettingsCache
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private readonly ConcurrentDictionary<Type, WeakReference<ISettings>> _cache;

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
			_cache = new ConcurrentDictionary<Type, WeakReference<ISettings>>();
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
						reference.TryGetTarget(out var settings);
						return settings;
					}
				)
				.Where(settings => settings != null)
				.ToArray()
				;
		}

		/// <inheritdoc />
		public bool TryGet<TSettings>(out TSettings settings) where TSettings : ISettings
		{
			ISettings cachedSettings = default;

			var key = WeakSettingsCache.GetKey<TSettings>();
			var wasLoadedFromCache = _cache.TryGetValue(key, out var cachedReference) && cachedReference.TryGetTarget(out cachedSettings);
			settings = (TSettings) cachedSettings;
			return wasLoadedFromCache;
		}

		/// <inheritdoc />
		public void AddOrUpdate<TSettings>(TSettings settings) where TSettings : ISettings
		{
			var key = WeakSettingsCache.GetKey<TSettings>();
			_cache.AddOrUpdate(key, new WeakReference<ISettings>(settings), (_, __) => new WeakReference<ISettings>(settings));
		}

		/// <inheritdoc />
		public bool TryGetOrAdd<TSettings>(out TSettings settings, Func<TSettings> factory) where TSettings : ISettings
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
				_cache.TryAdd(key, new WeakReference<ISettings>(settings));
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
			//return type.AssemblyQualifiedName ?? type.FullName ?? $"{type.Namespace}.{type.Name}";
		}

		#endregion
	}
}