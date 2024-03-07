#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Settings;

/// <summary>
/// Should a property of an <see cref="ISettings"/> class be hidden.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class SettingsHideAttribute : Attribute;