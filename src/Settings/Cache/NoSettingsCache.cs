﻿#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Settings.Cache;

/// <summary>
/// Implementation of an <see cref="ISettingsCache"/> that doesn't cache.
/// </summary>
public class NoSettingsCache : ISettingsCache
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	public NoSettingsCache() { }

	#endregion

	#region Methods

	/// <inheritdoc />
	public ICollection<ISettings> GetAllCachedSettings()
	{
		return Array.Empty<ISettings>();
	}

	/// <inheritdoc />
#if NETFRAMEWORK || NETSTANDARD2_0
	public bool TryGet<TSettings>(out TSettings? settings) where TSettings : class, ISettings
#else
	public bool TryGet<TSettings>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TSettings? settings) where TSettings : class, ISettings
#endif
	{
		settings = default;
		return false;
	}

	/// <inheritdoc />
	public void AddOrUpdate<TSettings>(TSettings settings) where TSettings : class, ISettings
	{
		/* no cache → no update */
	}

	/// <inheritdoc />
	public bool TryGetOrAdd<TSettings>(out TSettings settings, Func<TSettings> factory) where TSettings : class, ISettings
	{
		settings = factory.Invoke();
		return false;
	}

	/// <inheritdoc />
#if NETFRAMEWORK || NETSTANDARD2_0
	public bool TryRemove<TSettings>(out TSettings? settings) where TSettings : ISettings
#else
	public bool TryRemove<TSettings>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TSettings? settings) where TSettings : ISettings
#endif
	{
		settings = default;
		return false;
	}

	#endregion
}