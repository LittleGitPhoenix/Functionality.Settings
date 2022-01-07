using NUnit.Framework;
using Phoenix.Functionality.Settings;
using Phoenix.Functionality.Settings.Cache;
using Phoenix.Functionality.Settings.Json.Net;

namespace Settings.Json.Net.Test;

public class JsonSettingsManagerTest
{
	#region Setup

	[SetUp]
	public void BeforeEachTest()
	{
		var targetWorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
		var actualWorkingDirectory = Directory.GetCurrentDirectory();
		if (targetWorkingDirectory != actualWorkingDirectory)
		{
			/*
			! If running under .NET Framework 4.8, the working directory is not the build directory, but rather the solution root.
			! Therefor the existing settings file won't be found and the test will fail.
			*/
			Console.WriteLine($"The working directory '{actualWorkingDirectory}' mismatches its expectation '{targetWorkingDirectory}'. It has been changed for the unit test to properly run.");
			Directory.SetCurrentDirectory(targetWorkingDirectory);
		}
	}

	#endregion

	#region Data

	class UnnamedSettings : ISettings { }

	[SettingsFileName("CustomName")]
	class AttributedSettings : ISettings { }

	#endregion

	[Test]
	public void Build_Without_Cache()
	{
		var settingsManager = JsonSettingsManager
				.Construct()
				.UseDefaultDirectory()
				.WithoutCache()
				.Build()
			;

		Assert.IsNotNull(settingsManager);
		Assert.AreEqual(typeof(JsonSettingsManager), settingsManager.GetType());
		Assert.AreEqual(typeof(NoSettingsCache), settingsManager.Cache.GetType());
	}

	[Test]
	public void Build_With_Cache()
	{
		var settingsManager = JsonSettingsManager
				.Construct()
				.UseDefaultDirectory()
				.WithCache()
				.Build()
			;

		Assert.IsNotNull(settingsManager);
		Assert.AreEqual(typeof(JsonSettingsManager), settingsManager.GetType());
		Assert.AreEqual(typeof(SettingsCache), settingsManager.Cache.GetType());
	}

	[Test]
	public void Build_With_Weak_Cache()
	{
		var settingsManager = JsonSettingsManager
				.Construct()
				.UseDefaultDirectory()
				.WithWeakCache()
				.Build()
			;

		Assert.IsNotNull(settingsManager);
		Assert.AreEqual(typeof(JsonSettingsManager), settingsManager.GetType());
		Assert.AreEqual(typeof(WeakSettingsCache), settingsManager.Cache.GetType());
	}

	[Test]
	public void Get_Settings_Name()
	{
		var targetName = $"{typeof(UnnamedSettings).Namespace}.{typeof(UnnamedSettings).Name}";

		var settingsManager = new JsonSettingsManager();
		var actualName = settingsManager.GetSettingsFileNameWithoutExtension<UnnamedSettings>();

		Assert.AreEqual(targetName, actualName);
	}

	[Test]
	public void Get_Custom_Settings_Name()
	{
		var targetName = $"CustomName";

		var settingsManager = new JsonSettingsManager();
		var actualName = settingsManager.GetSettingsFileNameWithoutExtension<AttributedSettings>();

		Assert.AreEqual(targetName, actualName);
	}
}