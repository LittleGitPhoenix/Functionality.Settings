#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Reflection;

namespace Phoenix.Functionality.Settings;

/// <summary>
/// Extension methods for <see cref="ISettingsManager"/>.
/// </summary>
public static class SettingsManagerExtensions
{
	/// <summary>
	/// Gets the name for a settings class.
	/// </summary>
	/// <typeparam name="TSettings"> The generic type of the settings class. </typeparam>
	/// <param name="settingsManager"> The <see cref="ISettingsManager"/> that is extended. </param>
	/// <returns> The name of the settings class. </returns>
	public static string GetSettingsFileNameWithoutExtension<TSettings>(this ISettingsManager settingsManager) where TSettings : ISettings
		=> settingsManager.GetSettingsFileNameWithoutExtension(typeof(TSettings));

	/// <summary>
	/// Gets the name for a settings class.
	/// </summary>
	/// <param name="settingsManager"> The <see cref="ISettingsManager"/> that is extended. </param>
	/// <param name="settingsType"> The type of the settings class. </param>
	/// <returns> The name of the settings class. </returns>
	public static string GetSettingsFileNameWithoutExtension(this ISettingsManager settingsManager, Type settingsType)
	{
		if (!settingsType.IsClass) throw new ArgumentException($"The passed type '{settingsType}' must be a class.");
		if (!typeof(ISettings).IsAssignableFrom(settingsType)) throw new ArgumentException($"The passed type '{settingsType}' must implement the interface '{nameof(ISettings)}'.");
		if (settingsType.GetConstructor(Type.EmptyTypes) == null) throw new ArgumentException($"The passed type '{settingsType}' must provide a parameterless constructor.");

		// First check for the SettingsFileNameAttribute.
		var settingsFileNameAttribute = settingsType.GetCustomAttribute<SettingsNameAttribute>();
		return settingsFileNameAttribute?.Name ?? $"{settingsType.Namespace}.{settingsType.Name}";
	}
}