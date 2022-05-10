#region LICENSE NOTICE

//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.

#endregion

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;

/// <summary>
/// Custom json converter for <see cref="Version"/>.
/// </summary>
public class VersionConverter : JsonConverter<Version>
{
	#region Deserialization

	/// <inheritdoc />
	public override Version? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> this.Deserialize(reader.GetString());

	internal Version? Deserialize(string? value)
	{
		if (value is null) return null;
		if (Version.TryParse(value, out var version)) return version;
		throw new JsonException($"Cannot convert the value '{value}' into a {nameof(Version)}.");
	}
	
	#endregion

	#region Serialization

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, Version? version, JsonSerializerOptions options)
		=> writer.WriteStringValue(this.Serialize(version));

	internal string? Serialize(Version? version)
	{
		return version?.ToString() ?? null;
	}
	
	#endregion
}