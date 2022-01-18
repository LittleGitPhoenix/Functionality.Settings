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

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
		SettingsExtensions.Cache.Clear();
	}

	#endregion

	#region Data

	public class Settings : ISettings { }

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

	#region Load

	[Test]
	public void Check_Load_Returns_From_Cache()
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
		Assert.NotNull(settings);
		Mock.Get(sink).Verify(mock => mock.Retrieve<Settings>(), Times.Never);
		Mock.Get(manager).Verify(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()), Times.Never);
		Mock.Get(manager).Verify(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()), Times.Never);
		Mock.Get(serializer).Verify(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Never);
	}

	[Test]
	public void Check_Load_Saves_To_Cache()
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
		Assert.NotNull(settings);
		Mock.Get(cache).Verify(mock => mock.AddOrUpdate(It.IsAny<Settings>()), Times.Once);
	}

	/// <summary>
	/// Checks that the <see cref="SettingsManager{TSettingsData}"/> creates a new settings instance if no data is available.
	/// </summary>
	[Test]
	public void Check_Load_Creates_And_Saves_New_Instance()
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
		Assert.NotNull(settings);
		Mock.Get(sink).Verify(mock => mock.Retrieve<Settings>(), Times.Once);
		Mock.Get(manager).Verify(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()), Times.Once);
		Mock.Get(manager).Verify(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()), Times.Once);
		Mock.Get(serializer).Verify(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Never);
	}

	/// <summary>
	/// Checks that the <see cref="SettingsManager{TSettingsData}"/> does not create a new settings instance if no data is available and rather throws an <see cref="SettingsLoadException"/>.
	/// </summary>
	[Test]
	public void Check_Load_Does_Not_Create_And_Save_New_Instance()
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
	public void Check_Load_Creates_And_Saves_New_Instance_If_Deserialization_Fails()
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
		Assert.NotNull(settings);
		Mock.Get(sink).Verify(mock => mock.Retrieve<Settings>(), Times.Once);
		Mock.Get(manager).Verify(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()), Times.Once);
		Mock.Get(manager).Verify(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()), Times.Once);
		Mock.Get(serializer).Verify(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Once);
	}

	[Test]
	public void Check_Load_Throws_If_Deserialization_Fails()
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

	[Test]
	public void Check_Load_Uses_Existing_Data()
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
		Assert.NotNull(settings);
		Mock.Get(sink).Verify(mock => mock.Retrieve<Settings>(), Times.Once);
		Mock.Get(manager).Verify(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()), Times.Never);
		Mock.Get(manager).Verify(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()), Times.Never);
		Mock.Get(serializer).Verify(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Once);
		Mock.Get(serializer).Verify(mock => mock.AreIdentical(It.IsAny<ISettings>(), It.IsAny<string>()), Times.Once);
	}

	[Test]
	public void Check_Load_Uses_Existing_Data_And_ReSaves_Settings()
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
		Assert.NotNull(settings);
		Mock.Get(sink).Verify(mock => mock.Retrieve<Settings>(), Times.Once);
		Mock.Get(manager).Verify(mock => mock.GetAndSaveDefaultInstance<Settings>(It.IsAny<bool>()), Times.Never);
		Mock.Get(manager).Verify(mock => mock.Save(It.IsAny<Settings>(), It.IsAny<bool>()), Times.Once);
		Mock.Get(serializer).Verify(mock => mock.Deserialize<Settings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Once);
		Mock.Get(serializer).Verify(mock => mock.AreIdentical(It.IsAny<ISettings>(), It.IsAny<string>()), Times.Once);
	}

	[Test]
	public void Check_Load_Triggers_Layout_Change()
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
		Assert.True(settings.Raised);
		sinkMock.Verify(mock => mock.Retrieve<ChangeSettings>(), Times.Once);
		serializerMock.Verify(mock => mock.Deserialize<ChangeSettings>(It.IsAny<string>(), out It.Ref<ExpandoObject?>.IsAny), Times.Once);
	}

	/// <summary> Checks that the <see cref="SettingsManager{TSettingsData}"/> automatically adds loaded <see cref="ISettings"/> to the <see cref="SettingsExtensions.Cache"/>. </summary>
	[Test]
	public void Check_Load_Adds_Instance_To_SettingsExtensions_Cache()
	{
		// Arrange
		var manager = _fixture.Create<Mock<SettingsManager<string>>>().Object;

		// Act
		manager.Load<Settings>();

		// Assert
		Assert.That(SettingsExtensions.Cache, Has.Count.EqualTo(1));
	}

	#endregion
}