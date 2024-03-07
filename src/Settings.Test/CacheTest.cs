using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Settings;
using Phoenix.Functionality.Settings.Cache;

namespace Settings.Test;

public class CacheTest
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

	class TestSettings : ISettings { }
		
	class OtherTestSettings : ISettings { }
		
	class DifferentTestSettings : ISettings { }

	#endregion

	#region Load Settings

	[Test]
	public void LoadFromStrongCache()
	{
		var cache = new SettingsCache();

		int factoryExecutionCount = 0;
		TestSettings Factory()
		{
			factoryExecutionCount++;
			return new TestSettings();
		}

		var wasLoadedFromCache = this.OperateCache(cache, Factory);
		Assert.That(wasLoadedFromCache, Is.False);
		Assert.That(1, Is.EqualTo(factoryExecutionCount));

		this.OperateCache(cache, Factory);
		wasLoadedFromCache = this.OperateCache(cache, Factory);
		Assert.That(wasLoadedFromCache, Is.True);
		Assert.That(1, Is.EqualTo(factoryExecutionCount));
	}

	[Test]
	public async Task LoadFromWeakCache()
	{
		var cache = new WeakSettingsCache();

		int factoryExecutionCount = 0;
		TestSettings Factory()
		{
			factoryExecutionCount++;
			return new TestSettings();
		}

		var wasLoadedFromCache = this.OperateCache(cache, Factory);
		Assert.That(wasLoadedFromCache, Is.False);
		Assert.That(1, Is.EqualTo(factoryExecutionCount));

		GC.Collect();
		GC.WaitForPendingFinalizers();

		await Task.Delay(5000);

		// After the GC (hopefully) did its job, the stored reference to the settings should be gone. Therefore the settings instance shouldn't be from the cache.
		wasLoadedFromCache = this.OperateCache(cache, Factory);
		Assert.That(wasLoadedFromCache, Is.False);
		Assert.That(2, Is.EqualTo(factoryExecutionCount));

		// Without GC, the stored reference should still be alive and therefore the settings instance should be loaded from cache.
		wasLoadedFromCache = this.OperateCache(cache, Factory);
		Assert.That(wasLoadedFromCache, Is.True);
		Assert.That(2, Is.EqualTo(factoryExecutionCount));
	}

	[Test]
	public void LoadFromNoCache()
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
		Assert.That(wasLoadedFromCache, Is.False);
		Assert.That(1, Is.EqualTo(factoryExecutionCount));

		// Load #2:
		wasLoadedFromCache = this.OperateCache(cache, Factory);
		Assert.That(wasLoadedFromCache, Is.False);
		Assert.That(2, Is.EqualTo(factoryExecutionCount));

		// Load #3:
		wasLoadedFromCache = this.OperateCache(cache, Factory);
		Assert.That(wasLoadedFromCache, Is.False);
		Assert.That(3, Is.EqualTo(factoryExecutionCount));

		// Load #4:
		wasLoadedFromCache = this.OperateCache(cache, Factory);
		Assert.That(wasLoadedFromCache, Is.False);
		Assert.That(4, Is.EqualTo(factoryExecutionCount));
	}

	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	private bool OperateCache<TSettings>(ISettingsCache cache, Func<TSettings> factory) where TSettings : class, ISettings, new()
	{
		var wasLoadedFromCache = cache.TryGetOrAdd(out var settings, factory);
		Assert.That(settings, Is.TypeOf<TSettings>());
		return wasLoadedFromCache;
	}

	#endregion

	#region Load Different Settings

	[Test]
	public void LoadDifferentSettingsFromStrongCache()
		=> this.LoadDifferentSettings(new SettingsCache());

	[Test]
	public void LoadDifferentSettingsFromWeakCache()
		=> this.LoadDifferentSettings(new WeakSettingsCache());
		
	[Test]
	public void LoadDifferentSettingsFromNoCache()
		=> this.LoadDifferentSettings(new NoSettingsCache());

	private void LoadDifferentSettings(ISettingsCache cache)
	{
		TestSettings Factory() => new();
		OtherTestSettings OtherFactory() => new();
		DifferentTestSettings DifferentFactory() => new();

		// Check different types after creation.
		cache.TryGetOrAdd(out var settings, Factory);
		cache.TryGetOrAdd(out var otherSettings, OtherFactory);
		cache.TryGetOrAdd(out var differentSettings, DifferentFactory);
		Assert.That(settings, Is.TypeOf<TestSettings>());
		Assert.That(otherSettings, Is.TypeOf<OtherTestSettings>());
		Assert.That(differentSettings, Is.TypeOf<DifferentTestSettings>());

		// Check different types from cache.
		cache.TryGetOrAdd(out settings, Factory);
		cache.TryGetOrAdd(out otherSettings, OtherFactory);
		cache.TryGetOrAdd(out differentSettings, DifferentFactory);
		Assert.That(settings, Is.TypeOf<TestSettings>());
		Assert.That(otherSettings, Is.TypeOf<OtherTestSettings>());
		Assert.That(differentSettings, Is.TypeOf<DifferentTestSettings>());
	}

	#endregion

	#region Get All Settings

	[Test]
	public void CheckGetAllSettingsFromStrongCache()
	{
		var settings = this.LoadAllSettings(new SettingsCache());
		Assert.That(3, Is.EqualTo(settings.Count));
	}

	[Test]
	public void CheckGetAllSettingsFromWeakCache()
	{
		var settings = this.LoadAllSettings(new WeakSettingsCache());
		Assert.That(3, Is.EqualTo(settings.Count));
	}

	[Test]
	public void CheckGetAllSettingsNoCache()
	{
		var settings = this.LoadAllSettings(new NoSettingsCache());
		Assert.That(0, Is.EqualTo(settings.Count));
	}

	private ICollection<ISettings> LoadAllSettings(ISettingsCache cache)
	{
		TestSettings Factory() => new();
		OtherTestSettings OtherFactory() => new();
		DifferentTestSettings DifferentFactory() => new();

		// Check different types after creation.
		cache.TryGetOrAdd(out var settings, Factory);
		cache.TryGetOrAdd(out var otherSettings, OtherFactory);
		cache.TryGetOrAdd(out var differentSettings, DifferentFactory);

		return cache.GetAllCachedSettings();
	}

	#endregion

	#region Delete Settings
	
	[Test]
	public void CheckDeleteSettingsFromStrongCache()
	{
		var cache = new SettingsCache();
		var wasRemoved = this.DeleteSettings(cache);
		Assert.That(wasRemoved, Is.True);
		Assert.That(cache.GetAllCachedSettings(), Is.Empty);
	}
	
	[Test]
	public void CheckDeleteSettingsFromWeakCache()
	{
		var cache = new WeakSettingsCache();
		var wasRemoved = this.DeleteSettings(cache);
		Assert.That(wasRemoved, Is.True);
		Assert.That(cache.GetAllCachedSettings(), Is.Empty);
	}
	
	[Test]
	public void CheckDeleteSettingsFromNoCache()
	{
		var cache = new NoSettingsCache();
		var wasRemoved = this.DeleteSettings(cache);
		Assert.That(wasRemoved, Is.False);
		Assert.That(cache.GetAllCachedSettings(), Is.Empty);
	}

	private bool DeleteSettings(ISettingsCache cache)
	{
		var settings = new TestSettings();
		cache.AddOrUpdate(settings);
		return cache.TryRemove<TestSettings>(out _);
	}

	#endregion
}