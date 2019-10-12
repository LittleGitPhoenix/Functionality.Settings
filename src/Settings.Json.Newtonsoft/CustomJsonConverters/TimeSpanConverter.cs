#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using Newtonsoft.Json;

namespace Phoenix.Functionality.Settings.Json.Newtonsoft.CustomJsonConverters
{
	/// <summary>
	/// Custom Json.NET converter for <see cref="TimeSpan"/>.
	/// </summary>
	public class TimeSpanConverter : JsonConverter<TimeSpan>
	{
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
		{
			writer.WriteValue((long) value.TotalMilliseconds);
		}

		/// <inheritdoc />
		public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (long.TryParse(reader.Value.ToString(), out var numeric))
			{
				return TimeSpan.FromMilliseconds(numeric);
			}

			throw new JsonSerializationException($"Cannot convert the value '{reader.Value}' of type {reader.ValueType} into a {nameof(TimeSpan)}.");
		}
	}
}