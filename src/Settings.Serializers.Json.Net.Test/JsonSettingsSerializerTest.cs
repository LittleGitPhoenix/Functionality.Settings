﻿using System.Dynamic;
using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Settings;
using Phoenix.Functionality.Settings.Serializers.Json.Net;
using Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;
using System.Text.Json;

namespace Settings.Serializers.Json.Net.Test;

public class JsonSettingsSerializerTest
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

	/// <summary>
	/// Special json serializer options, that create a simplistic json string. This is useful for comparing.
	/// </summary>
	internal static JsonSerializerOptions MinimizedJsonSerializerOptions = new JsonSerializerOptions();

	[SettingsName("Settings")]
	// ReSharper disable once ClassNeverInstantiated.Local → Only the type is used for unit tests.
	public class Settings : ISettings
	{
		public string Message { get; set; }
	}

	public class NotSerializableSettings : ISettings
	{
		public NotSerializableSettings Recursive { get; }

		public NotSerializableSettings()
		{
			this.Recursive = this;
		}
	}

	public class ChangeSettings : ISettings, ISettingsLayoutChangedNotification
	{
		public string Message { get; set; }

		/// <inheritdoc />
		public void LayoutChanged(ExpandoObject rawData) { }
	}

	#endregion

	[Test]
	public void Check_Serialization_Succeeds()
	{
		// Arrange
		var message = _fixture.Create<string>();
		var settings = new Settings() { Message = message };
		var targetData = $@"{{""{nameof(Settings.Message)}"":""{message}""}}";
		var serializer = new JsonSettingsSerializer(MinimizedJsonSerializerOptions);

		// Act
		var jsonData = serializer.Serialize(settings);

		// Assert
		Assert.True(String.Equals(jsonData, targetData, StringComparison.OrdinalIgnoreCase));
	}

	[Test]
	public void Check_Serialization_Wraps_Exceptions()
	{
		// Arrange
		var settings = new NotSerializableSettings();
		var serializer = new JsonSettingsSerializer(MinimizedJsonSerializerOptions);

		// Act + Assert
		Assert.Catch<SettingsSaveException>(() => serializer.Serialize(settings));
	}

	[Test]
	public void Check_Deserialization_Succeeds()
	{
		// Arrange
		var message = _fixture.Create<string>();
		var jsonData = $@"{{""{nameof(Settings.Message)}"":""{message}""}}";
		var serializer = new JsonSettingsSerializer(MinimizedJsonSerializerOptions);

		// Act
		var settings = serializer.Deserialize<Settings>(jsonData, out _);

		// Assert
		Assert.AreEqual(message, settings?.Message);
	}

	[Test]
	public void Check_Deserialization_Wraps_Exceptions()
	{
		// Arrange
		var serializer = new JsonSettingsSerializer(MinimizedJsonSerializerOptions);

		// Act + Assert
		Assert.Catch<SettingsLoadException>(() => serializer.Deserialize<Settings>("This is not a valid JSON string.", out _));
	}

	/// <summary> Checks if an <see cref="ISettings"/> instance is identical to its data representation. </summary>
	[Test]
	public void Check_Are_Identical()
	{
		// Arrange
		var message = _fixture.Create<string>();
		var settings = new Settings() {Message = message};
		var settingsData = $@"{{""{nameof(Settings.Message)}"":""{message}""}}";
		var serializer = new JsonSettingsSerializer(MinimizedJsonSerializerOptions);

		// Act
		var areIdentical = serializer.AreIdentical(settings, settingsData);

		// Assert
		Assert.True(areIdentical);
	}

	/// <summary> Checks if an <see cref="ISettings"/> instance is not identical to its data representation. </summary>
	[Test]
	public void Check_Are_Not_Identical()
	{
		// Arrange
		var message = _fixture.Create<string>();
		var settings = new Settings() {Message = message};
		var settingsData = $@"{{""{nameof(Settings.Message)}"":""{message}"",""Superfluous"":""Irrelevant""}}";
		var serializer = new JsonSettingsSerializer(MinimizedJsonSerializerOptions);

		// Act
		var areIdentical = serializer.AreIdentical(settings, settingsData);

		// Assert
		Assert.False(areIdentical);
	}

	/// <summary>
	/// Checks that identical <see cref="System.Text.Json.Serialization.JsonConverter"/>s are not added multiple times.
	/// </summary>
	[Test]
	public void Check_Converter_Is_Only_Added_Once_Per_Type()
	{
		// Arrange
		var converter01 = new FileInfoConverter();
		var converter02 = new FileInfoConverter();
		var converters = new List<System.Text.Json.Serialization.JsonConverter>();

		// Act + Assert
		Assert.Multiple
		(
			() =>
			{
				Assert.True(JsonSettingsSerializer.TryAddCustomConverter(converter01, converters));
				Assert.False(JsonSettingsSerializer.TryAddCustomConverter(converter02, converters));
				Assert.That(converters, Has.Count.EqualTo(1));
			}
		);
	}

	/// <summary> Checks the the raw data (<see cref="ExpandoObject"/>) is filled if the settings instance is of type <see cref="ISettingsLayoutChangedNotification"/>. </summary>
	[Test]
	public void Check_Raw_Data_Is_Filled()
	{
		// Arrange
		var message = _fixture.Create<string>();
		var settingsData = $@"{{""{nameof(Settings.Message)}"":""{message}"",""Superfluous"":""Irrelevant""}}";
		var serializer = new JsonSettingsSerializer();

		// Act
		serializer.Deserialize<ChangeSettings>(settingsData, out var rawData);

		// Assert
		Assert.NotNull(rawData);
		Assert.That(((IDictionary<string, object>) rawData!)[nameof(ChangeSettings.Message)].ToString(), Is.EqualTo(message));
		Assert.That(((dynamic) rawData).Superfluous.ToString(), Is.EqualTo("Irrelevant"));
	}
}