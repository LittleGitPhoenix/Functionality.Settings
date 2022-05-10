#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Settings.Cache;

/// <summary>
/// Cache for different <see cref="ISettings"/> instances.
/// </summary>
public interface ISettingsCache
{
	/// <summary>
	/// Gets all currently cached settings.
	/// </summary>
	/// <returns> A collection of all cached <see cref="ISettings"/> instances. </returns>
	ICollection<ISettings> GetAllCachedSettings();

	/// <summary>
	/// Tries to retrieve the settings of type <typeparamref name="TSettings"/>.
	/// </summary>
	/// <typeparam name="TSettings"> The concrete settings type. </typeparam>
	/// <param name="settings"> The settings instance. </param>
	/// <returns> <b>True</b> if the settings where retrieved from cache, otherwise <b>False</b>. </returns>
#if NETFRAMEWORK || NETSTANDARD2_0
	bool TryGet<TSettings>(out TSettings? settings) where TSettings : class, ISettings;
#else
	bool TryGet<TSettings>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TSettings? settings) where TSettings : class, ISettings;
#endif

	/// <summary>
	/// Adds or updates the settings of type <typeparamref name="TSettings"/> to the cache.
	/// </summary>
	/// <typeparam name="TSettings"> The concrete settings type. </typeparam>
	/// <param name="settings"> The settings instance. </param>
	void AddOrUpdate<TSettings>(TSettings settings) where TSettings : class, ISettings;

	/// <summary>
	/// Tries to add or retrieve the settings of type <typeparamref name="TSettings"/>.
	/// </summary>
	/// <typeparam name="TSettings"> The concrete settings type. </typeparam>
	/// <param name="settings"> The settings instance. </param>
	/// <param name="factory"> The factory method that will create a new settings instance. </param>
	/// <returns> <b>True</b> if the settings where retrieved from cache, otherwise <b>False</b>. </returns>
	bool TryGetOrAdd<TSettings>(out TSettings settings, Func<TSettings> factory) where TSettings : class, ISettings;

	/// <summary>
	/// Tries to remove the settings of type <typeparamref name="TSettings"/>.
	/// </summary>
	/// <typeparam name="TSettings"> The concrete settings type. </typeparam>
	/// <param name="settings"> The removed settings instance. </param>
	/// <returns> <b>True</b> if the settings where removed from cache, otherwise <b>False</b>. </returns>
#if NETFRAMEWORK || NETSTANDARD2_0
	bool TryRemove<TSettings>(out TSettings? settings) where TSettings : ISettings;
#else
	bool TryRemove<TSettings>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TSettings? settings) where TSettings : ISettings;
#endif
}