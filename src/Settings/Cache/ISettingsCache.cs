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
	/// <returns> <c>True</c> if the settings where retrieved from cache, otherwise <c>False</c>. </returns>
	bool TryGet<TSettings>(out TSettings? settings) where TSettings : class, ISettings;
		
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
	/// <returns> <c>True</c> if the settings where retrieved from cache, otherwise <c>False</c>. </returns>
	bool TryGetOrAdd<TSettings>(out TSettings settings, Func<TSettings> factory) where TSettings : class, ISettings;
}