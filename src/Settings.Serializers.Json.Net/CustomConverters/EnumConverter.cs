#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;

/// <summary> Options for the <see cref="EnumConverter"/>. </summary>
public class EnumConverterOptions
{
	/// <summary> The options used to write out enumeration values. If not specified, the values are not serialized. </summary>
	public IWriteOutOptions WriteOutOptions { get; init; } = new NoValueWriteOut();
}

/// <summary> Wrapper interface for enumeration value write out. </summary>
public interface IWriteOutOptions { }

/// <summary> Enumeration values will not be serialized. </summary>
internal record NoValueWriteOut : IWriteOutOptions;

/// <inheritdoc cref="WriteOutValues.AsSuffix"/>
internal record ValueWriteOutAsSuffix(string Start, string Separator, string End) : IWriteOutOptions;

/// <summary>
/// Helper for creating different <see cref="IWriteOutOptions"/>.
/// </summary>
public static class WriteOutValues
{
	/// <summary>
	/// Enumeration values will be serialized as suffix within the property value.
	/// </summary>
	/// <param name="start"> The string used for the start of the value listing. Default is <b>[</b>. </param>
	/// <param name="separator"> The string used as separator for the values. Default is <b>, </b> </param>
	/// <param name="end"> The string used as the end of the value listing. Default is <b>]</b>. </param>
	/// <returns> A new <see cref="IWriteOutOptions"/> instance. </returns>
	public static IWriteOutOptions AsSuffix(string? start = "[", string? separator = ", ", string? end = "]")
		=> new ValueWriteOutAsSuffix(start ?? "[", separator ?? ", ", end ?? "]");
}

/// <summary>
/// Custom json converter for <see cref="Enum"/>s.
/// </summary>
public class EnumConverter : JsonConverterFactory
{
	private readonly EnumConverterOptions? _options;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="writeOutOptions"> The <see cref="IWriteOutOptions"/> used for writing out enumeration values. </param>
	public EnumConverter(IWriteOutOptions writeOutOptions)
	: this(new EnumConverterOptions() { WriteOutOptions = writeOutOptions }) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="options"> The <see cref="EnumConverterOptions"/> used for (de)serialization. </param>
	public EnumConverter(EnumConverterOptions? options = null)
	{
		_options = options;
	}

	/// <inheritdoc />
	public override bool CanConvert(Type typeToConvert)
	{
		if (typeToConvert.IsEnum) return true;
		return Nullable.GetUnderlyingType(typeToConvert)?.IsEnum ?? false;
	}

	/// <inheritdoc />
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions _) => CreateConverter(typeToConvert, _options);

	internal static JsonConverter CreateConverter(Type typeToConvert, EnumConverterOptions? options)
	{
		//? Use a cache per enum type?
		//* Depending on the amount of same enumeration types, this may be beneficial. On the other hand it may keep converter instances alive, that are only used one time.
		var converter = Activator.CreateInstance(typeof(InternalEnumConverter<>).MakeGenericType(typeToConvert), args: options);
		return (JsonConverter) converter;
	}

	#region Nested Types

	/// <summary>
	/// Provides static properties for the generic <see cref="InternalEnumConverter{TEnum}"/>.
	/// </summary>
	internal class InternalEnumConverterHelper
	{
		/// <summary>
		/// Matches the first whitespace not preceded by a comma and everything afterward.
		/// </summary>
		internal static Regex ValueCleanRegEx { get; } = new(@"(?<!,) .*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	}

	/// <summary>
	/// Custom json converter for <see cref="Enum"/>s.
	/// </summary>
	internal class InternalEnumConverter<TEnum> : JsonConverter<TEnum>
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private readonly EnumConverterOptions _options;

		private readonly Type _enumType;
		
		private readonly bool _isNullable;

		private readonly object? _defaultValue;

		#endregion

		#region Properties
		
		/// <inheritdoc />
		/// <remarks> https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-6-0#handle-null-values </remarks>
		public override bool HandleNull => true;

		#endregion

		#region (De)Constructors
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="options"> The <see cref="EnumConverterOptions"/> used for (de)serialization. </param>
		public InternalEnumConverter(EnumConverterOptions? options = null)
		{
			// Save parameters.
			_options = options ?? new EnumConverterOptions();

			// Initialize fields.
			_enumType = Nullable.GetUnderlyingType(typeof(TEnum)) ?? typeof(TEnum);
			_defaultValue = GetDefaultValue();
			_isNullable = _defaultValue is null;
		}

		#endregion

		#region Methods

		#region Deserialization

		/// <inheritdoc />
		public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> this.Deserialize(reader.GetString());

		/// <inheritdoc />
		public override TEnum ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> this.Deserialize(reader.GetString());

		internal TEnum? Deserialize(string? value)
		{
			value = CleanEnumValue(value);
			if (String.IsNullOrWhiteSpace(value))
			{
				if (_isNullable) return default;
				else
				{
					//var defaultEnumerationValue = GetDefaultValue(enumerationType);
					var defaultEnumerationValue = _defaultValue;
					if (defaultEnumerationValue is null) throw new JsonException($"Cannot convert the value '{value}' into a {_enumType.Name}.");
					return (TEnum?) defaultEnumerationValue;
				}
			}
			
#if NET5_0_OR_GREATER
			if (Enum.TryParse(_enumType, value, true, out var enumeration))
			{
#else
			if (TryParseEnum(value!, _enumType, out var enumeration))
			{
#endif
				return (TEnum?) enumeration;
			}
			else
			{
				throw new JsonException($"Cannot convert the value '{value}' into {_enumType.Name}.");
			}
		}
		
		#endregion

		#region Serialization

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, TEnum? enumeration, JsonSerializerOptions options)
		{
			var enumString = this.Serialize(enumeration, false);
			if (enumString is not null && options.PropertyNamingPolicy is not null) enumString = options.PropertyNamingPolicy.ConvertName(enumString);
			writer.WriteStringValue(enumString);
		}

		/// <inheritdoc />
		public override void WriteAsPropertyName(Utf8JsonWriter writer, TEnum? enumeration, JsonSerializerOptions options)
		{
			var enumString = this.Serialize(enumeration, true) ?? "null";
			if (options.PropertyNamingPolicy is not null) enumString = options.PropertyNamingPolicy.ConvertName(enumString);
			writer.WritePropertyName(enumString);
		}

		internal string? Serialize(TEnum? enumeration, bool blockWriteOut = false)
		{
			var enumString = enumeration?.ToString();
			var enumValues = GetEnumerationValues(_enumType, _isNullable);

			if (!blockWriteOut && _options.WriteOutOptions is ValueWriteOutAsSuffix suffixOptions)
			{
				var suffix = $" {suffixOptions.Start}{String.Join(suffixOptions.Separator, enumValues)}{suffixOptions.End}";
				return $"{enumString ?? "null"}{suffix}";
			}
			
			return enumString;
		}

		#endregion

		#region Helper

#if !NET5_0_OR_GREATER
		private static bool TryParseEnum(string value, Type enumerationType, out object? enumeration)
		{
			try
			{
				enumeration = Enum.Parse(enumerationType, value, true);
				return true;
			}
			catch (ArgumentException )
			{
				enumeration = null;
				return false;
			}
		}
#endif

		private static IEnumerable<string> GetEnumerationValues(Type enumerationType, bool isNullable)
		{
			if (isNullable) yield return "null";
			var enumValues = enumerationType.GetEnumValues();
			foreach (var enumValue in enumValues)
			{
				yield return enumValue.ToString()!;
			}
		}

		private static string? CleanEnumValue(string? value)
		{
			if (value == null) return null;
			var cleanedValue = InternalEnumConverterHelper.ValueCleanRegEx.Replace(value, String.Empty);
			return cleanedValue == "null" ? null : cleanedValue; // This is necessary because the WriteOutOption 'AsSuffix' needs to wrap null values inside a string.
		}

		internal static object? GetDefaultValue(/*Type enumerationType*/)
		{
			if (Nullable.GetUnderlyingType(typeof(TEnum)) is not null) return null;
			var enumerationType = typeof(TEnum);

			// Check for default attribute and return its value if available.
			var attribute = enumerationType.GetCustomAttribute<System.ComponentModel.DefaultValueAttribute>(inherit: false);
			if (attribute?.Value != null) return attribute.Value;

			// Try to get the value with the numeric counterpart of 0 (zero).
			var numericType = enumerationType.GetEnumUnderlyingType();
			var zero = Activator.CreateInstance(numericType)!;
			if (enumerationType.IsEnumDefined(zero)) return Enum.Parse(enumerationType, zero.ToString()!);

			/*
			* Per definition (https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum)
			* "...The default value of an enumeration type E is the value produced by expression (E)0, even if zero doesn't have the corresponding enum member...".
			* This means that even if '0' is not explicitly defined in an enumeration, the cast will succeed.
			* Contrary to this behavior, this method will return the first available value of an enumeration, if nothing is defined for '0',
			* to always return a valid enumeration value.
			*/
			
			//// No default value available.
			//return null;
			
			// Get the first defined value of the enumeration.
			var values = enumerationType.GetEnumValues();
			return values.GetValue(0);
		}

		#endregion

		#endregion
	}

	#endregion
}