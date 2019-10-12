#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Phoenix.Functionality.Settings.Json.Net.CustomJsonConverters;

namespace Phoenix.Functionality.Settings.Json.Net
{
	/// <summary>
	/// Provides predefined options for Json.Net.
	/// </summary>
	public class JsonOptions
	{
		/// <summary> Predefined <see cref="JsonSerializerOptions"/>. </summary>
		public static JsonSerializerOptions Instance => LazyInstance.Value;

		private static readonly Lazy<JsonSerializerOptions> LazyInstance = new Lazy<JsonSerializerOptions>
		(
			() =>
			{
				return new JsonSerializerOptions()
				{
					AllowTrailingCommas = true,
					IgnoreReadOnlyProperties = false,
					PropertyNameCaseInsensitive = true,
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					ReadCommentHandling = JsonCommentHandling.Skip,
					WriteIndented = true,
					Converters =
					{
						// Build in:
						new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false),

						// Custom:
						new FileInfoConverter(),
						new DirectoryInfoConverter(),
						new RegexConverter(),
						new TimeSpanConverter(),
						new IpAddressConverter(),
					}
				};
			}, LazyThreadSafetyMode.ExecutionAndPublication
		);
	}
}