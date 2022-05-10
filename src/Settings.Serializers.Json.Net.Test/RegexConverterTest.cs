using System.Text.RegularExpressions;
using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;

namespace Settings.Serializers.Json.Net.Test;

public class RegexConverterTest
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
	public void Deserialize_Regex_From_String()
	{
		// Arrange
		var lookup = "OnlyThisAndNothingElse";
		var targetPattern = $"^{lookup}$";
		var converter = new RegexConverter();

		// Act
		var regex = converter.Deserialize(targetPattern);
		
		// Assert
		Assert.True(regex.IsMatch(lookup));
		Assert.False(regex.IsMatch(_fixture.Create<string>()));
		Assert.AreEqual(RegexConverter.DefaultRegexOptions, regex.Options);
	}

	[Test]
	public void Deserialize_Regex_Uses_Custom_Options()
	{
		// Arrange
		var customOptions = RegexOptions.Multiline;
		var converter = new RegexConverter(customOptions);

		// Act
		var regex = converter.Deserialize(_fixture.Create<string>());
		
		// Assert
		Assert.AreEqual(customOptions, regex.Options);
	}

	[Test]
	public void Deserialize_Regex_From_String_Uses_Fallback()
	{
		// Arrange
		string? targetPattern = null;
		var converter = new RegexConverter();

		// Act
		var regex = converter.Deserialize(targetPattern);
		
		// Assert
		Assert.AreEqual(RegexConverter.DefaultFallbackPattern, regex.ToString());
		Assert.AreEqual(RegexConverter.DefaultRegexOptions, regex.Options);
	}

	[Test]
	public void Deserialize_Regex_From_String_Uses_Custom_Fallback()
	{
		// Arrange
		var customFallback = "Test";
		string? targetPattern = null;
		var converter = new RegexConverter(customFallback);

		// Act
		var regex = converter.Deserialize(targetPattern);

		// Assert
		Assert.AreEqual(customFallback, regex.ToString());
		Assert.AreEqual(RegexConverter.DefaultRegexOptions, regex.Options);
	}

	[Test]
	public void Serialize_Regex()
	{
		// Arrange
		var lookup = "OnlyThisAndNothingElse";
		var targetPattern = $"^{lookup}$";
		var regex = new Regex(targetPattern);
		var converter = new RegexConverter();

		// Act
		var actualPattern = converter.Serialize(regex);

		// Assert
		Assert.AreEqual(targetPattern, actualPattern);
	}

	#endregion
}