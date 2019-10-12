#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Functionality.Settings
{
	/// <summary>
	/// Defines a property of a <see cref="ISettings"/> class as nested.
	/// </summary>
	[Obsolete("Attributing complex types as 'Nested' is not needed anymore. All implementations are able to handle such types without this attribute.", true)]
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class NestedSettingsAttribute : Attribute { }
}