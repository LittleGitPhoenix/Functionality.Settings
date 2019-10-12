#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Phoenix.Functionality.Settings.Json.Newtonsoft.CustomJsonConverters
{
	/// <summary>
	/// Custom Json.NET converter for <see cref="Regex"/>.
	/// </summary>
	public class RegexConverter : JsonConverter<Regex>
	{
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, Regex value, JsonSerializer serializer)
		{
			//! The regex instance has no public property that allows access to its pattern. Using the ToString() method works, but this could change at any time.
			writer.WriteValue(value.ToString());
		}

		/// <inheritdoc />
		public override Regex ReadJson(JsonReader reader, Type objectType, Regex existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return new Regex(reader.Value.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}
	}
}