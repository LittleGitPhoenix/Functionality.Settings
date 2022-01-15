#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;

/// <summary>
/// Custom json converter for <see cref="TimeSpan"/>.
/// </summary>
public class TimeSpanConverter : JsonConverter<TimeSpan>
{
	#region Deserialization

	/// <inheritdoc />
	public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TryGetInt64(out var numeric) && this.TryDeserialize(numeric, out var timeSpan)) return timeSpan;
		throw new JsonException($"Cannot convert the value '{reader.GetString()}' of type {reader.TokenType} into a {nameof(TimeSpan)}.");
	}

	internal bool TryDeserialize(string value, out TimeSpan timeSpan)
	{
		if (long.TryParse(value, out var numeric)) return this.TryDeserialize(numeric, out timeSpan);

		timeSpan = default;
		return false;
	}

	internal bool TryDeserialize(long numeric, out TimeSpan timeSpan)
	{
		timeSpan = TimeSpan.FromMilliseconds(numeric);
		return true;
	}

	#endregion

	#region Serialization

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, TimeSpan timeSpan, JsonSerializerOptions options)
	{
		if (this.TrySerialize(timeSpan, out long numeric))
		{
			writer.WriteNumberValue(numeric);
			return;
		}
		throw new JsonException($"Cannot convert time span '{timeSpan}' into a string.");
	}

	internal bool TrySerialize(TimeSpan timeSpan, out string value)
	{
		if (this.TrySerialize(timeSpan, out long numeric))
		{
			value = numeric.ToString();
			return true;
		}
		value = String.Empty;
		return false;
	}

	internal bool TrySerialize(TimeSpan timeSpan, out long numeric)
	{
		numeric = (long) timeSpan.TotalMilliseconds;
		return true;
	}

	#endregion
}