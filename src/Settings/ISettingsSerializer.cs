#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Dynamic;

namespace Phoenix.Functionality.Settings;

/// <summary>
/// Base interface for all settings serializers.
/// </summary>
/// <remarks> The purpose of this interface is to use it to create extension methods. </remarks>
public interface ISettingsSerializer { }

/// <summary>
/// Interface for handlers that (de)serializes <see cref="ISettings"/> instances.
/// </summary>
/// <typeparam name="TSettingsData"> The type of the data the settings was saved in. </typeparam>
public interface ISettingsSerializer<TSettingsData> : ISettingsSerializer
	where TSettingsData : class
{
	/// <summary>
	/// Deserializes <paramref name="settingsData"/> into an <typeparamref name="TSettings"/> instance.
	/// </summary>
	/// <typeparam name="TSettings"> The concrete type of the <see cref="ISettings"/> instance. </typeparam>
	/// <param name="settingsData"> The data to deserialize. </param>
	/// <param name="rawData"> This should be the parsed raw data of <typeparamref name="TSettingsData"/>. It should only be filled if <typeparamref name="TSettings"/> implements <see cref="ISettingsLayoutChangedNotification"/>. </param>
	/// <returns> A new settings instance. </returns>
	/// <exception cref="SettingsLoadException"> Thrown if <paramref name="settingsData"/> could not be deserialized. </exception>
	TSettings? Deserialize<TSettings>(TSettingsData settingsData, out ExpandoObject? rawData) where TSettings : class, ISettings;

	/// <summary>
	/// Serializes <paramref name="settings"/> into <typeparamref name="TSettingsData"/>.
	/// </summary>
	/// <param name="settings"> The settings to serialize. </param>
	/// <returns></returns>
	/// <exception cref="SettingsSaveException"> Thrown if <paramref name="settings"/> could not be serialized. </exception>
	TSettingsData Serialize(ISettings settings);

	/// <summary>
	/// Checks if <paramref name="settings"/> and <paramref name="settingsData"/> are identical.
	/// </summary>
	/// <param name="settings"> The <see cref="ISettings"/> instance to compare. </param>
	/// <param name="settingsData"> The <typeparamref name="TSettingsData"/> to compare. </param>
	/// <returns></returns>
	/// <remarks> This is used to check if the data needs to be updated because the structure of the settings changed. </remarks>
	/// <exception cref="SettingsException"> Thrown if <paramref name="settings"/> could not be compared to <paramref name="settingsData"/>. </exception>
	bool AreIdentical(ISettings settings, TSettingsData settingsData);
}