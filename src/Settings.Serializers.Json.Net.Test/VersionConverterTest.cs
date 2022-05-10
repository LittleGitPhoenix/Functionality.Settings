using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;

namespace Settings.Serializers.Json.Net.Test;

public class VersionConverterTest
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
	public void Deserialize_Version()
	{
		// Arrange
		var converter = new VersionConverter();
		var value = "1.2.3.4";
		var targetVersion = new Version(1,2,3,4);

		// Act
		var version = converter.Deserialize(value);
			
		// Assert
		Assert.NotNull(version);
		Assert.AreEqual(targetVersion, version);
	}

	[Test]
	public void Deserialize_Null_Version()
	{
		// Arrange
		var converter = new VersionConverter();
		string? value = null;

		// Act
		var version = converter.Deserialize(value);

		// Assert
		Assert.Null(version);
	}

	[Test]
	public void Serialize_Version()
	{
		// Arrange
		var converter = new VersionConverter();
		var version = new Version(1, 2, 3, 4);
		var targetValue = "1.2.3.4";

		// Act
		var value = converter.Serialize(version);

		// Assert
		Assert.NotNull(value);
		Assert.AreEqual(targetValue, value);
	}

	[Test]
	public void Serialize_Null_Version()
	{
		// Arrange
		var converter = new VersionConverter();
		Version? version = null;
		
		// Act
		var value = converter.Serialize(version);

		// Assert
		Assert.Null(value);
	}

	#endregion
}