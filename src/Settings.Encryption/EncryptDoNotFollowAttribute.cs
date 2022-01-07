#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.Settings.Encryption;

/// <summary>
/// Attribute to not check the properties of the attributed property. This can be used to stop the automatic mechanism from following a nested property recursively.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class EncryptDoNotFollowAttribute : Attribute { }