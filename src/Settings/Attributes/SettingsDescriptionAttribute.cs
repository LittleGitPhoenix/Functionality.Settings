#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.Settings;

/// <summary>
/// The description for the <see cref="ISettings"/> class or property.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class SettingsDescriptionAttribute : Attribute
{
	/// <summary>
	/// The description for the settings class or property.
	/// </summary>
	public string Description { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="description"> The description for the settings class or property. </param>
	public SettingsDescriptionAttribute(string description)
	{
		this.Description = description;
	}
}