using System.Net;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Settings;
using Phoenix.Functionality.Settings.Encryption;

namespace Settings.Encryption.Test;

public class EncryptSettingsManagerTest
{
	#region Setup
	
#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[OneTimeSetUp]
	public void BeforeAllTests() { }

	[SetUp]
	public void Setup()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTest() { }
	
	#endregion

	#region Data

	class MultiplePropertiesSettings : ISettings
	{
		public string UnattributedString { get; set; }

		[Encrypt]
		public string PublicProperty { get; set; }

		[Encrypt]
		public object ObjectProperty { get; set; }

		[Encrypt]
		private string PrivateProperty { get; set; }

		[Encrypt]
		internal string InternalProperty { get; set; }
			
		[Encrypt]
		/// <summary> The properties type is not supported. </summary>
		public int InvalidProperty { get; set; }

		/// <summary> This property is not annotated and therefore irrelevant. </summary>
		public object IrrelevantProperty { get; set; }
	}

	class StringSettings : ISettings
	{
		[Encrypt]
		public string? Message { get; set; }
	}

	class NestedSettings : ISettings
	{
		internal class Inner
		{
			[Encrypt]
			public string? Message { get; set; }
		}

		public string BuildInString { get; set; }

		public DirectoryInfo BuildInDirectoryInfo { get; set; }
			
		public DateTime BuildInDateTime { get; set; }
			
		public IPAddress BuildInIPAddress { get; set; }

		internal Inner InnerInstance { get; set; } = new Inner();

		internal IReadOnlyCollection<Inner> InnerInstances { get; set; } = new Inner[] { new Inner(), new Inner() };
	}
		
	class DontFollowSettings : ISettings
	{
		internal class Inner
		{
			[Encrypt]
			public string? Message { get; set; }
		}

		[EncryptDoNotFollow]
		internal Inner InnerInstance { get; set; } = new Inner();

		[EncryptDoNotFollow]
		internal IReadOnlyCollection<Inner> IgnoredProperty { get; set; } = new Inner[] { new Inner(), new Inner() };
	}
		
	class IntegerSettings : ISettings
	{
		[Encrypt]
		public int Number { get; set; }
	}

	class ThrowingSettings : ISettings
	{

		internal Int64 Error => throw new Exception();
	}

	class SimpleArraySettings : ISettings
	{
		[Encrypt]
		internal string[] Messages { get; set; } = new string[] { "1", "2" };
	}

	class SimpleListSettings : ISettings
	{
		[Encrypt]
		internal List<string> Messages { get; set; } = new List<string>() { "1", "2" };
	}

	class StackedListSettings : ISettings
	{
		[Encrypt]
		internal List<List<string>> StackedMessages { get; set; } = new List<List<string>>()
		{
			new List<string>(){"1.1", "1.2", "1.3"},
			new List<string>(){"3.1", "2.2", "2.3"},
		};
	}

	class NestedListSettings : ISettings
	{
		internal class EncryptedCollection : List<string>
		{
			public EncryptedCollection() : base(new List<string>() { "1", "2", "3" }) { }
		}

		[Encrypt]
		internal EncryptedCollection NestedMessages { get; set; } = new EncryptedCollection();
	}

	#endregion

	#region Tests
	
	#region Property Detection

	[Test]
	[TestCase(typeof(string), false)]
	[TestCase(typeof(Dictionary<object, object>), false)]
	[TestCase(typeof(List<object>), false)]
	[TestCase(typeof(object[]), false)]
	[TestCase(typeof(IPAddress), false)]
	[TestCase(typeof(NestedSettings.Inner), true)]
	public void Check_Is_Treated_As_Nested(Type nestedType, bool shouldBeTreatedAsCollection)
	{
		// Act
		var isNested = EncryptSettingsManager.IsNested(nestedType);

		// Assert
		Assert.AreEqual(isNested, shouldBeTreatedAsCollection);
	}

	[Test]
	[TestCase(typeof(string), false)]
	[TestCase(typeof(Dictionary<object, object>), false)]
	[TestCase(typeof(List<object>), true)]
	[TestCase(typeof(object[]), true)]
	public void Check_Is_Treated_As_Collection(Type collectionType, bool shouldBeTreatedAsCollection)
	{
		// Act
		var isCollection = EncryptSettingsManager.IsCollection(collectionType);

		// Assert
		Assert.AreEqual(isCollection, shouldBeTreatedAsCollection);
	}

	[Test]
	public void Check_System_Namespace_Is_Ignored()
	{
		Assert.Ignore("This would need a custom assembly name 'System.[...].dll' which is currently not implemented.");
		//// Arrange
		//var settings = new CantFollowSettings();

		//// Act
		//var properties = EncryptSettingsManager.GetRelevantProperties(settings, System.Console.WriteLine).ToArray();

		//// Assert
		//Assert.IsEmpty(properties);
	}

	[Test]
	public void Check_EncryptDoNotFollowAttribute_Overrules()
	{
		// Arrange
		var settings = new DontFollowSettings();

		// Act
		var properties = EncryptSettingsManager.GetRelevantProperties(settings, System.Console.WriteLine).ToArray();

		// Assert
		Assert.IsEmpty(properties);
	}

	[Test]
	public void Check_EncryptForceFollowAttribute_Overrules()
	{
		Assert.Ignore("This would need a custom assembly name 'System.[...].dll' which is currently not implemented.");
		//// Arrange
		//var settings = new ForceFollowSettings();

		//// Act
		//var properties = EncryptSettingsManager.GetRelevantProperties(settings, System.Console.WriteLine).ToArray();

		//// Assert
		//Assert.That(properties, Has.Length.EqualTo(1));
		//Assert.IsNotNull(properties.FirstOrDefault(info => info.Name == nameof(System.Test.NestedClassFromSystemNamespace.Message)).Name); // '.Name' → Access some property, because the value tuple itself will always be available.
	}

	[Test]
	public void Check_Relevant_Properties_Are_Filtered()
	{
		// Arrange
		var settings = new MultiplePropertiesSettings();

		// Act
		var properties = EncryptSettingsManager.GetRelevantProperties(settings, System.Console.WriteLine).ToArray();

		// Assert
		Assert.That(properties, Has.Length.GreaterThanOrEqualTo(1));
		Assert.IsNotNull(properties.FirstOrDefault(info => info.Name == nameof(MultiplePropertiesSettings.PublicProperty)).Name); // '.Name' → Access some property, because the value tuple itself will always be available.
		Assert.IsNotNull(properties.FirstOrDefault(info => info.Name == nameof(MultiplePropertiesSettings.ObjectProperty)).Name);
		Assert.IsNotNull(properties.FirstOrDefault(info => info.Name == "PrivateProperty").Name);
		Assert.IsNotNull(properties.FirstOrDefault(info => info.Name == nameof(MultiplePropertiesSettings.InternalProperty)).Name);
		Assert.IsNull(properties.FirstOrDefault(info => info.Name == nameof(MultiplePropertiesSettings.InvalidProperty)).Name);
		Assert.IsNull(properties.FirstOrDefault(info => info.Name == nameof(MultiplePropertiesSettings.IrrelevantProperty)).Name);
		Assert.IsNull(properties.FirstOrDefault(info => info.Name == nameof(MultiplePropertiesSettings.UnattributedString)).Name);
	}

	[Test]
	public void Check_Nested_Properties_Are_Filtered()
	{
		// Arrange
		var settings = new NestedSettings();

		// Act
		var properties = EncryptSettingsManager.GetRelevantProperties(settings, System.Console.WriteLine).ToArray();

		// Assert
		Assert.That(properties, Has.Length.EqualTo(3));
		Assert.IsNotNull(properties.All(info => info.Name == nameof(NestedSettings.InnerInstance.Message)));
	}

	[Test]
	public void Check_Settings_With_Properties_That_Throw_Can_Be_Handled()
	{
		// Arrange
		var settings = new ThrowingSettings();

		// Act + Assert
		Assert.DoesNotThrow(() => EncryptSettingsManager.GetRelevantProperties(settings, System.Console.WriteLine).ToArray());
	}

	[Test]
	[Category("Encrypted Collection Test")]
	public void Check_Simple_Array_Handling() => this.CheckCollectionHandling<SimpleArraySettings>(2);

	[Test]
	[Category("Encrypted Collection Test")]
	public void Check_Simple_List_Handling() => this.CheckCollectionHandling<SimpleListSettings>(2);

	[Test]
	[Category("Encrypted Collection Test")]
	public void Check_Stacked_List_Handling() => this.CheckCollectionHandling<StackedListSettings>(6);

	[Test]
	[Category("Encrypted Collection Test")]
	public void Check_Nested_List_Handling() => this.CheckCollectionHandling<NestedListSettings>(3);

	private void CheckCollectionHandling<TSettings>(int amountOfProperties) where TSettings : new()
	{
		// Arrange
		var settings = new TSettings();

		// Act
		var properties = EncryptSettingsManager.GetRelevantProperties(settings, System.Console.WriteLine).ToArray();

		// Assert
		Assert.That(properties, Has.Length.GreaterThanOrEqualTo(amountOfProperties));
	}

	#endregion

	#region En-/Decryption
		
	[Test]
	public void Check_Unencrypted_Property_Is_Same()
	{
		// Arrange
		var underlyingSettingsManager = _fixture.Create<Mock<ISettingsManager>>().Object;
		Mock.Get(underlyingSettingsManager)
			.Setup(mock => mock.Load<StringSettings>(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns
			(
				new StringSettings()
				{
					Message = "unencrypted"
				}
			);
		var settingsManager = new EncryptSettingsManager(underlyingSettingsManager);
			
		// Act
		var settings = settingsManager.Load<StringSettings>();

		// Assert
		Assert.That(settings.Message, Is.EqualTo("unencrypted"));
	}

	[Test]
	public void Check_Null_Property_Is_Same()
	{
		// Arrange
		var underlyingSettingsManager = _fixture.Create<Mock<ISettingsManager>>().Object;
		Mock.Get(underlyingSettingsManager)
			.Setup(mock => mock.Load<StringSettings>(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns
			(
				new StringSettings()
				{
					Message = null
				}
			);
		var settingsManager = new EncryptSettingsManager(underlyingSettingsManager);
			
		// Act
		var settings = settingsManager.Load<StringSettings>();

		// Assert
		Assert.IsNull(settings.Message);
	}

	[Test]
	public void Check_Null_Is_Not_Encrypted()
	{
		// Arrange
		string? unencrypted = null;
		string? encryptedText = null;
		var settings = new StringSettings() { Message = unencrypted };
		var underlyingSettingsManager = _fixture.Create<Mock<ISettingsManager>>().Object;
		Mock.Get(underlyingSettingsManager)
			.Setup(mock => mock.Save(settings, It.IsAny<bool>()))
			.Callback<StringSettings, bool>
			(
				(s, _) =>
				{
					encryptedText = s.Message;
				}
			)
			;
					
		var settingsManager = new EncryptSettingsManager(underlyingSettingsManager);
			
		// Act
		settingsManager.Save(settings);

		// Assert
		Mock.Get(underlyingSettingsManager).Verify(mock => mock.Save(settings, It.IsAny<bool>()), Times.Once());
		Assert.IsNull(encryptedText);
	}

	[Test]
	public void Check_Value_Is_Encrypted_Upon_Save()
	{
		// Arrange
		string? unencrypted = "unencrypted";
		string? encrypted = null;
		var settings = new StringSettings() { Message = unencrypted };
		var underlyingSettingsManager = _fixture.Create<Mock<ISettingsManager>>().Object;
		Mock.Get(underlyingSettingsManager)
			.Setup(mock => mock.Save(settings, It.IsAny<bool>()))
			.Callback<StringSettings, bool>
			(
				(s, _) =>
				{
					encrypted = s.Message;
				}
			)
			;
					
		var settingsManager = new EncryptSettingsManager(underlyingSettingsManager);
			
		// Act
		settingsManager.Save(settings);

		// Assert
		Mock.Get(underlyingSettingsManager).Verify(mock => mock.Save(settings, It.IsAny<bool>()), Times.Once());
		Assert.Multiple
		(
			() =>
			{
				Assert.IsNotNull(encrypted);
				Assert.That(encrypted, Is.Not.EqualTo(unencrypted));
			}
		);
	}

	[Test]
	public void Check_Value_Is_Still_Decrypted_After_Save()
	{
		// Arrange
		string? unencrypted = "unencrypted";
		string? encryptedText = null;
		var settings = new StringSettings() { Message = unencrypted };
		var underlyingSettingsManager = _fixture.Create<Mock<ISettingsManager>>().Object;
		Mock.Get(underlyingSettingsManager)
			.Setup(mock => mock.Save(settings, It.IsAny<bool>()))
			.Callback<StringSettings, bool>
			(
				(s, _) =>
				{
					encryptedText = s.Message;
				}
			)
			;

		var settingsManager = new EncryptSettingsManager(underlyingSettingsManager);

		// Act
		settingsManager.Save(settings);

		// Assert
		Mock.Get(underlyingSettingsManager).Verify(mock => mock.Save(settings, It.IsAny<bool>()), Times.Once());
		Assert.That(settings.Message, Is.EqualTo(unencrypted));
	}

	[Test]
	public void Check_Nested_Property_Is_Decrypted()
	{
		// Arrange
		var unencrypted = "unencrypted";
		var encryptedText = "3DX19APzlJKi5kK0YlUjp1ZDNeQF9fDeN7bfsddBDKhX7w+jhw==";
		var settings = new NestedSettings() { InnerInstance = new NestedSettings.Inner() { Message = encryptedText } };
		var underlyingSettingsManager = _fixture.Create<Mock<ISettingsManager>>().Object;
		var settingsManager = new EncryptSettingsManager(underlyingSettingsManager);

		// Act
		settingsManager.DecryptProperties(settings);

		// Assert
		Assert.That(settings.InnerInstance.Message, Is.EqualTo(unencrypted));
	}

	[Test]
	public void Check_Nested_Property_Is_Encrypted()
	{
		// Arrange
		var unencrypted = "unencrypted";
		var settings = new NestedSettings() { InnerInstance = new NestedSettings.Inner() { Message = unencrypted } };
		var underlyingSettingsManager = _fixture.Create<Mock<ISettingsManager>>().Object;
		var settingsManager = new EncryptSettingsManager(underlyingSettingsManager);

		// Act
		settingsManager.EncryptProperties(settings);

		// Assert
		Assert.That(settings.InnerInstance.Message, Is.Not.EqualTo(unencrypted));
	}

	[Test]
	public void Check_Simple_Array_Property_Is_Encrypted()
	{
		// Arrange
		var unencrypted = "unencrypted";
		var settings = new SimpleArraySettings() { Messages = new [] { unencrypted, unencrypted } };
		var underlyingSettingsManager = _fixture.Create<Mock<ISettingsManager>>().Object;
		var settingsManager = new EncryptSettingsManager(underlyingSettingsManager);

		// Act
		settingsManager.EncryptProperties(settings);

		// Assert
		Assert.That(settings.Messages.First(), Is.Not.EqualTo(unencrypted));
		Assert.That(settings.Messages.Last(), Is.Not.EqualTo(unencrypted));
	}

	[Test]
	public void Check_Simple_List_Property_Is_Encrypted()
	{
		// Arrange
		var unencrypted = "unencrypted";
		var settings = new SimpleListSettings() { Messages = new List<string>() { unencrypted, unencrypted } };
		var underlyingSettingsManager = _fixture.Create<Mock<ISettingsManager>>().Object;
		var settingsManager = new EncryptSettingsManager(underlyingSettingsManager);

		// Act
		settingsManager.EncryptProperties(settings);

		// Assert
		Assert.That(settings.Messages.First(), Is.Not.EqualTo(unencrypted));
		Assert.That(settings.Messages.Last(), Is.Not.EqualTo(unencrypted));
	}

	[Test]
	public void Check_Stacked_List_Property_Is_Encrypted()
	{
		// Arrange
		var unencrypted = "unencrypted";
		var settings = new StackedListSettings()
		{
			StackedMessages = new List<List<string>>()
			{
				new List<string>() { unencrypted, unencrypted },
				new List<string>() { unencrypted, unencrypted },
			}
		};
		var underlyingSettingsManager = _fixture.Create<Mock<ISettingsManager>>().Object;
		var settingsManager = new EncryptSettingsManager(underlyingSettingsManager);

		// Act
		settingsManager.EncryptProperties(settings);

		// Assert
		Assert.That(settings.StackedMessages.First().First(), Is.Not.EqualTo(unencrypted));
		Assert.That(settings.StackedMessages.First().Last(), Is.Not.EqualTo(unencrypted));
		Assert.That(settings.StackedMessages.Last().First(), Is.Not.EqualTo(unencrypted));
		Assert.That(settings.StackedMessages.Last().Last(), Is.Not.EqualTo(unencrypted));
	}

	[Test]
	public void Check_True_Is_Returned_If_Not_All_Properties_Are_Encrypted()
	{
		// Arrange
		var unencrypted = "unencrypted";
		var settings = new StringSettings() { Message = unencrypted };
		var underlyingSettingsManager = _fixture.Create<Mock<ISettingsManager>>().Object;
		var settingsManager = new EncryptSettingsManager(underlyingSettingsManager);

		// Act
		var needsEncryption = settingsManager.DecryptProperties(settings);

		// Assert
		Assert.True(needsEncryption);
	}

	[Test]
	public void Check_False_Is_Returned_If_All_Properties_Are_Encrypted()
	{
		// Arrange
		var encryptedText = "3DX19APzlJKi5kK0YlUjp1ZDNeQF9fDeN7bfsddBDKhX7w+jhw==";
		var settings = new StringSettings() { Message = encryptedText };
		var underlyingSettingsManager = _fixture.Create<Mock<ISettingsManager>>().Object;
		var settingsManager = new EncryptSettingsManager(underlyingSettingsManager);

		// Act
		var needsEncryption = settingsManager.DecryptProperties(settings);

		// Assert
		Assert.False(needsEncryption);
	}

	#endregion

	#endregion
}