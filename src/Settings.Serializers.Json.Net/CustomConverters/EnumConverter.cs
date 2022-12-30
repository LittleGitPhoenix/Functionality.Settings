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

/// <inheritdoc cref="WriteOutValues.AsComment"/>
public record ValueWriteOutAsComment(string Separator) : IWriteOutOptions;

/// <inheritdoc cref="WriteOutValues.AsProperty"/>
public record ValueWriteOutAsProperty : IWriteOutOptions;

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

	/// <summary>
	/// Enumeration values will be serialized as comment after the property value.
	/// </summary>
	/// <param name="separator"> The string used as separator for the values. Default is <b>, </b> </param>
	[Obsolete("It is currently not advised to use this method of enumeration value write-out, because comments are not part of the JSON specification and due limits imposed by System.Text.Json that prevent from writing comments above a property.")]
	public static IWriteOutOptions AsComment(string? separator = ", ")
		=> new ValueWriteOutAsComment(separator ?? ", ");

	/// <summary> Enumeration values will be serialized in a separate property as an array below the property of the enumeration. </summary>
	/// <remarks>
	/// <para> It is currently not advised to use this method of enumeration value write-out. </para>
	/// <para> Due to limitations of <see cref="System.Text.Json"/> serializer, the additional property will be named after the enumeration type, rather then the original property name. </para>
	/// <para> Additionally the property will be suffixed by a random number, to prevent duplicate keys in the final JSON. </para>
	/// </remarks>
	[Obsolete("It is currently not advised to use this method of enumeration value write-out, because System.Text.Json does not provide access to the original property name during serialization.")]
	public static IWriteOutOptions AsProperty()
		=> new ValueWriteOutAsProperty();
}

/// <summary>
/// Custom json converter for <see cref="Enum"/>s.
/// </summary>
public class EnumConverter : JsonConverterFactory
{
	private readonly Lazy<InternalEnumConverter> _converter;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="writeOutOptions"> The <see cref="IWriteOutOptions"/> used for writing out enumeration values. </param>
	public EnumConverter(IWriteOutOptions writeOutOptions)
	{
		_converter = new Lazy<InternalEnumConverter>(() => new InternalEnumConverter(new EnumConverterOptions() { WriteOutOptions = writeOutOptions }), LazyThreadSafetyMode.ExecutionAndPublication);
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="options"> The <see cref="EnumConverterOptions"/> used for (de)serialization. </param>
	public EnumConverter(EnumConverterOptions? options = null)
	{
		_converter = new Lazy<InternalEnumConverter>(() => new InternalEnumConverter(options), LazyThreadSafetyMode.ExecutionAndPublication);
	}

	/// <inheritdoc />
	public override bool CanConvert(Type typeToConvert)
	{
		if (typeToConvert.IsEnum) return true;
		if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Nullable<>)) return typeToConvert.GenericTypeArguments.First().IsEnum;
		return false;
	}

	/// <inheritdoc />
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		return _converter.Value;
	}

	#region Nested Types
	
	/// <summary>
	/// Custom json converter for <see cref="Enum"/>s.
	/// </summary>
	internal class InternalEnumConverter : JsonConverter<Enum>
	{
		private static readonly Random Random;
		
		private static readonly Regex ValueCleanRegEx;
	
		private readonly EnumConverterOptions _options;

		static InternalEnumConverter()
		{
			Random = new Random();
			ValueCleanRegEx = new Regex(@"(?<!,) .*", RegexOptions.Compiled | RegexOptions.IgnoreCase); // Matches the first whitespace not preceded by a comma and everything afterwards.
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="options"> The <see cref="EnumConverterOptions"/> used for (de)serialization. </param>
		public InternalEnumConverter(EnumConverterOptions? options = null)
		{
			_options = options ?? new EnumConverterOptions();
		}

		#region Deserialization

		/// <inheritdoc />
		public override Enum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> this.Deserialize(reader.GetString(), typeToConvert);

		internal Enum? Deserialize(string? value, Type enumerationType)
		{
			value = CleanEnumValue(value);
			var isNullable = enumerationType.IsGenericType && enumerationType.GetGenericTypeDefinition() == typeof(Nullable<>);
			if (String.IsNullOrWhiteSpace(value))
			{
				if (isNullable) return null;
				else
				{
					var defaultEnumerationValue = GetDefaultValue(enumerationType);
					if (defaultEnumerationValue is null) throw new JsonException($"Cannot convert the value '{value}' into a {enumerationType.Name}.");
					return defaultEnumerationValue as Enum;
				}
			}

			// If the target type is nullable, but the value is not null, than change the nullable for the underlying enumeration type.
			if (isNullable) enumerationType = enumerationType.GenericTypeArguments.First();

#if NET5_0_OR_GREATER
			if (Enum.TryParse(enumerationType, value, true, out var enumeration))
			{
#else
			if (TryParseEnum(value!, enumerationType, out var enumeration))
			{
#endif

				return enumeration as Enum;
			}
			else
			{
				throw new JsonException($"Cannot convert the value '{value}' into {enumerationType.Name}.");
			}
		}
		
		#endregion

		#region Serialization

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, Enum? enumeration, JsonSerializerOptions options)
		{
			if (enumeration is null) return;
			writer.WriteStringValue(this.Serialize(enumeration));
			if (_options.WriteOutOptions is ValueWriteOutAsComment commentOptions)
			{
				writer.WriteCommentValue(String.Join(commentOptions.Separator, GetEnumerationValues(enumeration.GetType())));
			}
			else if (_options.WriteOutOptions is ValueWriteOutAsProperty)
			{
				writer.WriteStartArray($"Values_for_{enumeration.GetType().Name}_{Random.Next(ushort.MinValue, ushort.MaxValue)}");
				foreach (var enumerationValue in GetEnumerationValues(enumeration.GetType()))
				{
					writer.WriteStringValue(enumerationValue);
				}
				writer.WriteEndArray();
			}
		}

		internal string? Serialize(Enum? enumeration)
		{
			if (enumeration is null) return null;
			var suffix = String.Empty;
			if (_options.WriteOutOptions is ValueWriteOutAsSuffix suffixOptions) suffix = $" {suffixOptions.Start}{String.Join(suffixOptions.Separator, GetEnumerationValues(enumeration.GetType()))}{suffixOptions.End}";
			return $"{enumeration}{suffix}";
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

		private static IReadOnlyCollection<string> GetEnumerationValues(Type enumerationType)
		{
			var enumValues = enumerationType.GetEnumValues();
			var stringValues = new List<string>(enumValues.Length);
			foreach (var enumValue in enumValues)
			{
				stringValues.Add(enumValue.ToString()!);
			}
			return stringValues.ToArray();
		}

		private static string? CleanEnumValue(string? value)
		{
			if (value == null) return null;

			// Since whitespaces are not allowed withing identifiers, this can be used to remove value write-out as suffix.
			//! Flags enumerations are serialized as a comma separated list containing whitespaces. Therefore a RegEx approach has been chosen.
			//			var separatorPosition = value.IndexOf(" ", StringComparison.OrdinalIgnoreCase);
			//			if (separatorPosition < 0) return value;
			//#if NET5_0_OR_GREATER
			//			return value[..separatorPosition];
			//#else
			//			return value.Substring(0, separatorPosition);
			//#endif
			return ValueCleanRegEx.Replace(value, String.Empty);
		}

		internal static object? GetDefaultValue(Type enumerationType)
		{
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
	}

	#endregion
}