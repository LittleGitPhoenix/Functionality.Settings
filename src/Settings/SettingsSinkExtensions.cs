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
	/// Gets the name for a settings class.
	/// </summary>
	/// <typeparam name="TSettings"> The generic type of the settings class. </typeparam>
	/// <param name="sink"> The <see cref="ISettingsSink"/> that is extended. </param>
	/// <returns> The name of the settings class. </returns>
	public static string GetSettingsFileNameWithoutExtension<TSettings>(this ISettingsSink sink) where TSettings : ISettings
		=> sink.GetSettingsFileNameWithoutExtension(typeof(TSettings));

	/// <summary>
	/// Gets the name for a settings class.
	/// </summary>
	/// <param name="sink"> The <see cref="ISettingsSink"/> that is extended. </param>
	/// <param name="settingsType"> The type of the settings class. </param>
	/// <returns> The name of the settings class. </returns>
	public static string GetSettingsFileNameWithoutExtension(this ISettingsSink sink, Type settingsType)
	{
		if (!settingsType.IsClass) throw new ArgumentException($"The passed type '{settingsType}' must be a class.");
		if (!typeof(ISettings).IsAssignableFrom(settingsType)) throw new ArgumentException($"The passed type '{settingsType}' must implement the interface '{nameof(ISettings)}'.");
		if (settingsType.GetConstructor(Type.EmptyTypes) == null) throw new ArgumentException($"The passed type '{settingsType}' must provide a parameterless constructor.");

		// First check for the SettingsFileNameAttribute.
		var settingsFileNameAttribute = settingsType.GetCustomAttribute<SettingsNameAttribute>();
		return settingsFileNameAttribute?.Name ?? $"{settingsType.Namespace}.{settingsType.Name}";
	}
}