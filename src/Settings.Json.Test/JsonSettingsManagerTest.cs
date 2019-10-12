using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phoenix.Functionality.Settings.Cache;
using Phoenix.Functionality.Settings.Json.Net;

namespace Phoenix.Functionality.Settings.Json.Test
{
	[TestClass]
	public class JsonSettingsManagerTest
	{
		#region Data

		class UnnamedSettings : ISettings { }

		[SettingsFileName("CustomName")]
		class AttributedSettings : ISettings { }

		#endregion

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
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

		[TestMethod]
		public void Get_Settings_Name()
		{
			var targetName = $"{typeof(UnnamedSettings).Namespace}.{typeof(UnnamedSettings).Name}";

			var settingsManager = new JsonSettingsManager();
			var actualName = settingsManager.GetSettingsFileNameWithoutExtension<UnnamedSettings>();

			Assert.AreEqual(targetName, actualName);
		}

		[TestMethod]
		public void Get_Custom_Settings_Name()
		{
			var targetName = $"CustomName";

			var settingsManager = new JsonSettingsManager();
			var actualName = settingsManager.GetSettingsFileNameWithoutExtension<AttributedSettings>();

			Assert.AreEqual(targetName, actualName);
		}
	}
}