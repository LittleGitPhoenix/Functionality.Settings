#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Phoenix.Functionality.Settings.Json.Net.CustomJsonConverters
{
	/// <summary>
	/// Custom json converter for <see cref="Regex"/>.
	/// </summary>
	public class RegexConverter : JsonConverter<Regex>
	{
		/// <inheritdoc />
		public override Regex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var value = reader.GetString() ?? ".*";
			return new Regex(value, RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, Regex value, JsonSerializerOptions options)
		{
			//! The regex instance has no public property that allows access to its pattern. Using the ToString() method works, but this could change at any time.
			writer.WriteStringValue(value.ToString());
		}
	}
}