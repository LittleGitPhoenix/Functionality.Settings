using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Settings;

namespace Settings.Test;

public class SettingsExtensionsTest
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
	
	class Settings : ISettings { }

	class OtherSettings : ISettings { }
	
	/// <summary>
	/// Special <see cref="ISettingsManager"/> that can be used to verify the generic type parameter of the implemented methods.
	/// </summary>
	class TypeMatchSettingsManager : ISettingsManager
	{
		public Settings Settings { get; }

		private readonly Type _settingsType;

		public TypeMatchSettingsManager()
		{
			this.Settings = new Settings();
			this.Settings.InitializeExtensionMethods(this);
			_settingsType = this.Settings.GetType();
		}

		#region Implementation of ISettingsManager

		/// <inheritdoc />
		public TSettings Load<TSettings>(bool bypassCache = false, bool preventCreation = false, bool preventUpdate = false)
			where TSettings : class, ISettings, new()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (typeof(TSettings) != _settingsType) throw new SettingsTypeMismatchException(_settingsType, typeof(TSettings));
#pragma warning restore CS0618 // Type or member is obsolete
			return default;
		}

		/// <inheritdoc />
		public void Save<TSettings>(TSettings settings, bool createBackup = default)
			where TSettings : ISettings
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (typeof(TSettings) != _settingsType) throw new SettingsTypeMismatchException(_settingsType, typeof(TSettings));
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <inheritdoc />
		public void Delete<TSettings>(bool createBackup = default)
			where TSettings : ISettings
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (typeof(TSettings) != _settingsType) throw new SettingsTypeMismatchException(_settingsType, typeof(TSettings));
#pragma warning restore CS0618 // Type or member is obsolete
		}

		#endregion
	}

	#endregion

	#region Tests

	#region Reload

	[Test]
	public void InvokingReloadWithDifferentTypesThrows()
	{
		//var settings = new Settings();
		//Assert.Throws<SettingsTypeMismatchException>(() => settings.Reload<OtherSettings>());
		Assert.Pass("This test is superfluous, as the 'Reload' functions generic type now always matches the extended settings type. This leads to design time error now which prevents compilation.");
	}

	[Test]
	public void InvokingReloadWithoutInitializationThrows()
	{
		var settings = new Settings();
		Assert.Throws<MissingSettingsManagerException>(() => settings.Reload<Settings>());
	}

	[Test]
	public void Invoking_Reload_Succeeds()
	{
		var settingsManagerMock = new Mock<ISettingsManager>();
		settingsManagerMock
			.Setup(manager => manager.Load<Settings>(true, It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns(() => default)
			.Verifiable()
			;
		var settingsManager = settingsManagerMock.Object;

		var settings = new Settings();
		settings.InitializeExtensionMethods(settingsManager);

		settings.Reload<Settings>(false);
		settingsManagerMock.Verify(manager => manager.Load<Settings>(true, false, It.IsAny<bool>()), Times.Once());

		settings.Reload<Settings>(true);
		settingsManagerMock.Verify(manager => manager.Load<Settings>(true, true, It.IsAny<bool>()), Times.Once());
	}

	#endregion

	#region Save

	[Test]
	public void InvokingSaveWithoutInitializationThrows()
	{
		var settingsMock = new Mock<ISettings>();
		var settings = settingsMock.Object;
		Assert.Throws<MissingSettingsManagerException>(() => settings.Save());
	}

	[Test]
	public void InvokingSaveSucceeds()
	{
		var settingsManagerMock = new Mock<ISettingsManager>();
		settingsManagerMock
			.Setup(manager => manager.Save(It.IsAny<ISettings>(), It.IsAny<bool>()))
			.Callback(() => { })
			.Verifiable()
			;
		var settingsManager = settingsManagerMock.Object;

		var settingsMock = new Mock<ISettings>();
		var settings = settingsMock.Object;
		settings.InitializeExtensionMethods(settingsManager);
			
		settings.Save(false);
		settingsManagerMock.Verify(manager => manager.Save<ISettings>(settings, false), Times.Once());

		settings.Save(true);
		settingsManagerMock.Verify(manager => manager.Save<ISettings>(settings, true), Times.Once());
	}

	/// <summary>
	/// This test checks, that using the extension method <see cref="SettingsExtensions.Save{TSettings}"/> passes the specific generic type parameter of the settings instance to the <see cref="ISettingsManager.Save{TSettings}"/> method.
	/// </summary>
	/// <remarks> If the extension method uses <see cref="ISettings"/> as generic type, then <see cref="SettingsSinkExtensions.GetSettingsName{TSettings}"/> would not be able to get the real name of the settings instance. </remarks>
	[Test]
	public void InvokingSaveUsesSpecificGenericType()
	{
		// Arrange
		var settingsManager = new TypeMatchSettingsManager();
		var settings = settingsManager.Settings;
		
		// Act + Assert
		Assert.DoesNotThrow(() => settings.Save(false));
	}

	#endregion

	#endregion
}