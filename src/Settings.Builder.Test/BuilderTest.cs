using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Settings;
using Phoenix.Functionality.Settings.Cache;
using Phoenix.Functionality.Settings.Encryption;
using Phoenix.Functionality.Settings.Serializers.Json.Net;
using Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;
using Phoenix.Functionality.Settings.Sinks.File;

namespace Settings.Builder.Test;

public class BuilderTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[OneTimeSetUp]
	public void BeforeAllTests() { }

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTest() { }

	#endregion

	#region Tests

	[Test]
	public void Check_Build_With_File_Sink_And_Json_Serializer()
	{
		// Arrange
		var fileExtension = ".json";
		var baseDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

		// Act
		var settingsManager = SettingsManager<string>
			.Create()
			.UsingFileSink(fileExtension, baseDirectory)
			.UsingJsonSerializer()
			.WithFileInfoConverter(baseDirectory)
			.WithDirectoryInfoConverter(baseDirectory)
			.WithIpAddressConverter()
			.WithRegexConverter()
			.WithTimeSpanConverter()
			.WithDefaultSerializerOptions()
			.UsingWeakCache()
#if !DEBUG
			.UsingEncryption()
#endif
			.Build()
			;

		// Assert
		Assert.NotNull(settingsManager);
	}

	#endregion
}