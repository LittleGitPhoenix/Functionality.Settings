#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Phoenix.Functionality.Settings.Json.Net.CustomJsonConverters;

/// <summary>
/// Custom json converter for <see cref="IPAddress"/>.
/// </summary>
public class IpAddressConverter : JsonConverter<IPAddress>
{
	/// <inheritdoc />
	public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (IPAddress.TryParse(reader.GetString(), out var ip)) return ip;
		throw new JsonException($"Cannot convert the value '{reader.GetString()}' of type {reader.TokenType} into a {nameof(IPAddress)}.");
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}