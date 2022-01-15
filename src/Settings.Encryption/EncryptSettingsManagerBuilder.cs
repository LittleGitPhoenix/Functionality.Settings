#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.Settings.Encryption;

/// <summary>
/// Contains extension methods for the builder pattern of an <see cref="ISettingsManager"/>.
/// </summary>
public static class EncryptSettingsManagerBuilder
{
	/// <summary>
	/// Wraps the <see cref="ISettingsManager"/> into a <see cref="EncryptSettingsManager"/> after it has been build.
	/// </summary>
	/// <param name="settingsManagerCreator"> The extended <see cref="ISettingsManagerCreator"/> </param>
	/// <param name="key"> Optional secret key to use for the symmetric algorithm. </param>
	/// <param name="vector"> Optional initialization vector to use for the symmetric algorithm. </param>
	/// <param name="writeCallback"> Optional callback for the internal output. </param>
	/// <returns> An <see cref="ISettingsManagerCreator"/> for chaining. </returns>
	public static ISettingsManagerCreator UsingEncryption(this ISettingsManagerCreator settingsManagerCreator, byte[]? key = null, byte[]? vector = null, Action<string>? writeCallback = null)
	{
		return settingsManagerCreator.AddWrapper(manager => new EncryptSettingsManager(manager, key, vector, writeCallback));
	}
}