#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Text.Json;
using System.Text.Json.Serialization;

namespace Phoenix.Functionality.Settings.Serializers.Json.Net;

/// <summary>
/// Contains extension methods for the builder pattern of an <see cref="ISettingsManager"/>.
/// </summary>
public static class JsonSettingsSerializerBuilderExtensions
{
	/// <summary>
	/// Adds the <see cref="JsonSettingsSerializer"/> as <see cref="ISettingsSerializer"/> to the builder.
	/// </summary>
	/// <param name="serializerBuilder"> The extended <see cref="ISettingsSerializerBuilder{TSettingsData}"/> </param>
	/// <returns> An <see cref="IJsonSettingsSerializerOptionsBuilder"/> for chaining. </returns>
	public static IJsonSettingsSerializerOptionsBuilder UsingJsonSerializer(this ISettingsSerializerBuilder<string> serializerBuilder)
	{
		return new JsonSettingsSerializerBuilder(serializerBuilder);
	}
}

internal class JsonSettingsSerializerBuilder : IJsonSettingsSerializerOptionsBuilder
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	private readonly ISettingsSerializerBuilder<string> _serializerBuilder;

	#endregion

	#region Properties

	internal List<JsonConverter> Converters { get; }
	
	#endregion

	#region (De)Constructors

	public JsonSettingsSerializerBuilder(ISettingsSerializerBuilder<string> serializerBuilder)
	{
		_serializerBuilder = serializerBuilder;
		this.Converters = new List<JsonConverter>();
	}

	#endregion

	#region Methods

	/// <inheritdoc cref="IJsonSettingsSerializerConverterBuilder.AddConverter"/>
	public IJsonSettingsSerializerOptionsBuilder AddConverter(JsonConverter customConverter)
	{
		this.Converters.Add(customConverter);
		return this;
	}

	/// <inheritdoc cref="IJsonSettingsSerializerOptionsBuilder.WithDefaultSerializerOptions"/>
	public ISettingsCacheBuilder WithDefaultSerializerOptions()
	{
		return _serializerBuilder.AddSerializer(new JsonSettingsSerializer(this.Converters.ToArray()));
	}

	/// <inheritdoc cref="IJsonSettingsSerializerOptionsBuilder.WithSerializerOptions"/>
	public ISettingsCacheBuilder WithSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
	{
		return _serializerBuilder.AddSerializer(new JsonSettingsSerializer(jsonSerializerOptions, this.Converters.ToArray()));
	}

	#endregion
}

/// <summary>
/// Partial builder interface for adding custom <see cref="JsonConverter"/>s when building a <see cref="JsonSettingsSerializer"/>.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IJsonSettingsSerializerConverterBuilder
{
	/// <summary>
	/// Adds the <paramref name="customConverter"/> to the <see cref="JsonSettingsSerializer"/>.
	/// </summary>
	IJsonSettingsSerializerOptionsBuilder AddConverter(JsonConverter customConverter);

}

/// <summary>
/// Partial builder interface for specifying <see cref="JsonSerializerOptions"/> when building a <see cref="JsonSettingsSerializer"/>.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IJsonSettingsSerializerOptionsBuilder : IJsonSettingsSerializerConverterBuilder
{
	/// <summary>
	/// Builds the <see cref="JsonSettingsSerializer"/> with the <see cref="JsonSettingsSerializer.DefaultJsonSerializerOptions"/>.
	/// </summary>
	ISettingsCacheBuilder WithDefaultSerializerOptions();

	/// <summary>
	/// Builds the <see cref="JsonSettingsSerializer"/> with the given <paramref name="jsonSerializerOptions"/>.
	/// </summary>
	/// <param name="jsonSerializerOptions"></param>
	/// <returns></returns>
	ISettingsCacheBuilder WithSerializerOptions(JsonSerializerOptions jsonSerializerOptions);
}