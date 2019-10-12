#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.IO;
using Newtonsoft.Json;

namespace Phoenix.Functionality.Settings.Json.Newtonsoft.CustomJsonConverters
{
	/// <summary>
	/// Custom Json.NET converter for <see cref="FileSystemInfo"/>.
	/// </summary>
	public class FileSystemInfoConverter : JsonConverter<FileSystemInfo>
	{
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, FileSystemInfo value, JsonSerializer serializer)
		{
			writer.WriteValue(value.FullName);
		}

		/// <inheritdoc />
		public override FileSystemInfo ReadJson(JsonReader reader, Type objectType, FileSystemInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			try
			{
				if (objectType == typeof(FileInfo))
				{
					return new FileInfo(reader.Value.ToString());
				}
				else
				{
					return new DirectoryInfo(reader.Value.ToString());
				}
			}
			catch (Exception)
			{
				throw new JsonSerializationException($"Cannot convert the value '{reader.Value}' of type {reader.ValueType} into a {nameof(FileSystemInfo)}.");
			}
		}
	}
}