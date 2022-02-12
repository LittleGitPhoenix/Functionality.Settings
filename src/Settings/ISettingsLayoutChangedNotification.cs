#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Dynamic;

namespace Phoenix.Functionality.Settings;

/// <summary>
/// Notification interface for <see cref="ISettings"/>.
/// </summary>
public interface ISettingsLayoutChangedNotification
{
	/// <summary>
	/// Invoked if settings have been loaded but they differ from the underlying data.
	/// </summary>
	/// <param name="rawData"> The original serialized settings data. </param>
	void LayoutChanged(ExpandoObject rawData);
}