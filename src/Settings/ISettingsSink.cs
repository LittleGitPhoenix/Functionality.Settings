﻿#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Settings;

/// <summary>
/// Base interface for all settings sinks.
/// </summary>
/// <remarks> The purpose of this interface is to use it to create extension methods. </remarks>
public interface ISettingsSink { }

/// <summary>
/// Interface for handlers that are responsible for loading and saving <see cref="ISettings"/> to persistent storage.
/// </summary>
/// <typeparam name="TSettingsData"> The type of the data the settings is saved as. </typeparam>
public interface ISettingsSink<TSettingsData> : ISettingsSink
	where TSettingsData : class?
{
	/// <summary>
	/// Retrieves the settings data from the persistent storage.
	/// </summary>
	/// <typeparam name="TSettings"> The type of the settings to retrieve. </typeparam>
	/// <returns> The settings data or null. </returns>
	/// <exception cref="SettingsLoadException"> May be thrown if the settings data could not be retrieved. </exception>
	TSettingsData? Retrieve<TSettings>() where TSettings : ISettings;

	/// <summary>
	/// Stores <paramref name="settingsData"/> in the persistent storage.
	/// </summary>
	/// <typeparam name="TSettings"> The type of the settings to store. </typeparam>
	/// <param name="settingsData"> The settings data to store. </param>
	/// <param name="createBackup"> Optional flag if a backup of already existing data should be created. Default is false. </param>
	/// <exception cref="SettingsSaveException"> May be thrown if the settings data could not be stored. </exception>
	void Store<TSettings>(TSettingsData settingsData, bool createBackup = default) where TSettings : ISettings;

	/// <summary>
	/// Purges the settings data for <typeparamref name="TSettings"/> from the persistent storage.
	/// </summary>
	/// <typeparam name="TSettings"> The type of the settings to purge. </typeparam>
	/// <param name="createBackup"> Optional flag if a backup should be created. Default is false. </param>
	void Purge <TSettings>(bool createBackup = default) where TSettings : ISettings;
}