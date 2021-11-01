using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Phoenix.Functionality.Settings;
using Phoenix.Functionality.Settings.Cache;

namespace Settings.Test
{
	public class CacheTest
	{
		#region Data

		class TestSettings : ISettings { }
		
		class OtherTestSettings : ISettings { }
		
		class DifferentTestSettings : ISettings { }

		#endregion

		#region Load Settings

		[Test]
		public void Load_From_Strong_Cache()
		{
			var cache = new SettingsCache();

			int factoryExecutionCount = 0;
			TestSettings Factory()
			{
				factoryExecutionCount++;
				return new TestSettings();
			}

			var wasLoadedFromCache = this.OperateCache(cache, Factory);
			Assert.IsFalse(wasLoadedFromCache);
			Assert.AreEqual(1, factoryExecutionCount);

			this.OperateCache(cache, Factory);
			wasLoadedFromCache = this.OperateCache(cache, Factory);
			Assert.IsTrue(wasLoadedFromCache);
			Assert.AreEqual(1, factoryExecutionCount);
		}

		[Test]
		public async Task Load_From_Weak_Cache()
		{
			var cache = new WeakSettingsCache();

			int factoryExecutionCount = 0;
			TestSettings Factory()
			{
				factoryExecutionCount++;
				return new TestSettings();
			}

			var wasLoadedFromCache = this.OperateCache(cache, Factory);
			Assert.IsFalse(wasLoadedFromCache);
			Assert.AreEqual(1, factoryExecutionCount);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			await Task.Delay(5000);

			// After the GC (hopefully) did its job, the stored reference to the settings should be gone. Therefore the settings instance shouldn't be from the cache.
			wasLoadedFromCache = this.OperateCache(cache, Factory);
			Assert.IsFalse(wasLoadedFromCache);
			Assert.AreEqual(2, factoryExecutionCount);

			// Without GC, the stored reference should still be alive and therefore the settings instance should be loaded from cache.
			wasLoadedFromCache = this.OperateCache(cache, Factory);
			Assert.IsTrue(wasLoadedFromCache);
			Assert.AreEqual(2, factoryExecutionCount);
		}

		[Test]
		public void Load_From_No_Cache()
		{
			var cache = new NoSettingsCache();

			int factoryExecutionCount = 0;
			TestSettings Factory()
			{
				factoryExecutionCount++;
				return new TestSettings();
			}

			// Load #1:
			var wasLoadedFromCache = this.OperateCache(cache, Factory);
			Assert.IsFalse(wasLoadedFromCache);
			Assert.AreEqual(1, factoryExecutionCount);

			// Load #2:
			wasLoadedFromCache = this.OperateCache(cache, Factory);
			Assert.IsFalse(wasLoadedFromCache);
			Assert.AreEqual(2, factoryExecutionCount);

			// Load #3:
			wasLoadedFromCache = this.OperateCache(cache, Factory);
			Assert.IsFalse(wasLoadedFromCache);
			Assert.AreEqual(3, factoryExecutionCount);

			// Load #4:
			wasLoadedFromCache = this.OperateCache(cache, Factory);
			Assert.IsFalse(wasLoadedFromCache);
			Assert.AreEqual(4, factoryExecutionCount);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		private bool OperateCache<TSettings>(ISettingsCache cache, Func<TSettings> factory) where TSettings : class, ISettings, new()
		{
			var wasLoadedFromCache = cache.TryGetOrAdd(out var settings, factory);
			Assert.AreEqual(settings.GetType(), typeof(TSettings));
			return wasLoadedFromCache;
		}

		#endregion

		#region Load Different Settings

		[Test]
		public void Load_Different_Settings_From_Strong_Cache()
			=> this.Load_Different_Settings(new SettingsCache());

		[Test]
		public void Load_Different_Settings_From_Weak_Cache()
			=> this.Load_Different_Settings(new WeakSettingsCache());
		
		[Test]
		public void Load_Different_Settings_From_No_Cache()
			=> this.Load_Different_Settings(new NoSettingsCache());

		private void Load_Different_Settings(ISettingsCache cache)
		{
			TestSettings Factory() => new TestSettings();
			OtherTestSettings OtherFactory() => new OtherTestSettings();
			DifferentTestSettings DifferentFactory() => new DifferentTestSettings();

			// Check different types after creation.
			cache.TryGetOrAdd(out var settings, Factory);
			cache.TryGetOrAdd(out var otherSettings, OtherFactory);
			cache.TryGetOrAdd(out var differentSettings, DifferentFactory);
			Assert.AreEqual(settings.GetType(), typeof(TestSettings));
			Assert.AreEqual(otherSettings.GetType(), typeof(OtherTestSettings));
			Assert.AreEqual(differentSettings.GetType(), typeof(DifferentTestSettings));

			// Check different types from cache.
			cache.TryGetOrAdd(out settings, Factory);
			cache.TryGetOrAdd(out otherSettings, OtherFactory);
			cache.TryGetOrAdd(out differentSettings, DifferentFactory);
			Assert.AreEqual(settings.GetType(), typeof(TestSettings));
			Assert.AreEqual(otherSettings.GetType(), typeof(OtherTestSettings));
			Assert.AreEqual(differentSettings.GetType(), typeof(DifferentTestSettings));
		}

		#endregion

		#region Get All Settings

		[Test]
		public void Check_Get_All_Settings_From_Strong_Cache()
		{
			var settings = this.Load_All_Settings(new SettingsCache());
			Assert.AreEqual(3, settings.Count);
		}

		[Test]
		public void Check_Get_All_Settings_From_Weak_Cache()
		{
			var settings = this.Load_All_Settings(new WeakSettingsCache());
			Assert.AreEqual(3, settings.Count);
		}

		[Test]
		public void Check_Get_All_Settings_No_Cache()
		{
			var settings = this.Load_All_Settings(new NoSettingsCache());
			Assert.AreEqual(0, settings.Count);
		}

		private ICollection<ISettings> Load_All_Settings(ISettingsCache cache)
		{
			TestSettings Factory() => new TestSettings();
			OtherTestSettings OtherFactory() => new OtherTestSettings();
			DifferentTestSettings DifferentFactory() => new DifferentTestSettings();

			// Check different types after creation.
			cache.TryGetOrAdd(out var settings, Factory);
			cache.TryGetOrAdd(out var otherSettings, OtherFactory);
			cache.TryGetOrAdd(out var differentSettings, DifferentFactory);

			return cache.GetAllCachedSettings();
		}

		#endregion
	}
}