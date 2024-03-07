using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Settings;

namespace Settings.Test;

public class SettingsTest
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

	#region Data

	private const string CustomSettingsName = "MySettings";

	// ReSharper disable once ClassNeverInstantiated.Local → Only the type is used for unit tests.
	class UnnamedSettings : ISettings { }

	[SettingsName(CustomSettingsName)]
	// ReSharper disable once ClassNeverInstantiated.Local → Only the type is used for unit tests.
	class NamedSettings : ISettings { }

	#endregion

	#region Tests

	[Test]
	public void GetSettingsName()
	{
		// Arrange
		var targetName = $"{typeof(UnnamedSettings).Namespace}.{nameof(UnnamedSettings)}";
		var settings = new UnnamedSettings();

		// Act
		var settingsFileName = settings.GetSettingsName();

		// Assert
		Assert.That(targetName, Is.EqualTo(settingsFileName));
	}

	[Test]
	public void GetCustomSettingsName()
	{
		// Arrange
		var targetName = CustomSettingsName;
		var settings = new NamedSettings();

		// Act
		var settingsFileName = settings.GetSettingsName();

		// Assert
		Assert.That(targetName, Is.EqualTo(settingsFileName));
	}

	#endregion
}