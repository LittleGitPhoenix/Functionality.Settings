#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Settings;

/// <summary>
/// Base exception class for the <see cref="Phoenix.Functionality.Settings"/> library.
/// </summary>
public class SettingsException : Exception
{
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="message"> <see cref="Exception.Message"/> </param>
	/// <param name="innerException"> <see cref="Exception.InnerException"/> </param>
	public SettingsException(string? message = null, Exception? innerException = null) : base(message, innerException) { }
}

/// <summary>
/// Base exception used when loading settings fails (e.g. <see cref="ISettingsManager.Load{TSettings}"/>).
/// </summary>
public class SettingsLoadException : SettingsException
{
	/// <inheritdoc />
	public SettingsLoadException(string? message = null, Exception? innerException = null) : base(message, innerException) { }
}

/// <summary>
/// Base exception used when saving settings fails (e.g. <see cref="ISettingsManager.Save{TSettings}"/>).
/// </summary>
public class SettingsSaveException : SettingsException
{
	/// <inheritdoc />
	public SettingsSaveException(string message, Exception? innerException = null) : base(message, innerException) { }
}

/// <summary>
/// Base exception used when deleting settings fails (e.g. <see cref="ISettingsManager.Delete{TSettings}"/>).
/// </summary>
public class SettingsDeleteException : SettingsException
{
	/// <inheritdoc />
	public SettingsDeleteException(string message, Exception? innerException = null) : base(message, innerException) { }
}