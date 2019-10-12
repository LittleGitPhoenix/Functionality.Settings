#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Threading;
using Newtonsoft.Json;
using Phoenix.Functionality.Settings.Json.Newtonsoft.CustomJsonConverters;

namespace Phoenix.Functionality.Settings.Json.Newtonsoft
{
	/// <summary>
	/// Provides predefined options for Json.Net.
	/// </summary>
	public class JsonOptions
	{
		/// <summary> Predefined <see cref="JsonSerializerSettings"/>. </summary>
		public static JsonSerializerSettings Instance => LazyInstance.Value;
		private static readonly Lazy<JsonSerializerSettings> LazyInstance = new Lazy<JsonSerializerSettings>(
			() =>
			{
				return new JsonSerializerSettings()
				{
					MissingMemberHandling = MissingMemberHandling.Ignore,
					DateFormatHandling = DateFormatHandling.IsoDateFormat,
					ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
					ObjectCreationHandling = ObjectCreationHandling.Replace, // This options lets Json.Net override lists that are filled with initial data instead of adding deserialized items to the existing list.
					TypeNameHandling = TypeNameHandling.None,                // If the settings object is more complex, it may be necessary to use 'TypeNameHandling.Auto' here.
					Converters =
					{
						new global::Newtonsoft.Json.Converters.StringEnumConverter(), // Save enumerations with their name rather than their number.
						new FileSystemInfoConverter(),
						new TimeSpanConverter(),
						new RegexConverter(),
						new IpAddressConverter(),
					},
					Error = (sender, args) =>
					{
						args.ErrorContext.Handled = true;
					},
				};
			}, LazyThreadSafetyMode.ExecutionAndPublication);
	}
}