#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Dynamic;
using System.Text.Json;

namespace Phoenix.Functionality.Settings.Serializers.Json.Net;

/// <summary>
/// An <see cref="ISettingsSerializer"/> that (de)serializes JSON based settings.
/// </summary>
public class JsonSettingsSerializer : ISettingsSerializer<string>
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	/// <summary> The default <see cref="JsonSerializerOptions"/>. </summary>
	private static readonly JsonSerializerOptions DefaultJsonSerializerOptions;
	
	/// <summary> The <see cref="JsonSerializerOptions"/> used by this handler. </summary>
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	static JsonSettingsSerializer()
	{
		DefaultJsonSerializerOptions = new JsonSerializerOptions()
		{
			AllowTrailingCommas = true,
			IgnoreReadOnlyProperties = false,
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReadCommentHandling = JsonCommentHandling.Skip,
			WriteIndented = true,
			Converters =
			{
				// Build in:
				new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false),
			}
		};
	}

	/// <summary>
	/// Constructor
	/// </summary>
	public JsonSettingsSerializer()
		: this(DefaultJsonSerializerOptions) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="customJsonConverters"> An optional collection of custom <see cref="System.Text.Json.Serialization.JsonConverter"/>s. </param>
	public JsonSettingsSerializer(params System.Text.Json.Serialization.JsonConverter[] customJsonConverters)
		: this(DefaultJsonSerializerOptions, customJsonConverters) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="jsonSerializerOptions"> Optional <see cref="JsonSerializerOptions"/>. Default will be <see cref="DefaultJsonSerializerOptions"/>. </param>
	/// <param name="customJsonConverters"> An optional collection of custom <see cref="System.Text.Json.Serialization.JsonConverter"/>s. </param>
	public JsonSettingsSerializer(JsonSerializerOptions? jsonSerializerOptions = null, params System.Text.Json.Serialization.JsonConverter[] customJsonConverters)
	{
		// Save parameters.
		_jsonSerializerOptions = jsonSerializerOptions ?? DefaultJsonSerializerOptions;

		// Initialize fields.
		foreach (var customJsonConverter in customJsonConverters) TryAddCustomConverter(customJsonConverter, _jsonSerializerOptions.Converters);
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public TSettings Deserialize<TSettings>(string settingsData, out ExpandoObject? rawData) where TSettings : class, ISettings
	{
		try
		{
			rawData = null;
			if (String.IsNullOrWhiteSpace(settingsData)) throw new SettingsLoadException("Cannot de-serialize empty settings data.");

			// If it is necessary get the raw data as expando object.
			if (typeof(ISettingsLayoutChangedNotification).IsAssignableFrom(typeof(TSettings))) rawData = JsonSerializer.Deserialize<ExpandoObject>(settingsData, _jsonSerializerOptions);

			// Deserialize into the real instance.
			var settings = this.Deserialize<TSettings>(settingsData);
			return settings ?? throw new SettingsLoadException("The de-serialized settings data yielded null.");
		}
		catch (SettingsLoadException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new SettingsLoadException("Could not de-serialize the settings data. See the inner exception for further details.", ex);
		}
	}

	/// <inheritdoc />
	public string Serialize(ISettings settings)
	{
		try
		{
			//! This needs to be treated as an object, because the serializer otherwise only uses the properties the interface specifies (none in this case).
			return JsonSerializer.Serialize((object) settings, _jsonSerializerOptions);
		}
		catch (Exception ex)
		{
			throw new SettingsSaveException("Could not serialize the settings instance. See the inner exception for further details.", ex);
		}
	}

	/// <inheritdoc />
	public bool AreIdentical(ISettings settings, string settingsData)
	{
		try
		{
			var targetJsonData = this.Serialize(settings);
			
			//// Manual
			//var targetJsonDocument = JsonDocument.Parse(targetJsonData, CompareOptions);
			//var targetData = ConvertJsonDocumentToData(targetJsonDocument).Result;
			//var actualJsonDocument = JsonDocument.Parse(settingsData, CompareOptions);
			//var actualData = ConvertJsonDocumentToData(actualJsonDocument).Result;
			
			// Build-in
			var targetData = JsonSerializer.SerializeToUtf8Bytes(targetJsonData, _jsonSerializerOptions);
			//var target = System.Text.Encoding.UTF8.GetString(targetData, 0, targetData.Length);
			var actualData = JsonSerializer.SerializeToUtf8Bytes(settingsData, _jsonSerializerOptions);
			//var actual = System.Text.Encoding.UTF8.GetString(actualData, 0, actualData.Length);

			var areIdentical = targetData.SequenceEqual(actualData);
			return areIdentical;
		}
		catch (Exception ex)
		{
			throw new SettingsException("Could not compare the settings instance with the settings data. See the inner exception for further details.", ex);
		}
	}

	#region Helper

	internal static bool TryAddCustomConverter(System.Text.Json.Serialization.JsonConverter customJsonConverter, ICollection<System.Text.Json.Serialization.JsonConverter> customJsonConverters)
	{
		var newType = customJsonConverter.GetType();
		var converterTypes = customJsonConverters.Select(converter => converter.GetType()).ToArray();
		if (converterTypes.Contains(newType)) return false;
		customJsonConverters.Add(customJsonConverter);
		return true;
	}

	internal virtual TSettings? Deserialize<TSettings>(string settingsData)
		=> JsonSerializer.Deserialize<TSettings>(settingsData, _jsonSerializerOptions);

//	private static async Task<byte[]> ConvertJsonDocumentToData(JsonDocument document)
//	{
//#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
//		using var stream = new MemoryStream();
//#else
//		await using var stream = new MemoryStream();
//#endif
//		await using (var writer = new Utf8JsonWriter(stream))
//		{
//			document.RootElement.WriteTo(writer);
//			await writer.FlushAsync().ConfigureAwait(false);
//		}

//		stream.Seek(0, SeekOrigin.Begin);
//		return stream.ToArray();
//	}

	#endregion

	#endregion
}