using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Settings;
using Phoenix.Functionality.Settings.Json.Net.CustomJsonConverters;

namespace Settings.Json.Net.Test;

public class FileSystemInfoConverterTest
{
#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[SetUp]
	public void Setup()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	#region Data

	class DirectorySettings : ISettings
	{
		public DirectoryInfo Directory { get; set; }
	}

	class FileSettings : ISettings
	{
		public FileInfo File { get; set; }
	}

	#endregion
		
	[Test]
	[TestCase(@"..\Test\some.file")]
	[TestCase(@"..\..\Test\some.file")]
	[TestCase(@"..\..\..\Test\some.file")]
	[TestCase(@"C:\some.file")]
	[TestCase(@"C:\Test\some.file")]
	[TestCase(@"C:\Test\..\some.file")]
	public void Check_FileInfo_Deserialization_Succeeds(string path)
	{
		// Arrange
		var jsonOptions = new JsonSerializerOptions()
		{
			AllowTrailingCommas = true,
			IgnoreReadOnlyProperties = false,
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReadCommentHandling = JsonCommentHandling.Skip,
			WriteIndented = true,
			Converters =
			{
				new FileInfoConverter(new DirectoryInfo(Environment.CurrentDirectory)),
				new DirectoryInfoConverter(new DirectoryInfo(Environment.CurrentDirectory)),
			}
		};
		var file = new FileInfo(path);
		var settingsString = $"{{\r\n  \"file\": \"{path.Replace(@"\", @"\\")}\"\r\n}}";

		// Act
		var deserializedSettings = JsonSerializer.Deserialize<FileSettings>(settingsString, jsonOptions);

		// Assert
		// Checking the instances for equality fails for FileInfo. Instead check the full name.
		Assert.AreEqual(deserializedSettings?.File.FullName, file.FullName);
	}
		
	[Test]
	[TestCase(@"..\Test")]
	[TestCase(@"..\..\Test")]
	[TestCase(@"..\..\..\Test")]
	[TestCase(@"C:\")]
	[TestCase(@"C:\Test")]
	[TestCase(@"C:\Test\..")]
	public void Check_DirectoryInfo_Deserialization_Succeeds(string path)
	{
		// Arrange
		var jsonOptions = new JsonSerializerOptions()
		{
			AllowTrailingCommas = true,
			IgnoreReadOnlyProperties = false,
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReadCommentHandling = JsonCommentHandling.Skip,
			WriteIndented = true,
			Converters =
			{
				new FileInfoConverter(new DirectoryInfo(Environment.CurrentDirectory)),
				new DirectoryInfoConverter(new DirectoryInfo(Environment.CurrentDirectory)),
			}
		};
		var directory = new DirectoryInfo(path);
		var settingsString = $"{{\r\n  \"directory\": \"{path.Replace(@"\", @"\\")}\"\r\n}}";
			
		// Act
		var deserializedSettings = JsonSerializer.Deserialize<DirectorySettings>(settingsString, jsonOptions);

		// Assert
		Assert.AreEqual(deserializedSettings?.Directory, directory);
	}
		
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET5_0_OR_GREATER
		[Test]
		[TestCase(@"..\Test\some.file", true)]
		[TestCase(@"..\..\Test\some.file", true)]
		[TestCase(@"..\..\..\Test\some.file", true)]
		[TestCase(@"..\..\..\..\Test\some.file", false)]
#endif
	[TestCase(@"C:\some.file", false)]
	[TestCase(@"C:\Test\some.file", false)]
	[TestCase(@"C:\Test\..\some.file", false)]
	public void Check_FileInfo_Serialization_Succeeds(string path, bool mustBeRelativeAfterSave)
	{
		// Arrange
		var jsonOptions = new JsonSerializerOptions()
		{
			AllowTrailingCommas = true,
			IgnoreReadOnlyProperties = false,
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReadCommentHandling = JsonCommentHandling.Skip,
			WriteIndented = true,
			Converters =
			{
				new FileInfoConverter(new DirectoryInfo(Environment.CurrentDirectory)),
				new DirectoryInfoConverter(new DirectoryInfo(Environment.CurrentDirectory)),
			}
		};
		var file = new FileInfo(path);
		var target = mustBeRelativeAfterSave ? path : file.FullName;
		var settings = new FileSettings() { File = file };

		// Act
		var settingsString = JsonSerializer.Serialize(settings, jsonOptions);

		// Assert
		Assert.That(settingsString, Contains.Substring(target.Replace(@"\", @"\\")));
		Assert.That(settingsString.ToLower(), Contains.Substring(nameof(FileSettings.File).ToLower()));
	}

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET5_0_OR_GREATER
		[Test]
		[TestCase(@"..\Test", true)]
		[TestCase(@"..\..\Test", true)]
		[TestCase(@"..\..\..\Test", true)]
		[TestCase(@"..\..\..\..\Test", false)]
#endif
	[TestCase(@"C:\", false)]
	[TestCase(@"C:\Test", false)]
	[TestCase(@"C:\Test\..", false)]
	public void Check_DirectoryInfo_Serialization_Succeeds(string path, bool mustBeRelativeAfterSave)
	{
		// Arrange
		var jsonOptions = new JsonSerializerOptions()
		{
			AllowTrailingCommas = true,
			IgnoreReadOnlyProperties = false,
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReadCommentHandling = JsonCommentHandling.Skip,
			WriteIndented = true,
			Converters =
			{
				new FileInfoConverter(new DirectoryInfo(Environment.CurrentDirectory)),
				new DirectoryInfoConverter(new DirectoryInfo(Environment.CurrentDirectory)),
			}
		};
		var directory = new DirectoryInfo(path);
		var target = mustBeRelativeAfterSave ? path : directory.FullName;
		var settings = new DirectorySettings() { Directory =  directory};

		// Act
		var settingsString = JsonSerializer.Serialize(settings, jsonOptions);

		// Assert
		Assert.That(settingsString, Contains.Substring(target.Replace(@"\", @"\\")));
		Assert.That(settingsString.ToLower(), Contains.Substring(nameof(DirectorySettings.Directory).ToLower()));
	}
}