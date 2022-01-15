#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Text.Json;
using System.Text.Json.Serialization;

namespace Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;

//! Using the base class 'FileSystemInfo' does not work. It seems that the type must be an exact match.

/// <summary>
/// Custom json converter for <see cref="DirectoryInfo"/>.
/// </summary>
public class DirectoryInfoConverter : JsonConverter<DirectoryInfo>
{
	private readonly DirectoryInfo? _baseDirectory;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="baseDirectory"> Optional base directory used to determine relative path. </param>
	public DirectoryInfoConverter(DirectoryInfo? baseDirectory = null)
	{
		_baseDirectory = baseDirectory;
	}

	/// <inheritdoc />
	public override DirectoryInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		try
		{
			if (value is null) throw new NotSupportedException();
			return new DirectoryInfo(Path.Combine(_baseDirectory?.FullName ?? String.Empty, value));
		}
		catch (Exception)
		{
			throw new JsonException($"Cannot convert the value '{value ?? "[NULL]"}' of type {reader.TokenType} into a {nameof(DirectoryInfo)}.");
		}
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, DirectoryInfo value, JsonSerializerOptions options)
	{
		var path = value.GetRelativePath(_baseDirectory);
		writer.WriteStringValue(path);
	}
}

/// <summary>
/// Custom json converter for <see cref="FileInfo"/>.
/// </summary>
public class FileInfoConverter : JsonConverter<FileInfo>
{
	private readonly DirectoryInfo? _baseDirectory;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="baseDirectory"> Optional base directory used to determine relative path. </param>
	public FileInfoConverter(DirectoryInfo? baseDirectory = null)
	{
		_baseDirectory = baseDirectory;
	}

	/// <inheritdoc />
	public override FileInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		try
		{
			if (value is null) throw new NotSupportedException();
			return new FileInfo(Path.Combine(_baseDirectory?.FullName ?? String.Empty, value));
		}
		catch (Exception)
		{
			throw new JsonException($"Cannot convert the value '{value ?? "[NULL]"}' of type {reader.TokenType} into a {nameof(FileInfo)}.");
		}
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, FileInfo value, JsonSerializerOptions options)
	{
		var parentDirectory = value.Directory;
		var path = parentDirectory is not null ? Path.Combine(parentDirectory.GetRelativePath(_baseDirectory), value.Name) : value.FullName;
		writer.WriteStringValue(path);
	}
}

internal static class FileSystemInfoHelper
{
	internal static string GetRelativePath(this FileSystemInfo info, DirectoryInfo? baseDirectory)
	{
		var path = info.FullName;
		if (baseDirectory is null) return path;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET5_0_OR_GREATER
		var relativePath = Path.GetRelativePath(baseDirectory.FullName, path);
#else
		//! Older framework version will not be supported. They get absolute path no matter what.
		var relativePath = path;
#endif
		if (relativePath.GetOccurrenceCount("..") >= 4) return path;
		return relativePath;
	}

	internal static uint GetOccurrenceCount(this string value, string search, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
	{
		var count = (uint)0;
		var searchLength = search.Length;
		var valueLength = value.Length;
		for (var index = 0; index < value.Length; index++)
		{
			var part = index + searchLength > valueLength ? value.Substring(index) : value.Substring(index, searchLength);
			if (search.Equals(part, comparison))
			{
				// With each found match set the index to the end of the match.
				index += searchLength - 1; //! minus one because the index is automatically increased each turn.
				count++;
			}
		}
		return count;
	}
}