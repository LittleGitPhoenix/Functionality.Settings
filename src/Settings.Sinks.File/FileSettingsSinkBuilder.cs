#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.Settings.Sinks.File;

/// <summary>
/// Contains extension methods for the builder pattern of an <see cref="ISettingsManager"/>.
/// </summary>
public static class FileSettingsSinkBuilder
{
	/// <summary>
	/// Adds the <see cref="FileSettingsSink"/> as <see cref="ISettingsSink"/> to the builder.
	/// </summary>
	/// <param name="sinkBuilder"> The extended <see cref="ISettingsSinkBuilder{TSettingsData}"/> </param>
	/// <param name="fileExtension"> Optional file extension of settings files. Default is <see cref="FileSettingsSink.DefaultSettingsFileExtension"/>. </param>
	/// <param name="baseDirectory"> Optional directory where all setting files reside. Default will be a folder named <see cref="FileSettingsSink.DefaultSettingsFolderName"/> in <see cref="Directory.GetCurrentDirectory"/>. </param>
	/// <returns> An <see cref="ISettingsSerializerBuilder{TSettingsData}"/> for chaining. </returns>
	public static ISettingsSerializerBuilder<string> UsingFileSink(this ISettingsSinkBuilder<string> sinkBuilder, string? fileExtension = null, DirectoryInfo? baseDirectory = null)
	{
		return sinkBuilder.AddSink(new FileSettingsSink(fileExtension, baseDirectory));
	}
}