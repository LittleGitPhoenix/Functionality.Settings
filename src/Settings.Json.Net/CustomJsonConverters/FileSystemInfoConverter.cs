#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Phoenix.Functionality.Settings.Json.Net.CustomJsonConverters
{
	//! Using the base class 'FileSystemInfo' does not work. It seems that the type must be an exact match.
	///// <summary>
	///// Custom json converter for <see cref="FileSystemInfo"/>.
	///// </summary>
	//public class FileSystemInfoConverter : JsonConverter<FileSystemInfo>
	//{
	//	/// <inheritdoc />
	//	public override FileSystemInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	//	{
	//		try
	//		{
	//			if (typeToConvert == typeof(FileInfo))
	//			{
	//				return new FileInfo(reader.GetString());
	//			}
	//			else
	//			{
	//				return new DirectoryInfo(reader.GetString());
	//			}
	//		}
	//		catch (Exception)
	//		{
	//			throw new JsonException($"Cannot convert the value '{(reader.GetString() ?? "[NULL]")}' of type {reader.TokenType} into a {nameof(FileSystemInfo)}.");
	//		}
	//	}

	//	/// <inheritdoc />
	//	public override void Write(Utf8JsonWriter writer, FileSystemInfo value, JsonSerializerOptions options)
	//	{
	//		writer.WriteStringValue(value.FullName);
	//	}
	//}

	/// <summary>
	/// Custom json converter for <see cref="FileInfo"/>.
	/// </summary>
	public class FileInfoConverter : JsonConverter<FileInfo>
	{
		/// <inheritdoc />
		public override FileInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			try
			{
				return new FileInfo(reader.GetString());
			}
			catch (Exception)
			{
				throw new JsonException($"Cannot convert the value '{(reader.GetString() ?? "[NULL]")}' of type {reader.TokenType} into a {nameof(FileInfo)}.");
			}
		}

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, FileInfo value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.FullName);
		}
	}

	/// <summary>
	/// Custom json converter for <see cref="DirectoryInfo"/>.
	/// </summary>
	public class DirectoryInfoConverter : JsonConverter<DirectoryInfo>
	{
		/// <inheritdoc />
		public override DirectoryInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			try
			{
				return new DirectoryInfo(reader.GetString());
			}
			catch (Exception)
			{
				throw new JsonException($"Cannot convert the value '{(reader.GetString() ?? "[NULL]")}' of type {reader.TokenType} into a {nameof(DirectoryInfo)}.");
			}
		}

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, DirectoryInfo value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.FullName);
		}
	}
}