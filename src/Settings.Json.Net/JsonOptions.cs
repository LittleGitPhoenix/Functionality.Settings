#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Phoenix.Functionality.Settings.Json.Net.CustomJsonConverters;

namespace Phoenix.Functionality.Settings.Json.Net
{
	/// <summary>
	/// Provides predefined options for Json.Net.
	/// </summary>
	public class JsonOptionProvider
	{
		/// <summary>
		/// Returns predefined <see cref="JsonSerializerOptions"/>.
		/// </summary>
		/// <param name="basePath"> Optional base directory that can be used by the different <see cref="JsonConverter"/>s. </param>
		/// <returns> <see cref="JsonSerializerOptions"/> </returns>
		public static JsonSerializerOptions GetOptions(DirectoryInfo basePath)
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
					new FileInfoConverter(basePath),
					new DirectoryInfoConverter(basePath),
					new RegexConverter(),
					new TimeSpanConverter(),
					new IpAddressConverter(),
				}
			};
		}
	}
}