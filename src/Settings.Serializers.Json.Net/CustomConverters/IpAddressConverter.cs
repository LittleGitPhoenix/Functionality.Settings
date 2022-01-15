#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;

/// <summary>
/// Custom json converter for <see cref="IPAddress"/>.
/// </summary>
public class IpAddressConverter : JsonConverter<IPAddress>
{
	/// <inheritdoc />
	public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> this.Deserialize(reader.GetString());

	internal IPAddress Deserialize(string? value)
	{
		if (String.Equals(value, "localhost", StringComparison.OrdinalIgnoreCase)) return IPAddress.Loopback;
		if (String.Equals(value, nameof(IPAddress.Loopback), StringComparison.OrdinalIgnoreCase)) return IPAddress.Loopback;
		if (String.Equals(value, nameof(IPAddress.IPv6Loopback), StringComparison.OrdinalIgnoreCase)) return IPAddress.IPv6Loopback;
		if (String.Equals(value, nameof(IPAddress.Broadcast), StringComparison.OrdinalIgnoreCase)) return IPAddress.Broadcast;
		
		if (IPAddress.TryParse(value, out var ip)) return ip;
		throw new JsonException($"Cannot convert the value '{value}' into an {nameof(IPAddress)}.");
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
		=> writer.WriteStringValue(this.Serialize(value));

	internal string Serialize(IPAddress ipAddress)
	{
		if (Equals(ipAddress, IPAddress.Loopback)) return nameof(IPAddress.Loopback);
		if (Equals(ipAddress, IPAddress.IPv6Loopback)) return nameof(IPAddress.IPv6Loopback);
		if (Equals(ipAddress, IPAddress.Broadcast)) return nameof(IPAddress.Broadcast);

		return ipAddress.ToString();
	}
}