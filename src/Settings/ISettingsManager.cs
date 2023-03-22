#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Phoenix.Functionality.Settings.Cache;

namespace Phoenix.Functionality.Settings;

/// <summary>
/// Manager for <see cref="ISettings"/>
/// </summary>
public interface ISettingsManager
{
	#region Delegates / Events
	#endregion

	#region Properties

	//! This is currently not implemented, as it seems not to be the responsibility of the manager to expose all the loaded settings instances.
	///// <summary>
	///// A cache for loaded <see cref="ISettings"/> instance.
	///// </summary>
	///// <remarks> Could be used to get all currently available instances and make them editable via some kind of UI. </remarks>
	//ISettingsCache Cache { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Loads the settings of type <typeparamref name="TSettings"/>.
	/// </summary>
	/// <typeparam name="TSettings"> The concrete settings type. </typeparam>
	/// <param name="bypassCache"> Prevents the <see cref="ISettingsManager"/> from using its internal <see cref="ISettingsCache"/> when loading settings. Default is false. </param>
	/// <param name="preventCreation"> Prevents the <see cref="ISettingsManager"/> from creating and saving a default instance of <typeparamref name="TSettings"/> if no underlying settings data is available. Default is false. </param>
	/// <param name="preventUpdate"> Prevents the <see cref="ISettingsManager"/> from updating the underlying data source in case the settings instance differs from it. Default is false. </param>
	/// <returns> A new instance of <typeparamref name="TSettings"/>. </returns>
	/// <exception cref="SettingsLoadException"> May be thrown if the settings data could not be loaded. </exception>
	/// <exception cref="SettingsUnavailableException"> May be thrown if the settings data could not be loaded and <see cref="preventCreation"/> is <b>true</b>. </exception>
	TSettings Load<TSettings>(bool bypassCache = false, bool preventCreation = false, bool preventUpdate = false) where TSettings : class, ISettings, new();

	/// <summary>
	/// Saves the settings.
	/// </summary>
	/// <param name="settings"> The settings instance to save. </param>
	/// <param name="createBackup"> Should a backup of the settings be made before the settings are saved. </param>
	/// <typeparam name="TSettings"> The concrete settings type. </typeparam>
	/// <exception cref="SettingsSaveException"> May be thrown if the settings data could not be saved. </exception>
	void Save<TSettings>(TSettings settings, bool createBackup = default) where TSettings : ISettings;

	/// <summary>
	/// Deletes the settings.
	/// </summary>
	/// <param name="createBackup"> Should a backup of the settings be made before the get deleted. </param>
	/// <typeparam name="TSettings"> The concrete settings type. </typeparam>
	/// <exception cref="SettingsDeleteException"> May be thrown if the settings data could not be saved. </exception>
	void Delete<TSettings>(bool createBackup = default) where TSettings : ISettings;

	#endregion
}