﻿using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Settings;

namespace Settings.Test;

public class SettingsSinkExtensionsTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	#endregion

	#region Data

	private const string CustomSettingsName = "MyCustomSettingsName";

	// ReSharper disable once ClassNeverInstantiated.Local → Only the type is used for unit tests.
	class UnnamedSettings : ISettings { }

	[SettingsName(CustomSettingsName)]
	// ReSharper disable once ClassNeverInstantiated.Local → Only the type is used for unit tests.
	class NamedSettings : ISettings { }

	#endregion

	[Test]
	public void Get_Settings_Name()
	{
		// Arrange
		var targetName = $"{typeof(UnnamedSettings).Namespace}.{nameof(UnnamedSettings)}";
		var sink = _fixture.Create<ISettingsSink>();

		// Act
		var settingsFileName = sink.GetSettingsFileNameWithoutExtension<UnnamedSettings>();

		// Assert
		Assert.AreEqual(targetName, settingsFileName);
	}

	[Test]
	public void Get_Custom_Settings_Name()
	{
		// Arrange
		var targetName = CustomSettingsName;
		var sink = _fixture.Create<ISettingsSink>();

		// Act
		var settingsFileName = sink.GetSettingsFileNameWithoutExtension<NamedSettings>();

		// Assert
		Assert.AreEqual(targetName, settingsFileName);
	}
}