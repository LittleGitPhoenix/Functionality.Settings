#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Phoenix.Functionality.Settings.Json.Net.CustomJsonConverters
{
	/// <summary>
	/// Custom json converter for <see cref="TimeSpan"/>.
	/// </summary>
	public class TimeSpanConverter : JsonConverter<TimeSpan>
	{
		/// <inheritdoc />
		public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TryGetInt64(out var numeric))
			{
				return TimeSpan.FromMilliseconds(numeric);
			}

			throw new JsonException($"Cannot convert the value '{reader.GetString()}' of type {reader.TokenType} into a {nameof(TimeSpan)}.");
		}

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
		{
			writer.WriteNumberValue((long) value.TotalMilliseconds);
		}
	}
}