#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Settings;

/// <summary>
/// <para> If used on <b>classes</b>: Defines a custom name for an <see cref="ISettings"/> class. </para>
/// <para> If used on <b>properties</b>: Defines a custom name for the property (that could be displayed by a property grid). </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class SettingsNameAttribute : Attribute
{
	/// <summary>
	/// The custom name for the settings class.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="name"> <inheritdoc cref="Name"/> </param>
	public SettingsNameAttribute(string name)
	{
		this.Name = name;
	}
}