#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;

/// <summary>
/// Contains extension methods for the builder pattern of a <see cref="JsonSettingsSerializer"/>.
/// </summary>
public static class CustomConverterBuilder
{
	/// <summary>
	/// Adds the <see cref="DirectoryInfoConverter"/> as custom <see cref="System.Text.Json.Serialization.JsonConverter"/> to the builder.
	/// </summary>
	/// <param name="builder"> The extended <see cref="IJsonSettingsSerializerOptionsBuilder"/>. </param>
	/// <param name="baseDirectory"> Optional base directory used to determine relative path. </param>
	/// <returns> An <see cref="IJsonSettingsSerializerOptionsBuilder"/> for chaining. </returns>
	public static IJsonSettingsSerializerOptionsBuilder WithDirectoryInfoConverter(this IJsonSettingsSerializerOptionsBuilder builder, DirectoryInfo? baseDirectory = null)
	{
		return builder.AddConverter(new DirectoryInfoConverter(baseDirectory));
	}

	/// <summary>
	/// Adds the <see cref="FileInfoConverter"/> as custom <see cref="System.Text.Json.Serialization.JsonConverter"/> to the builder.
	/// </summary>
	/// <param name="builder"> The extended <see cref="IJsonSettingsSerializerOptionsBuilder"/>. </param>
	/// <param name="baseDirectory"> Optional base directory used to determine relative path. </param>
	/// <returns> An <see cref="IJsonSettingsSerializerOptionsBuilder"/> for chaining. </returns>
	public static IJsonSettingsSerializerOptionsBuilder WithFileInfoConverter(this IJsonSettingsSerializerOptionsBuilder builder, DirectoryInfo? baseDirectory = null)
	{
		return builder.AddConverter(new FileInfoConverter(baseDirectory));
	}

	/// <summary>
	/// Adds the <see cref="IpAddressConverter"/> as custom <see cref="System.Text.Json.Serialization.JsonConverter"/> to the builder.
	/// </summary>
	/// <param name="builder"> The extended <see cref="IJsonSettingsSerializerOptionsBuilder"/>. </param>
	/// <returns> An <see cref="IJsonSettingsSerializerOptionsBuilder"/> for chaining. </returns>
	public static IJsonSettingsSerializerOptionsBuilder WithIpAddressConverter(this IJsonSettingsSerializerOptionsBuilder builder)
	{
		return builder.AddConverter(new IpAddressConverter());
	}

	/// <summary>
	/// Adds the <see cref="RegexConverter"/> as custom <see cref="System.Text.Json.Serialization.JsonConverter"/> to the builder.
	/// </summary>
	/// <param name="builder"> The extended <see cref="IJsonSettingsSerializerOptionsBuilder"/>. </param>
	/// <param name="fallbackPattern"> Optional fallback pattern used if a null-string is parsed. Default is <see cref="RegexConverter.DefaultFallbackPattern"/>. </param>
	/// <param name="regexOptions"> Optional <see cref="System.Text.RegularExpressions.RegexOptions"/> applied to newly created <see cref="System.Text.RegularExpressions.Regex"/> instances. Default is <see cref="RegexConverter.DefaultRegexOptions"/>. </param>
	/// <returns> An <see cref="IJsonSettingsSerializerOptionsBuilder"/> for chaining. </returns>
	public static IJsonSettingsSerializerOptionsBuilder WithRegexConverter(this IJsonSettingsSerializerOptionsBuilder builder, string? fallbackPattern = null, System.Text.RegularExpressions.RegexOptions? regexOptions = null)
	{
		return builder.AddConverter(new RegexConverter(fallbackPattern, regexOptions));
	}

	/// <summary>
	/// Adds the <see cref="TimeSpanConverter"/> as custom <see cref="System.Text.Json.Serialization.JsonConverter"/> to the builder.
	/// </summary>
	/// <param name="builder"> The extended <see cref="IJsonSettingsSerializerOptionsBuilder"/>. </param>
	/// <returns> An <see cref="IJsonSettingsSerializerOptionsBuilder"/> for chaining. </returns>
	public static IJsonSettingsSerializerOptionsBuilder WithTimeSpanConverter(this IJsonSettingsSerializerOptionsBuilder builder)
	{
		return builder.AddConverter(new TimeSpanConverter());
	}

	/// <summary>
	/// Adds the <see cref="VersionConverter"/> as custom <see cref="System.Text.Json.Serialization.JsonConverter"/> to the builder.
	/// </summary>
	/// <param name="builder"> The extended <see cref="IJsonSettingsSerializerOptionsBuilder"/>. </param>
	/// <returns> An <see cref="IJsonSettingsSerializerOptionsBuilder"/> for chaining. </returns>
	public static IJsonSettingsSerializerOptionsBuilder WithVersionConverter(this IJsonSettingsSerializerOptionsBuilder builder)
	{
		return builder.AddConverter(new VersionConverter());
	}

	/// <summary>
	/// Adds the <see cref="EnumConverter"/> as custom <see cref="System.Text.Json.Serialization.JsonConverter"/> to the builder.
	/// </summary>
	/// <param name="builder"> The extended <see cref="IJsonSettingsSerializerOptionsBuilder"/>. </param>
	/// <param name="options"> The <see cref="EnumConverterOptions"/> used for (de)serialization. </param>
	/// <returns> An <see cref="IJsonSettingsSerializerOptionsBuilder"/> for chaining. </returns>
	public static IJsonSettingsSerializerOptionsBuilder WithEnumConverter(this IJsonSettingsSerializerOptionsBuilder builder, EnumConverterOptions? options)
	{
		return builder.AddConverter(new EnumConverter(options));
	}

	/// <summary>
	/// Adds the <see cref="EnumConverter"/> as custom <see cref="System.Text.Json.Serialization.JsonConverter"/> to the builder.
	/// </summary>
	/// <param name="builder"> The extended <see cref="IJsonSettingsSerializerOptionsBuilder"/>. </param>
	/// <param name="writeOutOptions"> The <see cref="IWriteOutOptions"/> used for writing out enumeration values. </param>
	/// <returns> An <see cref="IJsonSettingsSerializerOptionsBuilder"/> for chaining. </returns>
	public static IJsonSettingsSerializerOptionsBuilder WithEnumConverter(this IJsonSettingsSerializerOptionsBuilder builder, IWriteOutOptions writeOutOptions)
	{
		return builder.AddConverter(new EnumConverter(new EnumConverterOptions() { WriteOutOptions = writeOutOptions }));
	}
}