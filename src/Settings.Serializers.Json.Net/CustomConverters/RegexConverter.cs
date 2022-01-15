#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;

/// <summary>
/// Custom json converter for <see cref="Regex"/>.
/// </summary>
public class RegexConverter : JsonConverter<Regex>
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	
	private readonly RegexOptions _regexOptions;
	
	private readonly Regex _fallbackPattern;
	
	#endregion

	#region Properties

	/// <summary> The default fallback pattern used if a null-string is parsed. </summary>
	public static string DefaultFallbackPattern = ".*";
	
	/// <summary> The <see cref="RegexOptions"/> applied to newly created <see cref="Regex"/> instances. </summary>
	public static RegexOptions DefaultRegexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled;

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	public RegexConverter()
		: this(null, null) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="regexOptions"> Optional <see cref="RegexOptions"/> applied to newly created <see cref="Regex"/> instances. Default is <see cref="DefaultRegexOptions"/>. </param>
	public RegexConverter(RegexOptions? regexOptions = null)
		: this(null, regexOptions) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="fallbackPattern"> Optional fallback pattern used if a null-string is parsed. Default is <see cref="DefaultFallbackPattern"/>. </param>
	/// <param name="regexOptions"> Optional <see cref="RegexOptions"/> applied to newly created <see cref="Regex"/> instances. Default is <see cref="DefaultRegexOptions"/>. </param>
	public RegexConverter(string? fallbackPattern = null, RegexOptions? regexOptions = null)
	{
		// Save parameters.
		fallbackPattern ??= DefaultFallbackPattern;
		_regexOptions = regexOptions ?? DefaultRegexOptions;

		// Initialize fields.
		_fallbackPattern = new Regex(fallbackPattern, _regexOptions);
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public override Regex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> this.Deserialize(reader.GetString());

	internal Regex Deserialize(string? value)
	{
		if (value is null) return _fallbackPattern;
		return new Regex(value, _regexOptions);
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, Regex value, JsonSerializerOptions options)
		=> writer.WriteStringValue(this.Serialize(value));

	internal string Serialize(Regex regex)
	{
		//! The Regex class has no public property that allows access to its pattern. Using the ToString() method works, but this could change at any time.
		return regex.ToString();
	}

	#endregion
}