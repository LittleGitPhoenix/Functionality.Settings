#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Settings;

/// <summary>
/// Notification interface for <see cref="ISettings"/>.
/// </summary>
public interface ISettingsLoadedNotification
{
	/// <summary>
	/// Invoked if the settings instance has been loaded for the first time.
	/// </summary>
	/// <remarks> Only gets invoked if a new instance of the settings is created by an <see cref="ISettingsManager"/>. Therefore, does not get triggered again, if a <see cref="Cache.ISettingsCache"/> is hit when loading settings. </remarks>
	void Loaded();
}