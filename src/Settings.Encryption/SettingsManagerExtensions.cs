#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.Settings.Encryption;

/// <summary>
/// Contains extension methods for <see cref="ISettingsManager"/> instances.
/// </summary>
public static class SettingsManagerExtensions
{
	/// <summary>
	/// Each property annotated with the <see cref="EncryptAttribute"/> from will automatically be de- or encrypted.
	/// </summary>
	/// <param name="settingsManager"> The extended <see cref="ISettingsManager"/>. </param>
	/// <param name="writeCallback"> Optional callback for the internal output of the <see cref="EncryptSettingsManager"/>. Default is null. </param>
	/// <returns> An <see cref="ISettingsManager"/> for chaining. </returns>
	public static ISettingsManager ApplyEncryption(this ISettingsManager settingsManager, Action<string>? writeCallback = null)
	{
		return new EncryptSettingsManager(settingsManager, writeCallback: writeCallback);
	}

	/// <summary>
	/// Each property annotated with the <see cref="EncryptAttribute"/> from will automatically be de- or encrypted.
	/// </summary>
	/// <param name="settingsManager"> The extended <see cref="ISettingsManager"/>. </param>
	/// <param name="key"> The secret key to use for the symmetric algorithm. </param>
	/// <param name="vector"> The initialization vector to use for the symmetric algorithm. </param>
	/// <param name="writeCallback"> Optional callback for the internal output of the <see cref="EncryptSettingsManager"/>. Default is null. </param>
	/// <returns> An <see cref="ISettingsManager"/> for chaining. </returns>
	public static ISettingsManager ApplyEncryption(this ISettingsManager settingsManager, byte[] key, byte[] vector, Action<string>? writeCallback = null)
	{
		return new EncryptSettingsManager(settingsManager, key, vector, writeCallback);
	}
}