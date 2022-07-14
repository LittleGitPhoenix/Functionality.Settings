using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Settings;
using Phoenix.Functionality.Settings.Serializers.Json.Net;
using Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;

//using EnumConverter = Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters.EnumConverter.InternalEnumConverter;

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
	[TestCase(typeof(MyEnum), MyEnum.Default)]
	[TestCase(typeof(MyEnumWithoutDefault), MyEnumWithoutDefault.First)]
	[TestCase(typeof(MyEnumWithDefaultAttribute), MyEnumWithDefaultAttribute.Default)]
	public void GetDefaultEnumerationValue(Type enumerationType, object target)
	{
		// Act
		var defaultValue = EnumConverter.InternalEnumConverter.GetDefaultValue(enumerationType);

		// Assert
		Assert.AreEqual(target, defaultValue);
	}

	#endregion

	#region Deserialize

	[Test]
	public void DeserializeEnumeration()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter();
		var value = nameof(MyEnum.Default);
		var target = MyEnum.Default;
		var type = target.GetType();

		// Act
		var actual = converter.Deserialize(value, type);
			
		// Assert
		Assert.NotNull(actual);
		Assert.AreEqual(target, actual);
	}

	[Test]
	public void DeserializeEnumerationCleansWriteOut()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter();
		var value = $"{nameof(MyEnum.Entry2)}{MyEnumSuffix}";
		var target = MyEnum.Entry2;
		var type = target.GetType();

		// Act
		var actual = converter.Deserialize(value, type);

		// Assert
		Assert.NotNull(actual);
		Assert.AreEqual(target, actual);
	}

	[Test]
	public void DeserializeFlagsEnumeration()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter();
		var value = $"{nameof(MyFlags.Me)}, {nameof(MyFlags.Myself)}, {nameof(MyFlags.I)}";
		var target = MyFlags.Me | MyFlags.Myself | MyFlags.I;
		var type = target.GetType();

		// Act
		var actual = converter.Deserialize(value, type);
			
		// Assert
		Assert.NotNull(actual);
		Assert.AreEqual(target, actual);
	}

	[Test]
	public void DeserializeFlagsEnumerationCleansWriteOut()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter();
		var value = $"{nameof(MyFlags.Me)}, {nameof(MyFlags.Myself)}, {nameof(MyFlags.I)}{MyFlagsSuffix}";
		var target = MyFlags.Me | MyFlags.Myself | MyFlags.I;
		var type = target.GetType();

		// Act
		var actual = converter.Deserialize(value, type);

		// Assert
		Assert.NotNull(actual);
		Assert.AreEqual(target, actual);
	}

	[Test]
	public void DeserializeEnumerationIsCaseInsensitive()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter();
		var value = nameof(MyEnum.Default).ToLower();
		var target = MyEnum.Default;
		var type = target.GetType();

		// Act
		var actual = converter.Deserialize(value, type);
			
		// Assert
		Assert.NotNull(actual);
		Assert.AreEqual(target, actual);
	}

	[Test]
	public void DeserializeNullableEnumeration()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter();
		var value = nameof(MyEnum.Default);
		var target = (MyEnum?) MyEnum.Default;
		var type = typeof(MyEnum?); //! nullable

		// Act
		var actual = converter.Deserialize(value, type);
			
		// Assert
		Assert.NotNull(actual);
		Assert.AreEqual(target, actual);
	}

	/// <summary> Checks that converting <b>null</b> or empty strings into an <see cref="Enum"/> will return <b>null</b>, if the target type is nullable. </summary>
	[Test]
	[TestCase(null)]
	[TestCase("")]
	public void DeserializeEnumerationReturnsNull(string? value)
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter();
		var target = (MyEnum?) null;
		var type = typeof(MyEnum?); //! nullable

		// Act
		var actual = converter.Deserialize(value, type);
			
		// Assert
		Assert.Null(actual);
		Assert.AreEqual(target, actual);
	}

	/// <summary> Checks that converting <b>null</b> or empty strings into an <see cref="Enum"/> will return the enumerations default value, if the target type is not nullable. </summary>
	[Test]
	[TestCase(null)]
	[TestCase("")]
	public void DeserializeEnumerationReturnsDefault(string? value)
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter();
		var target = MyEnum.Default;
		var type = typeof(MyEnum); //! not-nullable

		// Act
		var actual = converter.Deserialize(value, type);
			
		// Assert
		Assert.NotNull(actual);
		Assert.AreEqual(target, actual);
	}

	/// <summary> Checks that converting <b>null</b> or empty strings into an <see cref="Enum"/> will return the enumerations first value, if the target type is not nullable and the enum does not provide a default value. </summary>
	[Test]
	[TestCase(null)]
	[TestCase("")]
	public void DeserializeEnumerationThrowsBecauseNoDefault(string? value)
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter();
		var target = MyEnumWithoutDefault.First;
		var type = typeof(MyEnumWithoutDefault); //! not-nullable

		// Act + Assert
		var actual = converter.Deserialize(value, type);
			
		// Assert
		Assert.NotNull(actual);
		Assert.AreEqual(target, actual);
	}

	/// <summary> Checks that converting a string that is not defined within an <see cref="Enum"/> will throw. </summary>
	[Test]
	public void DeserializeEnumerationThrowsIfNotDefined()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter();
		var value = "not_defined";
		var target = MyEnum.Default;
		var type = typeof(MyEnum);

		// Act + Assert
		Assert.Catch<System.Text.Json.JsonException>(() => converter.Deserialize(value, type));
	}

	#endregion

	#region Serialize
	
	[Test]
	public void SerializeEnumeration()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter();
		var value = MyEnum.Entry1;
		var target = nameof(MyEnum.Entry1);

		// Act
		var actual = converter.Serialize(value);

		// Assert
		Assert.NotNull(actual);
		Assert.AreEqual(target, actual);
	}

	[Test]
	public void SerializeEnumerationWithWriteOut()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter(new EnumConverterOptions() {WriteOutOptions = WriteOutValues.AsSuffix()});
		var value = MyEnum.Entry1;
		var target = $"{nameof(MyEnum.Entry1)}{MyEnumSuffix}";

		// Act
		var actual = converter.Serialize(value);

		// Assert
		Assert.NotNull(actual);
		Assert.AreEqual(target, actual);
	}

	[Test]
	public void SerializeNullEnumeration()
	{
		// Arrange
		var converter = new EnumConverter.InternalEnumConverter();
		var value = (MyEnum?) null;
		var target = (string?) null;

		// Act
		var actual = converter.Serialize(value);

		// Assert
		Assert.Null(actual);
		Assert.AreEqual(target, actual);
	}

	[Test]
	public void SerializeAndDeserializeWithWriteOutAsSuffix()
	{
		// Arrange
		var serializer = new JsonSettingsSerializer(new EnumConverter(WriteOutValues.AsSuffix(start: "[", separator: ";", end: "]")));
		var settings = new MySettings() { First = MyEnum.Entry1, Second = MyEnum.Entry2 };

		// Act + Assert
		var settingsData = serializer.Serialize(settings);
		Assert.That(settingsData, Is.Not.Empty);
		
		// Assert + Assert
		var deserializedSettings = serializer.Deserialize<MySettings>(settingsData);
		Assert.NotNull(deserializedSettings);
	}

	[Test]
	public void SerializeAndDeserializeWithWriteOutAsComment()
	{
		// Arrange
		var serializer = new JsonSettingsSerializer(new EnumConverter(WriteOutValues.AsComment()));
		var settings = new MySettings() { First = MyEnum.Entry1, Second = MyEnum.Entry2 };

		// Act + Assert
		var settingsData = serializer.Serialize(settings);
		Assert.That(settingsData, Is.Not.Empty);
		
		// Assert + Assert
		var deserializedSettings = serializer.Deserialize<MySettings>(settingsData);
		Assert.NotNull(deserializedSettings);
	}

	[Test]
	public void SerializeAndDeserializeWithWriteOutAsProperty()
	{
		// Arrange
		var serializer = new JsonSettingsSerializer(new EnumConverter(WriteOutValues.AsProperty()));
		var settings = new MySettings() { First = MyEnum.Entry1, Second = MyEnum.Entry2 };

		// Act + Assert
		var settingsData = serializer.Serialize(settings);
		Assert.That(settingsData, Is.Not.Empty);
		
		// Assert + Assert
		var deserializedSettings = serializer.Deserialize<MySettings>(settingsData);
		Assert.NotNull(deserializedSettings);
	}

	#endregion

	#endregion
}