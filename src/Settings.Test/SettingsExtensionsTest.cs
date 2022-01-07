using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Settings;

namespace Settings.Test
{
	public class SettingsExtensionsTest
	{
		[SetUp]
		public void Setup()
		{
			SettingsExtensions.Cache.Clear();
		}
		
		#region Reload

		class Settings : ISettings { }

		class OtherSettings : ISettings { }

		[Test]
		public void Invoking_Reload_With_Different_Types_Throws()
		{
			//var settings = new Settings();
			//Assert.Throws<SettingsTypeMismatchException>(() => settings.Reload<OtherSettings>());
			Assert.Pass("This test is superfluous, as the 'Reload' functions generic type now always matches the extended settings type. This leads to design time error now which prevents compilation.");
		}

		[Test]
		public void Invoking_Reload_Without_Initialization_Throws()
		{
			var settings = new Settings();
			Assert.Throws<MissingSettingsManagerException>(() => settings.Reload<Settings>());
		}

		[Test]
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
			settingsManagerMock.Verify(manager => manager.Load<Settings>(true, false), Times.Once());
			
			settings.Reload<Settings>(true);
			settingsManagerMock.Verify(manager => manager.Load<Settings>(true, true), Times.Once());
		}

		#endregion

		#region Save

		[Test]
		public void Invoking_Save_Without_Initialization_Throws()
		{
			var settingsMock = new Mock<ISettings>();
			var settings = settingsMock.Object;
			Assert.Throws<MissingSettingsManagerException>(() => settings.Save());
		}

		[Test]
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
			settingsManagerMock.Verify(manager => manager.Save<ISettings>(settings, false), Times.Once());

			settings.Save(true);
			settingsManagerMock.Verify(manager => manager.Save<ISettings>(settings, true), Times.Once());
		}

		#endregion
	}
}