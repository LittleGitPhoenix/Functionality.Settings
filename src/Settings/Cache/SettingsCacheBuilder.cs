#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.Settings.Cache;

/// <summary>
/// Contains extension methods for the builder pattern of an <see cref="ISettingsManager"/>.
/// </summary>
public static class SettingsCacheBuilder
{
	/// <summary>
	/// Adds the <see cref="SettingsCache"/> as <see cref="ISettingsCache"/> to the builder.
	/// </summary>
	/// <param name="cacheBuilder"> The extended <see cref="ISettingsCacheBuilder"/>. </param>
	/// <returns> An <see cref="ISettingsManagerCreator"/> for chaining. </returns>
	public static ISettingsManagerCreator UsingCache(this ISettingsCacheBuilder cacheBuilder)
	{
		return cacheBuilder.AddCache(new SettingsCache());
	}

	/// <summary>
	/// Adds the <see cref="WeakSettingsCache"/> as <see cref="ISettingsCache"/> to the builder.
	/// </summary>
	/// <param name="cacheBuilder"> The extended <see cref="ISettingsCacheBuilder"/>. </param>
	/// <returns> An <see cref="ISettingsManagerCreator"/> for chaining. </returns>
	public static ISettingsManagerCreator UsingWeakCache(this ISettingsCacheBuilder cacheBuilder)
	{
		return cacheBuilder.AddCache(new WeakSettingsCache());
	}
}