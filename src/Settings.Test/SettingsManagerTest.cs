using System.Dynamic;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Settings;
using Phoenix.Functionality.Settings.Cache;

namespace Settings.Test;

public class SettingsManagerTest
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
		SettingsExtensions.Cache.Clear();
	}

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTest() { }

	#endregion

	#region Data

	public class Settings : ISettings { }
	
	public class LoadSettings : ISettings, ISettingsLoadedNotification
	{
		public bool Raised { get; private set; }

		/// <inheritdoc />
		public void Loaded()
		{
			this.Raised = true;
		}
	}

	public class ChangeSettings : ISettings, ISettingsLayoutChangedNotification
	{
		public bool Raised { get; private set; }

		/// <inheritdoc />
		public void LayoutChanged(ExpandoObject rawData)
		{
			this.Raised = true;
		}
	}

	#endregion

	#region Tests
	
	#region Load

	[Test]
	public void LoadReturnsFromCache()
	{
		// Arrange
		var existingSettings = new Settings();
		var cache = new SettingsCache();
		cache.AddOrUpdate(existingSettings);
		_fixture.Inject<ISettingsCache>(cache);
		var sink = _fixture.Create<Mock<ISettingsSink<string>>>().Object;
		Mock.Get(sink)
			.Setup(mock => mock.Retrieve<Settings>())
			.Verifiable()
			;
		_fixture.Inject(sink);
		var serializer = _fixture.Create<Mock<ISettingsSerializer<string>>>().Object;
		Mock.Get(serializer)
			.Setup(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny))
			.Verifiable()
			;
		_fixture.Inject(serializer);
		var manager = _fixture.Create<Mock<SettingsManager<string>>>().Object;
		Mock.Get(manager)
			.Setup(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()))
			.Verifiable()
			;
		Mock.Get(manager)
			.Setup(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()))
			.Verifiable()
			;

		// Act
		var settings = manager.Load<Settings>();

		// Assert
		Assert.That(settings, Is.Not.Null);
		Mock.Get(sink).Verify(mock => mock.Retrieve<Settings>(), Times.Never);
		Mock.Get(manager).Verify(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()), Times.Never);
		Mock.Get(manager).Verify(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()), Times.Never);
		Mock.Get(serializer).Verify(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Never);
	}

	[Test]
	public void LoadSavesToCache()
	{
		// Arrange
		var cache = _fixture.Create<Mock<ISettingsCache>>().Object;
		Mock.Get(cache)
			.Setup(mock => mock.AddOrUpdate(It.IsAny<Settings>()))
			.Verifiable()
			;
		_fixture.Inject(cache);
		var manager = _fixture.Create<Mock<SettingsManager<string>>>().Object;

		// Act
		var settings = manager.Load<Settings>();

		// Assert
		Assert.That(settings, Is.Not.Null);
		Mock.Get(cache).Verify(mock => mock.AddOrUpdate(It.IsAny<Settings>()), Times.Once);
	}

	/// <summary>
	/// Checks that the <see cref="SettingsManager{TSettingsData}"/> creates a new settings instance if no data is available.
	/// </summary>
	[Test]
	public void LoadCreatesAndSavesNewInstance()
	{
		// Arrange
		var sink = _fixture.Create<Mock<ISettingsSink<string>>>().Object;
		Mock.Get(sink)
			.Setup(mock => mock.Retrieve<Settings>())
			.Returns((string) null) //! This needs to return null so that the initial instance is created and saved.
			.Verifiable()
			;
		_fixture.Inject(sink);
		var serializer = _fixture.Create<Mock<ISettingsSerializer<string>>>().Object;
		Mock.Get(serializer)
			.Setup(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny))
			.Verifiable()
			;
		_fixture.Inject(serializer);
		var manager = _fixture.Create<Mock<SettingsManager<string>>>().Object;
		Mock.Get(manager)
			.Setup(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()))
			.Verifiable()
			;
		Mock.Get(manager)
			.Setup(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()))
			.Verifiable()
			;

		// Act
		var settings = manager.Load<Settings>(preventCreation: false);

		// Assert
		Assert.That(settings, Is.Not.Null);
		Mock.Get(sink).Verify(mock => mock.Retrieve<Settings>(), Times.Once);
		Mock.Get(manager).Verify(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()), Times.Once);
		Mock.Get(manager).Verify(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()), Times.Once);
		Mock.Get(serializer).Verify(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Never);
	}

	/// <summary>
	/// Checks that the <see cref="SettingsManager{TSettingsData}"/> does not create a new settings instance if no data is available and rather throws an <see cref="SettingsLoadException"/>.
	/// </summary>
	[Test]
	public void LoadDoesNotCreateAndSaveNewInstance()
	{
		// Arrange
		var sink = _fixture.Create<Mock<ISettingsSink<string>>>().Object;
		Mock.Get(sink)
			.Setup(mock => mock.Retrieve<Settings>())
			.Returns((string) null) //! This needs to return null so that the initial instance is created and saved.
			.Verifiable()
			;
		_fixture.Inject(sink);
		var serializer = _fixture.Create<Mock<ISettingsSerializer<string>>>().Object;
		Mock.Get(serializer)
			.Setup(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny))
			.Verifiable()
			;
		_fixture.Inject(serializer);
		var manager = _fixture.Create<Mock<SettingsManager<string>>>().Object;
		Mock.Get(manager)
			.Setup(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()))
			.Verifiable()
			;
		Mock.Get(manager)
			.Setup(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()))
			.Verifiable()
			;

		// Act + Assert
		Assert.Catch<SettingsLoadException>(() => manager.Load<Settings>(preventCreation: true));
		Mock.Get(sink).Verify(mock => mock.Retrieve<Settings>(), Times.Once);
		Mock.Get(manager).Verify(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()), Times.Never);
		Mock.Get(manager).Verify(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()), Times.Never);
		Mock.Get(serializer).Verify(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Never);
	}

	[Test]
	[Ignore("The manger no longer creates a default instance if deserialization fails. Instead it throws.")]
	public void LoadCreatesAndSavesNewInstanceIfDeserializationFails()
	{
		// Arrange
		var sink = _fixture.Create<Mock<ISettingsSink<string>>>().Object;
		Mock.Get(sink)
			.Setup(mock => mock.Retrieve<Settings>())
			.Returns(String.Empty) //! This needs to return at least a string instance.
			.Verifiable()
			;
		_fixture.Inject(sink);
		var serializer = _fixture.Create<Mock<ISettingsSerializer<string>>>().Object;
		Mock.Get(serializer)
			.Setup(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny))
			.Returns
			(
				(string _, out ExpandoObject? rawData) =>
				{
					rawData = null;
					return (Settings) null; //! This must return null in order for the manager to create a new instance.
				}
			)
			.Verifiable()
			;
		_fixture.Inject(serializer);
		var manager = _fixture.Create<Mock<SettingsManager<string>>>().Object;
		Mock.Get(manager)
			.Setup(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()))
			.Verifiable()
			;
		Mock.Get(manager)
			.Setup(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()))
			.Verifiable()
			;

		// Act
		var settings = manager.Load<Settings>();

		// Assert
		Assert.That(settings, Is.Not.Null);
		Mock.Get(sink).Verify(mock => mock.Retrieve<Settings>(), Times.Once);
		Mock.Get(manager).Verify(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()), Times.Once);
		Mock.Get(manager).Verify(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()), Times.Once);
		Mock.Get(serializer).Verify(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Once);
	}

	[Test]
	public void LoadThrowsIfDeserializationFails()
	{
		// Arrange
		var sink = _fixture.Create<Mock<ISettingsSink<string>>>().Object;
		Mock.Get(sink)
			.Setup(mock => mock.Retrieve<Settings>())
			.Returns(String.Empty) //! This needs to return at least a string instance.
			.Verifiable()
			;
		_fixture.Inject(sink);
		var serializer = _fixture.Create<Mock<ISettingsSerializer<string>>>().Object;
		Mock.Get(serializer)
			.Setup(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny))
			.Throws(_fixture.Create<SettingsLoadException>()) //! This needs to throw.
			.Verifiable()
			;
		_fixture.Inject(serializer);
		var manager = _fixture.Create<Mock<SettingsManager<string>>>().Object;
		Mock.Get(manager)
			.Setup(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()))
			.Verifiable()
			;
		Mock.Get(manager)
			.Setup(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()))
			.Verifiable()
			;

		// Act + Assert
		Assert.Catch<SettingsLoadException>(() => manager.Load<Settings>());

		// Assert
		Mock.Get(sink).Verify(mock => mock.Retrieve<Settings>(), Times.Once);
		Mock.Get(serializer).Verify(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Once);
		Mock.Get(manager).Verify(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()), Times.Never);
		Mock.Get(manager).Verify(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()), Times.Never);
	}

	/// <summary>
	/// Checks that loading unavailable settings with the <b>preventCreation</b> parameter set to <b>true</b> throws the <see cref="SettingsUnavailableException"/>.
	/// </summary>
	[Test]
	public void LoadThrowsIfSettingsDataIsUnavailable()
	{
		// Arrange
		var sink = _fixture.Create<Mock<ISettingsSink<string>>>().Object;
		Mock.Get(sink)
			.Setup(mock => mock.Retrieve<Settings>())
			.Returns((string?) null) //! This needs to return null to simulate unavailable settings.
			.Verifiable()
			;
		_fixture.Inject(sink);
		var serializer = _fixture.Create<Mock<ISettingsSerializer<string>>>().Object;
		_fixture.Inject(serializer);
		var manager = _fixture.Create<Mock<SettingsManager<string>>>().Object;
		
		// Act + Assert
		Assert.Catch<SettingsUnavailableException>(() => manager.Load<Settings>(preventCreation: true));
	}

	[Test]
	public void LoadUsesExistingData()
	{
		// Arrange
		var sink = _fixture.Create<Mock<ISettingsSink<string>>>().Object;
		Mock.Get(sink)
			.Setup(mock => mock.Retrieve<Settings>())
			.Returns(String.Empty) //! This needs to return at least a string instance.
			.Verifiable()
			;
		_fixture.Inject(sink);
		var serializer = _fixture.Create<Mock<ISettingsSerializer<string>>>().Object;
		Mock.Get(serializer)
			.Setup(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny))
			.Returns
			(
				(string _, out ExpandoObject? rawData) =>
				{
					rawData = null;
					return new Settings();
				}
			)
			.Verifiable()
			;
		Mock.Get(serializer)
			.Setup(mock => mock.AreIdentical(It.IsAny<ISettings>(), It.IsAny<string>()))
			.Returns(true) //! Must return true, so that the settings are not saved, because they differ from their data.
			.Verifiable()
			;
		_fixture.Inject(serializer);
		var manager = _fixture.Create<Mock<SettingsManager<string>>>().Object;
		Mock.Get(manager)
			.Setup(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()))
			.Verifiable()
			;
		Mock.Get(manager)
			.Setup(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()))
			.Verifiable()
			;

		// Act
		var settings = manager.Load<Settings>();

		// Assert
		Assert.That(settings, Is.Not.Null);
		Mock.Get(sink).Verify(mock => mock.Retrieve<Settings>(), Times.Once);
		Mock.Get(manager).Verify(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()), Times.Never);
		Mock.Get(manager).Verify(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()), Times.Never);
		Mock.Get(serializer).Verify(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Once);
		Mock.Get(serializer).Verify(mock => mock.AreIdentical(It.IsAny<ISettings>(), It.IsAny<string>()), Times.Once);
	}

	[Test]
	public void LoadUsesExistingDataAndReSavesSettings()
	{
		// Arrange
		var sink = _fixture.Create<Mock<ISettingsSink<string>>>().Object;
		Mock.Get(sink)
			.Setup(mock => mock.Retrieve<Settings>())
			.Returns(String.Empty) //! This needs to return at least a string instance.
			.Verifiable()
			;
		_fixture.Inject(sink);
		var serializer = _fixture.Create<Mock<ISettingsSerializer<string>>>().Object;
		Mock.Get(serializer)
			.Setup(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny))
			.Returns
			(
				(string _, out ExpandoObject? rawData) =>
				{
					rawData = null;
					return new Settings();
				}
			)
			.Verifiable()
			;
		Mock.Get(serializer)
			.Setup(mock => mock.AreIdentical(It.IsAny<ISettings>(), It.IsAny<string>()))
			.Returns(false) //! Must return false, so that the settings are re-saved.
			.Verifiable()
			;
		_fixture.Inject(serializer);
		var manager = _fixture.Create<Mock<SettingsManager<string>>>().Object;
		Mock.Get(manager)
			.Setup(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()))
			.Verifiable()
			;
		Mock.Get(manager)
			.Setup(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()))
			.Verifiable()
			;

		// Act
		var settings = manager.Load<Settings>();

		// Assert
		Assert.That(settings, Is.Not.Null);
		Mock.Get(sink).Verify(mock => mock.Retrieve<Settings>(), Times.Once);
		Mock.Get(manager).Verify(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()), Times.Never);
		Mock.Get(manager).Verify(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()), Times.Once);
		Mock.Get(serializer).Verify(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Once);
		Mock.Get(serializer).Verify(mock => mock.AreIdentical(It.IsAny<ISettings>(), It.IsAny<string>()), Times.Once);
	}

	[Test]
	public void LoadTriggersLayoutChange()
	{
		// Arrange
		var sinkMock = _fixture.Create<Mock<ISettingsSink<string>>>();
		sinkMock
			.Setup(mock => mock.Retrieve<ChangeSettings>())
			.Returns(String.Empty)
			.Verifiable()
			;
		var serializerMock = _fixture.Create<Mock<ISettingsSerializer<string>>>();
		serializerMock
			.Setup(mock => mock.Deserialize<ChangeSettings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny))
			.Returns
			(
				(string _, out ExpandoObject? rawData) =>
				{
					rawData = new ExpandoObject();
					return new ChangeSettings();
				}
			)
			.Verifiable()
			;
		var manager = new SettingsManager<string>(sinkMock.Object, serializerMock.Object);

		// Act
		var settings = manager.Load<ChangeSettings>();

		// Assert
		Assert.That(settings.Raised, Is.True);
		sinkMock.Verify(mock => mock.Retrieve<ChangeSettings>(), Times.Once);
		serializerMock.Verify(mock => mock.Deserialize<ChangeSettings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Once);
	}

	[Test]
	public void LoadTriggersLoadEvent()
	{
		// Arrange
		var sinkMock = _fixture.Create<Mock<ISettingsSink<string>>>();
		sinkMock
			.Setup(mock => mock.Retrieve<LoadSettings>())
			.Returns(String.Empty)
			.Verifiable()
			;
		var serializerMock = _fixture.Create<Mock<ISettingsSerializer<string>>>();
		serializerMock
			.Setup(mock => mock.Deserialize<LoadSettings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny))
			.Returns
			(
				(string _, out ExpandoObject? rawData) =>
				{
					rawData = new ExpandoObject();
					return new LoadSettings();
				}
			)
			.Verifiable()
			;
		var manager = new SettingsManager<string>(sinkMock.Object, serializerMock.Object);

		// Act
		var settings = manager.Load<LoadSettings>();

		// Assert
		Assert.That(settings.Raised, Is.True);
		sinkMock.Verify(mock => mock.Retrieve<LoadSettings>(), Times.Once);
		serializerMock.Verify(mock => mock.Deserialize<LoadSettings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Once);
	}

	[Test]
	public void CacheDoesNotTriggerLoadEvent()
	{
		// Arrange
		var sinkMock = _fixture.Create<Mock<ISettingsSink<string>>>();
		var serializerMock = _fixture.Create<Mock<ISettingsSerializer<string>>>();
		var cacheMock = _fixture.Create<Mock<ISettingsCache>>();
		cacheMock
			.Setup(mock => mock.TryGet(out It.Ref<LoadSettings?>.IsAny))
			.Returns
			(
				(out LoadSettings? settings) =>
				{
					settings = new LoadSettings();
					return true;
				}
			)
			.Verifiable()
			;

		var manager = new SettingsManager<string>(sinkMock.Object, serializerMock.Object, cacheMock.Object);

		// Act
		var settings = manager.Load<LoadSettings>();

		// Assert
		Assert.That(settings.Raised, Is.False);
		sinkMock.Verify(mock => mock.Retrieve<LoadSettings>(), Times.Never);
		serializerMock.Verify(mock => mock.Deserialize<LoadSettings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Never);
		cacheMock.Verify(mock => mock.TryGet(out It.Ref<LoadSettings?>.IsAny), Times.Once);
	}

	/// <summary> Checks that the <see cref="SettingsManager{TSettingsData}"/> automatically adds loaded <see cref="ISettings"/> to the <see cref="SettingsExtensions.Cache"/>. </summary>
	[Test]
	public void LoadAddsInstanceToSettingsExtensionsCache()
	{
		// Arrange
		var manager = _fixture.Create<Mock<SettingsManager<string>>>().Object;

		// Act
		manager.Load<Settings>();

		// Assert
		Assert.That(SettingsExtensions.Cache, Has.Count.EqualTo(1));
	}

	#endregion

	#region Delete

	[Test]
	public void DeleteCallsSink()
	{
		// Arrange
		var sink = _fixture.Create<Mock<ISettingsSink<string>>>().Object;
		Mock.Get(sink).Setup(mock => mock.Purge<Settings>(It.IsAny<bool>())).Verifiable();
		_fixture.Inject(sink);
		var manager = _fixture.Create<Mock<SettingsManager<string>>>().Object;

		// Act
		manager.Delete<Settings>(false);

		// Assert
		Mock.Get(sink).Verify(mock => mock.Purge<Settings>(It.IsAny<bool>()), Times.Once);
	}

	[Test]
	public void DeleteRemovesCachedInstance()
	{
		// Arrange
		var settings = new Settings();
		var internalCache = new List<Settings>() {settings};
		var cache = _fixture.Create<Mock<ISettingsCache>>().Object;
		Mock.Get(cache)
			.Setup(mock => mock.TryRemove(out It.Ref<Settings?>.IsAny))
			.Callback
			(
				() =>
				{
					internalCache.Remove(settings);
				}
			)
			;
		_fixture.Inject(cache);
		var manager = _fixture.Create<Mock<SettingsManager<string>>>().Object;

		// Act
		manager.Delete<Settings>(false);

		// Assert
		Assert.That(internalCache, Is.Empty);
	}

	#endregion

	#endregion
}