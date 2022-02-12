using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;

namespace Settings.Serializers.Json.Net.Test;

public class TimeSpanConverterTest
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

	[Test]
	public void Deserialize_TimeSpan_From_Numeric()
	{
		// Arrange
		var converter = new TimeSpanConverter();
		var targetMilliseconds = 5000;

		// Act
		var success = converter.TryDeserialize(targetMilliseconds, out var timeSpan);
			
		// Assert
		Assert.True(success);
		Assert.AreEqual(timeSpan.TotalMilliseconds, targetMilliseconds);
	}

	[Test]
	public void Deserialize_TimeSpan_From_String()
	{
		// Arrange
		var converter = new TimeSpanConverter();
		var targetMilliseconds = 5000;
		var value = $"{targetMilliseconds}";

		// Act
		var success = converter.TryDeserialize(value, out var timeSpan);
			
		// Assert
		Assert.True(success);
		Assert.AreEqual(timeSpan.TotalMilliseconds, targetMilliseconds);
	}

	[Test]
	public void Serialize_TimeSpan_Into_Numeric()
	{
		// Arrange
		var converter = new TimeSpanConverter();
		var targetMilliseconds = 5000;
		var timeSpan = TimeSpan.FromMilliseconds(targetMilliseconds);

		// Act
		var success = converter.TrySerialize(timeSpan, out long numeric);

		// Assert
		Assert.True(success);
		Assert.AreEqual(numeric, targetMilliseconds);
	}

	[Test]
	public void Serialize_TimeSpan_Into_String()
	{
		// Arrange
		var converter = new TimeSpanConverter();
		var targetMilliseconds = 5000;
		var targetValue = $"{targetMilliseconds}";
		var timeSpan = TimeSpan.FromMilliseconds(targetMilliseconds);
			
		// Act
		var success = converter.TrySerialize(timeSpan, out string value);

		// Assert
		Assert.True(success);
		Assert.AreEqual(value, targetValue);
	}
}