#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using Phoenix.Functionality.Settings.Cache;

namespace Phoenix.Functionality.Settings;

/// <summary>
/// <see cref="ISettingsManager"/> builder.
/// </summary>
/// <typeparam name="TSettingsData"> The type of the data the settings is saved as. </typeparam>
public class SettingsManagerBuilder<TSettingsData>
	: ISettingsSinkBuilder<TSettingsData>,
		ISettingsSerializerBuilder<TSettingsData>,
		ISettingsCacheBuilder, 
		ISettingsManagerCreator
	where TSettingsData : class
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	
	private ISettingsSink<TSettingsData>? _sink;

	private ISettingsSerializer<TSettingsData>? _serializer;

	private ISettingsCache? _cache;

	private readonly List<Func<ISettingsManager, ISettingsManager>> _wrapperCallbacks;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	internal SettingsManagerBuilder()
	{
		// Save parameters.

		// Initialize fields.
		_wrapperCallbacks = new List<Func<ISettingsManager, ISettingsManager>>();
	}

	#endregion

	#region Methods

	/// <inheritdoc cref="ISettingsSinkBuilder{TSettingsData}.AddSink"/>
	public ISettingsSerializerBuilder<TSettingsData> AddSink(ISettingsSink<TSettingsData> sink)
	{
		_sink = sink;
		return this;
	}

	/// <inheritdoc cref="ISettingsSerializerBuilder{TSettingsData}.AddSerializer"/>
	public ISettingsCacheBuilder AddSerializer(ISettingsSerializer<TSettingsData> serializer)
	{
		_serializer = serializer;
		return this;
	}

	/// <inheritdoc cref="ISettingsCacheBuilder.AddCache"/>
	public ISettingsManagerCreator AddCache(ISettingsCache cache)
	{
		_cache = cache;
		return this;
	}

	/// <inheritdoc cref="ISettingsCacheBuilder.WithoutCache"/>
	public ISettingsManagerCreator WithoutCache()
	{
		return this;
	}

	/// <inheritdoc cref="ISettingsManagerCreator.AddWrapper"/>
	public ISettingsManagerCreator AddWrapper(Func<ISettingsManager, ISettingsManager> wrapperCallback)
	{
		_wrapperCallbacks.Add(wrapperCallback);
		return this;
	}

	/// <inheritdoc cref="ISettingsManagerCreator.Build"/>
	public ISettingsManager Build()
	{
		if (_sink is null) throw new MissingMemberException(nameof(SettingsManagerBuilder<TSettingsData>), nameof(_sink));
		if (_serializer is null) throw new MissingMemberException(nameof(SettingsManagerBuilder<TSettingsData>), nameof(_serializer));
		
		ISettingsManager settingsManager = new SettingsManager<TSettingsData>(_sink, _serializer, _cache);
		return _wrapperCallbacks.Aggregate(settingsManager, (current, wrapperCallback) => wrapperCallback.Invoke(current));
	}

	#endregion
}

/// <summary>
/// Partial builder interface for adding the <see cref="ISettingsSink"/>.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ISettingsSinkBuilder<TSettingsData>
	where TSettingsData : class
{
	/// <summary>
	/// Adds the <paramref name="sink"/> that will be used to build the <see cref="ISettingsManager"/>.
	/// </summary>
	ISettingsSerializerBuilder<TSettingsData> AddSink(ISettingsSink<TSettingsData> sink);
}

/// <summary>
/// Partial builder interface for adding the <see cref="ISettingsSerializer"/>.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ISettingsSerializerBuilder<TSettingsData>
	where TSettingsData : class
{
	/// <summary>
	/// Adds the <paramref name="serializer"/> that will be used to build the <see cref="ISettingsManager"/>.
	/// </summary>
	ISettingsCacheBuilder AddSerializer(ISettingsSerializer<TSettingsData> serializer);
}

/// <summary>
/// Partial builder interface for adding the <see cref="ISettingsCache"/>.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ISettingsCacheBuilder
{
	/// <summary>
	/// Adds the <paramref name="cache"/> that will be used to build the <see cref="ISettingsManager"/>.
	/// </summary>
	ISettingsManagerCreator AddCache(ISettingsCache cache);

	/// <summary>
	/// The <see cref="ISettingsManager"/> will not use any internal cache.
	/// </summary>
	ISettingsManagerCreator WithoutCache();
}

/// <summary>
/// Partial builder interface for building the <see cref="ISettingsManager"/>.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ISettingsManagerCreator
{
	/// <summary>
	/// Adds a wrapping <see cref="ISettingsManager"/> to the one that this builder created.
	/// </summary>
	ISettingsManagerCreator AddWrapper(Func<ISettingsManager, ISettingsManager> wrapperCallback);

	/// <summary>
	/// Builds the <see cref="ISettingsManager"/>.
	/// </summary>
	ISettingsManager Build();
}