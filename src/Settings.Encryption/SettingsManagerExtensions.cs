#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Functionality.Settings.Encryption
{
	/// <summary>
	/// Contains extension methods for <see cref="ISettingsManager"/> instances.
	/// </summary>
	public static class SettingsManagerExtensions
	{
		/// <summary>
		/// Each property annotated with the <see cref="EncryptAttribute"/> from will automatically be de- or encrypted.
		/// </summary>
		/// <param name="settingsManager"> The extended <see cref="ISettingsManager"/>. </param>
		/// <returns> An <see cref="ISettingsManager"/> for chaining. </returns>
		public static ISettingsManager ApplyEncryption(this ISettingsManager settingsManager)
		{
			return new EncryptSettingsManager(settingsManager);
		}
	}
}