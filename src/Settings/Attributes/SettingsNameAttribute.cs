#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.Settings;

/// <summary>
/// Defines a custom name for a <see cref="ISettings"/> class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class SettingsNameAttribute : Attribute
{
	/// <summary>
	/// The custom name for the settings class.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="name"> The custom name for the settings class. </param>
	public SettingsNameAttribute(string name)
	{
		this.Name = name;
	}
}