#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.Settings.Encryption;

/// <summary>
/// Attribute to force checking the properties of the attributed property. This can be used to force following a nested property recursively and overruling the automatic mechanism.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class EncryptForceFollowAttribute : Attribute { }