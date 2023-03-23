using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Settings;
using Phoenix.Functionality.Settings.Serializers.Json.Net;
using Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;

namespace Settings.Serializers.Json.Net.Test;

public class EnumConverterTest
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

	class MySettings : ISettings
	{
		public MyEnum First { get; init; } = MyEnum.Default;
		public MyEnum Second { get; init; } = MyEnum.Default;
	}

	class MyNullableSettings : ISettings
	{
		public MyEnum? First { get; init; } = null;
	}

	enum MyEnum
	{
		Default,
		Entry1,
		Entry2,
	}

	private static string MyEnumSuffix = $" [{nameof(MyEnum.Default)}, {nameof(MyEnum.Entry1)}, {nameof(MyEnum.Entry2)}]";

	enum MyEnumWithoutDefault
	{
		First = 1,
		Entry2 = 2,
		Entry3 = 3,
	}

	[System.ComponentModel.DefaultValue(MyEnumWithDefaultAttribute.Default)]
	enum MyEnumWithDefaultAttribute
	{
		Entry1 = 1,
		Entry2 = 2,
		Default = 3,
	}

	[Flags]
	enum MyFlags
	{
		God = 0b_0000,
		Me = 0b_0001,
		Myself = 0b_0010,
		I = 0b_0100,
	}

	private static string MyFlagsSuffix = $" [{nameof(MyFlags.God)}, {nameof(MyFlags.Me)}, {nameof(MyFlags.Myself)}, {nameof(MyFlags.I)}]";

	#endregion

	#region Tests

	#region Helper
	
	[Test]
	[TestCase(MyEnum.Default)]
	[TestCase(MyEnumWithoutDefault.First)]
	[TestCase(MyEnumWithDefaultAttribute.Default)]
	public void GetDefaultEnumerationValue<TEnum>(TEnum target)
	{
		// Act
		var defaultValue = EnumConverter.InternalEnumConverter<TEnum>.GetDefaultValue();

		// Assert
		Assert.That(defaultValue, Is.EqualTo(target));
	}

	[Test]
	public void GetDefaultValueForNullableEnumeration()
	{
		// Act
		var defaultValue = EnumConverter.InternalEnumConverter<MyEnum?>.GetDefaultValue();

		// Assert
		Assert.That(defaultValue, Is.Null);
	}

	#endregion

	#region Deserialize

	[Test]
	public void DeserializeEnumeration()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyEnum>();
		var value = nameof(MyEnum.Default);
		var target = MyEnum.Default;
		
		// Act
		var actual = converter.Deserialize(value);

		// Assert
		Assert.NotNull(actual);
		Assert.That(actual, Is.EqualTo(target));
	}

	[Test]
	public void DeserializeEnumerationCleansWriteOut()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyEnum>();
		var value = $"{nameof(MyEnum.Entry2)}{MyEnumSuffix}";
		var target = MyEnum.Entry2;

		// Act
		var actual = converter.Deserialize(value);

		// Assert
		Assert.NotNull(actual);
		Assert.That(actual, Is.EqualTo(target));
	}

	[Test]
	public void DeserializeFlagsEnumeration()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyFlags>();
		var value = $"{nameof(MyFlags.Me)}, {nameof(MyFlags.Myself)}, {nameof(MyFlags.I)}";
		var target = MyFlags.Me | MyFlags.Myself | MyFlags.I;
		
		// Act
		var actual = converter.Deserialize(value);

		// Assert
		Assert.NotNull(actual);
		Assert.That(actual, Is.EqualTo(target));
	}

	[Test]
	public void DeserializeFlagsEnumerationCleansWriteOut()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyFlags>();
		var value = $"{nameof(MyFlags.Me)}, {nameof(MyFlags.Myself)}, {nameof(MyFlags.I)}{MyFlagsSuffix}";
		var target = MyFlags.Me | MyFlags.Myself | MyFlags.I;
		
		// Act
		var actual = converter.Deserialize(value);

		// Assert
		Assert.NotNull(actual);
		Assert.That(actual, Is.EqualTo(target));
	}

	[Test]
	public void DeserializeEnumerationIsCaseInsensitive()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyEnum>();
		var value = nameof(MyEnum.Default).ToLower();
		var target = MyEnum.Default;
		
		// Act
		var actual = converter.Deserialize(value);

		// Assert
		Assert.NotNull(actual);
		Assert.That(actual, Is.EqualTo(target));
	}

	[Test]
	public void DeserializeNullableEnumeration()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyEnum?>(); //! Nullable
		var value = nameof(MyEnum.Default);
		var target = (MyEnum?) MyEnum.Default;
		
		// Act
		var actual = converter.Deserialize(value);

		// Assert
		Assert.NotNull(actual);
		Assert.That(actual, Is.EqualTo(target));
	}

	/// <summary> Checks that converting a null-string into a <see cref="Enum?"/> succeeds. </summary>
	[Test]
	public void DeserializeNullableEnumProperty()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyEnum?>(); //! Nullable
		var value = (string?) null;

		// Act
		var actual = converter.Deserialize(value);
		
		//Assert
		Assert.That(actual, Is.Null);
	}

	/// <summary> Checks that converting <b>null</b> or empty strings into an <see cref="Enum"/> will return <b>null</b>, if the target type is nullable. </summary>
	[Test]
	[TestCase(null)]
	[TestCase("")]
	public void DeserializeEnumerationReturnsNull(string? value)
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyEnum?>(); //! Nullable
		var target = (MyEnum?) null;
		
		// Act
		var actual = converter.Deserialize(value);

		// Assert
		Assert.Null(actual);
		Assert.That(actual, Is.EqualTo(target));
	}

	/// <summary> Checks that converting <b>null</b> or empty strings into an <see cref="Enum"/> will return the enumerations default value, if the target type is not nullable. </summary>
	[Test]
	[TestCase(null)]
	[TestCase("")]
	public void DeserializeEnumerationReturnsDefault(string? value)
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyEnum>();
		var target = MyEnum.Default;
		
		// Act
		var actual = converter.Deserialize(value);

		// Assert
		Assert.NotNull(actual);
		Assert.That(actual, Is.EqualTo(target));
	}

	/// <summary> Checks that converting <b>null</b> or empty strings into an <see cref="Enum"/> will return the enumerations first value, if the target type is not nullable and the enum does not provide a default value. </summary>
	[Test]
	[TestCase(null)]
	[TestCase("")]
	public void DeserializeEnumerationThrowsBecauseNoDefault(string? value)
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyEnumWithoutDefault>();
		var target = MyEnumWithoutDefault.First;
		
		// Act + Assert
		var actual = converter.Deserialize(value);

		// Assert
		Assert.NotNull(actual);
		Assert.That(actual, Is.EqualTo(target));
	}

	/// <summary> Checks that converting a string that is not defined within an <see cref="Enum"/> will throw. </summary>
	[Test]
	public void DeserializeEnumerationThrowsIfNotDefined()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyEnum>();
		var value = "not_defined";
		var target = MyEnum.Default;
		
		// Act + Assert
		Assert.Catch<System.Text.Json.JsonException>(() => converter.Deserialize(value));
	}

	[Test]
	public void DeserializeNullableEnumPropertyInSettings()
	{
		// Arrange
		//var options = new EnumConverterOptions() {WriteOutOptions = WriteOutValues.AsSuffix()};
		var converter = new EnumConverter(/*options*/);
		var serializer = new JsonSettingsSerializer(converter);
		
		//var settingsData = @"{""First"": null, ""Second"": ""Entry2""}";
		var settingsData = @"{""First"": null}";
		
		// Act
		var settings = serializer.Deserialize<MyNullableSettings>(settingsData);
		
		// Assert
		Assert.That(settings, Is.Not.Null);
		Assert.That(settings!.First, Is.Null);
	}

	[Test]
	public void DeserializeEnumPropertyInSettings()
	{
		// Arrange
		var converter = new EnumConverter();
		var serializer = new JsonSettingsSerializer(converter);
		var settingsData = @"{""First"": ""Entry1"", ""Second"": ""Entry2""}";
		
		// Act
		var settings = serializer.Deserialize<MySettings>(settingsData);
		
		// Assert
		Assert.That(settings, Is.Not.Null);
		Assert.That(settings!.First, Is.EqualTo(MyEnum.Entry1));
		Assert.That(settings!.Second, Is.EqualTo(MyEnum.Entry2));
	}

	#endregion

	#region Serialize

	[Test]
	public void SerializeEnumeration()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyEnum>();
		var value = MyEnum.Entry1;
		var target = nameof(MyEnum.Entry1);

		// Act
		var actual = converter.Serialize(value);

		// Assert
		Assert.NotNull(actual);
		Assert.That(actual, Is.EqualTo(target));
	}

	[Test]
	public void SerializeEnumerationWithWriteOut()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyEnum>(new EnumConverterOptions() { WriteOutOptions = WriteOutValues.AsSuffix() });
		var value = MyEnum.Entry1;
		var target = $"{nameof(MyEnum.Entry1)}{MyEnumSuffix}";

		// Act
		var actual = converter.Serialize(value);

		// Assert
		Assert.That(actual, Is.EqualTo(target));
	}

	[Test]
	public void SerializeNullEnumeration()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter<MyEnum?>();
		var value = (MyEnum?) null;
		var target = (string?) null;

		// Act
		var actual = converter.Serialize(value);

		// Assert
		Assert.Null(actual);
		Assert.That(actual, Is.EqualTo(target));
	}

	[Test]
	public void SerializeAndDeserializeWithWriteOutAsSuffix()
	{
		// Arrange
		var converter = new EnumConverter(WriteOutValues.AsSuffix(start: "[", separator: ";", end: "]"));
		var serializer = new JsonSettingsSerializer(converter);
		var settings = new MySettings() { First = MyEnum.Entry1, Second = MyEnum.Entry2 };

		// Act + Assert
		var settingsData = serializer.Serialize(settings);
		Console.WriteLine(settingsData);
		Assert.That(settingsData, Is.Not.Empty);
		Assert.That(settingsData, Does.Contain($"\"{nameof(MySettings.First)}\"").IgnoreCase);
		Assert.That(settingsData, Does.Contain($"\"{nameof(MyEnum.Entry1)} [").IgnoreCase);
		Assert.That(settingsData, Does.Contain($"\"{nameof(MySettings.Second)}\"").IgnoreCase);
		Assert.That(settingsData, Does.Contain($"\"{nameof(MyEnum.Entry2)} [").IgnoreCase);
		
		// Assert + Assert
		var deserializedSettings = serializer.Deserialize<MySettings>(settingsData);
		Assert.NotNull(deserializedSettings);
		Assert.That(deserializedSettings!.First, Is.EqualTo(settings.First));
		Assert.That(deserializedSettings!.Second, Is.EqualTo(settings.Second));
	}

	[Test]
	public void SerializeAndDeserializeWithWriteOutAsSuffixForNullableEnumeration()
	{
		// Arrange
		var converter = new EnumConverter(WriteOutValues.AsSuffix(start: "[", separator: ";", end: "]"));
		var serializer = new JsonSettingsSerializer(converter);
		var settings = new MyNullableSettings() { First = null };

		// Act + Assert
		var settingsData = serializer.Serialize(settings);
		Console.WriteLine(settingsData);
		Assert.That(settingsData, Is.Not.Empty);
		Assert.That(settingsData, Does.Contain(nameof(MyNullableSettings.First)).IgnoreCase);
		Assert.That(settingsData, Does.Contain("null").IgnoreCase);
		
		// Assert + Assert
		var deserializedSettings = serializer.Deserialize<MyNullableSettings>(settingsData);
		Assert.NotNull(deserializedSettings);
		Assert.That(deserializedSettings!.First, Is.EqualTo(settings.First));
	}
	
	[Test]
	public void SerializedNullableEnumPropertyDoesNotHaveQuatationMarks()
	{
		// Arrange
		var converter = new EnumConverter();
		var serializer = new JsonSettingsSerializer(converter);
		var settings = new MyNullableSettings() { First = null };

		// Act + Assert
		var settingsData = serializer.Serialize(settings);
		Console.WriteLine(settingsData);
		Assert.That(settingsData, Is.Not.Empty);
		Assert.That(settingsData, Does.Contain("null")); //! Must contain a pure null without quotation marks.
		Assert.That(settingsData, Does.Not.Contain("\"null\""));
		
		// Assert + Assert
		var deserializedSettings = serializer.Deserialize<MyNullableSettings>(settingsData);
		Assert.NotNull(deserializedSettings);
		Assert.That(deserializedSettings!.First, Is.EqualTo(settings.First));
	}

	#endregion

	#endregion
}