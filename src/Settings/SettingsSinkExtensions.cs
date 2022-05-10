#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Reflection;

namespace Phoenix.Functionality.Settings;

/// <summary>
/// Extension methods for <see cref="ISettingsSink"/>.
/// </summary>
public static class SettingsSinkExtensions
{
	/// <summary>
	/// Gets the name of a settings class respecting the <see cref="SettingsNameAttribute"/>.
	/// </summary>
	/// <typeparam name="TSettings"> The generic type of the settings class. </typeparam>
	/// <param name="sink"> The <see cref="ISettingsSink"/> that is extended. </param>
	/// <returns> The name of the settings class. </returns>
	public static string GetSettingsName<TSettings>(this ISettingsSink sink) where TSettings : ISettings
		=> sink.GetSettingsName(typeof(TSettings));

	/// <summary>
	/// Gets the name of a settings class respecting the <see cref="SettingsNameAttribute"/>.
	/// </summary>
	/// <param name="sink"> The <see cref="ISettingsSink"/> that is extended. </param>
	/// <param name="settingsType"> The type of the settings class. </param>
	/// <returns> The name of the settings class. </returns>
	public static string GetSettingsName(this ISettingsSink sink, Type settingsType)
	{
		if (!typeof(ISettings).IsAssignableFrom(settingsType)) throw new ArgumentException($"The passed type '{settingsType}' must implement the interface '{nameof(ISettings)}'.");

		// First check for the SettingsFileNameAttribute.
		var settingsFileNameAttribute = settingsType.GetCustomAttribute<SettingsNameAttribute>();
		return settingsFileNameAttribute?.Name ?? $"{settingsType.Namespace}.{settingsType.Name}";
	}
}