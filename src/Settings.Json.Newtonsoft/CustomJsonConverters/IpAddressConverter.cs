#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Net;
using Newtonsoft.Json;

namespace Phoenix.Functionality.Settings.Json.Newtonsoft.CustomJsonConverters
{
	/// <summary>
	/// Custom Json.NET converter for <see cref="IPAddress"/>.
	/// </summary>
	public class IpAddressConverter : JsonConverter<IPAddress>
	{
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, IPAddress value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString());
		}

		/// <inheritdoc />
		public override IPAddress ReadJson(JsonReader reader, Type objectType, IPAddress existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (IPAddress.TryParse(reader.Value.ToString(), out var ip)) return ip;
			throw new JsonSerializationException($"Cannot convert the value '{reader.Value}' of type {reader.ValueType} into a {nameof(IPAddress)}.");
		}
	}
}