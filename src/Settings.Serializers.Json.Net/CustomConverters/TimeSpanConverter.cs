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
		try
		{
			if (reader.TryGetInt64(out var numeric) && this.TryDeserialize(numeric, out var timeSpan)) return timeSpan;
		}
		// Thrown if the value is not numeric.
		//* Directly obtaining the value as string would fail if it is a pure number, so those differences must be handled with a try...catch.
		catch (InvalidOperationException ex)
		{
			var value = reader.GetString();
			if (this.TryDeserialize(value, out var timeSpan)) return timeSpan;
		}
		
		throw new JsonException($"Cannot convert the value '{reader.GetString()}' of type {reader.TokenType} into a {nameof(TimeSpan)}.");
	}
	
	internal bool TryDeserialize(long numeric, out TimeSpan timeSpan)
	{
		timeSpan = TimeSpan.FromMilliseconds(numeric);
		return true;
	}

	internal bool TryDeserialize(string value, out TimeSpan timeSpan, bool couldBeNumeric = false)
	{
		if (couldBeNumeric && long.TryParse(value, out var numeric)) return this.TryDeserialize(numeric, out timeSpan); // This shouldn't be necessary because the numeric check was already done in the 'Read' method, but for unit testing this is helpful.
		if (TimeSpan.TryParse(value, out timeSpan)) return true;
		return false;
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