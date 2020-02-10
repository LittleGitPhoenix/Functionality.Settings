using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Phoenix.Functionality.Settings.Test
{
	[TestClass]
	public class SettingsExtensionsTest
	{
		#region Reload

		class Settings : ISettings { }

		class OtherSettings : ISettings { }

		[TestMethod]
		public void Invoking_Reload_With_Different_Types_Throws()
		{
			var settings = new Settings();
			Assert.ThrowsException<SettingsTypeMismatchException>(() => settings.Reload<OtherSettings>());
		}

		[TestMethod]
		public void Invoking_Reload_Without_Initialization_Throws()
		{
			var settings = new Settings();
			Assert.ThrowsException<MissingSettingsManagerException>(() => settings.Reload<Settings>());
		}

		[TestMethod]
		public void Invoking_Reload_Succeeds()
		{
			var settingsManagerMock = new Mock<ISettingsManager>();
			settingsManagerMock
				.Setup(manager => manager.Load<Settings>(true, It.IsAny<bool>()))
				.Returns(() => default)
				.Verifiable()
				;
			var settingsManager = settingsManagerMock.Object;

			var settings = new Settings();
			settings.InitializeExtensionMethods(settingsManager);

			settings.Reload<Settings>(false);
			settingsManagerMock.Verify(manager => manager.Load<Settings>(true, false), Times.Once);
			
			settings.Reload<Settings>(true);
			settingsManagerMock.Verify(manager => manager.Load<Settings>(true, true), Times.Once);
		}

		#endregion

		#region Save

		[TestMethod]
		public void Invoking_Save_Without_Initialization_Throws()
		{
			var settingsMock = new Mock<ISettings>();
			var settings = settingsMock.Object;
			Assert.ThrowsException<MissingSettingsManagerException>(() => settings.Save());
		}

		[TestMethod]
		public void Invoking_Save_Succeeds()
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
			settingsManagerMock.Verify(manager => manager.Save<ISettings>(settings, false), Times.Once);

			settings.Save(true);
			settingsManagerMock.Verify(manager => manager.Save<ISettings>(settings, true), Times.Once);
		}

		#endregion
	}
}