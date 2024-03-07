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

	#region Tests

	[Test]
	public void DeserializeTimeSpanFromNumeric()
	{
		// Arrange
		var converter = new TimeSpanConverter();
		var targetMilliseconds = 5000;
		var value = targetMilliseconds;

		// Act
		var success = converter.TryDeserialize(value, out var timeSpan);

		// Assert
		Assert.That(success, Is.True);
		Assert.That(timeSpan.TotalMilliseconds, Is.EqualTo(targetMilliseconds));
	}

	[Test]
	public void DeserializeTimeSpanFromNumericString()
	{
		// Arrange
		var converter = new TimeSpanConverter();
		var targetMilliseconds = 5000;
		var value = $"{targetMilliseconds}";

		// Act
		var success = converter.TryDeserialize(value, out var timeSpan, couldBeNumeric: true);

		// Assert
		Assert.That(success, Is.True);
		Assert.That(timeSpan.TotalMilliseconds, Is.EqualTo(targetMilliseconds));
	}

	[Test]
	public void DeserializeTimeSpanFromString()
	{
		// Arrange
		var converter = new TimeSpanConverter();
		var targetMilliseconds = 5000;
		var value = "00:00:05";

		// Act
		var success = converter.TryDeserialize(value, out var timeSpan);
			
		// Assert
		Assert.That(success, Is.True);
		Assert.That(timeSpan.TotalMilliseconds, Is.EqualTo(targetMilliseconds));
	}

	[Test]
	public void DeserializeTimeSpanFromNull()
	{
		// Arrange
		var converter = new TimeSpanConverter();
		string? value = null;

		// Act
		var success = converter.TryDeserialize(value, out var timeSpan);

		// Assert
		Assert.That(success, Is.False);
	}

	[Test]
	public void SerializeTimeSpanIntoNumeric()
	{
		// Arrange
		var converter = new TimeSpanConverter();
		var targetMilliseconds = 5000;
		var timeSpan = TimeSpan.FromMilliseconds(targetMilliseconds);

		// Act
		var success = converter.TrySerialize(timeSpan, out long numeric);

		// Assert
		Assert.That(success, Is.True);
		Assert.That(numeric, Is.EqualTo(targetMilliseconds));
	}

	[Test]
	public void SerializeTimeSpanIntoString()
	{
		// Arrange
		var converter = new TimeSpanConverter();
		var targetMilliseconds = 5000;
		var targetValue = $"{targetMilliseconds}";
		var timeSpan = TimeSpan.FromMilliseconds(targetMilliseconds);
			
		// Act
		var success = converter.TrySerialize(timeSpan, out string value);

		// Assert
		Assert.That(success, Is.True);
		Assert.That(value, Is.EqualTo(targetValue));
	}

	#endregion
}