#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;

//namespace Phoenix.Functionality.Settings.Json.Newtonsoft
//{
//	/// <summary>
//	/// Converter used for implicit conversion of values to a concrete type.
//	/// </summary>
//	public class TypeValueConverter
//	{
//		/// <summary>
//		/// Tries to convert <paramref name="value"/> into <paramref name="targetType"/>.
//		/// </summary>
//		/// <param name="value"> The value to convert. </param>
//		/// <param name="targetType"> The target <see cref="Type"/>. </param>
//		/// <returns> A new instance of the target type build from the value. </returns>
//		public object ConvertValue(object value, Type targetType)
//		{

//			if (value is string stringValue) return this.ConvertValue(stringValue, targetType);
//			if (value is int intValue) return this.ConvertValue(intValue, targetType);
//			if (value is long longValue) return this.ConvertValue(longValue, targetType);
//			if (value is IDictionary<string, object> expando) return this.ConvertValue(expando, targetType);

//			//// Check for dictionaries.
//			//var valueType = value.GetType();
//			//if (valueType.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(valueType.GetGenericTypeDefinition()))

//			return value;
//		}

//		object ConvertValue(string value, Type targetType)
//		{
//			if (targetType == typeof(Guid))
//			{
//				return new Guid(value);
//			}
//			if (targetType == typeof(TimeSpan))
//			{
//				var values = value.Split(':').Select(part => int.TryParse(part, out var numeric)? numeric : 0).Take(5).ToArray();

//				// In case the values contain only three elements, then copy those values at index 1 to the destination array. THis is needed because of the special TimeSpan constructor for three parameters.
//				var numerics = new int[5];
//				values.CopyTo(numerics, index: values.Length == 3 ? 1:0);

//				return new TimeSpan(numerics[0], numerics[1], numerics[2], numerics[3], numerics[4]);
//			}
//			if (targetType.IsEnum)
//			{
//				return Enum.Parse(targetType, value, true);
//			}
//			if (targetType == typeof(FileInfo))
//			{
//				return new FileInfo(value);
//			}
//			if (targetType == typeof(DirectoryInfo))
//			{
//				return new DirectoryInfo(value);
//			}
//			if (targetType == typeof(Regex))
//			{
//				return new Regex(value, options: RegexOptions.IgnoreCase | RegexOptions.Compiled);
//			}

//			return value;
//		}

//		object ConvertValue(int value, Type targetType)
//		{
//			if (targetType == typeof(TimeSpan)) return TimeSpan.FromMilliseconds(value);
//			return value;
//		}

//		object ConvertValue(long value, Type targetType)
//		{
//			if (targetType == typeof(TimeSpan)) return TimeSpan.FromMilliseconds(value);
//			return value;
//		}

//		object ConvertValue(IDictionary<string, object> expando, Type targetType)
//		{
//			if
//			(
//				targetType == typeof(Regex)

//				&& expando.TryGetValue("Pattern", out var patternObject)
//				&& patternObject is string pattern
//				&& expando.TryGetValue("Options", out var optionsObject)
//				&& optionsObject is string optionsString
//				&& Enum.TryParse<RegexOptions>(optionsString, true, out var options)
//			)
//			{
//				return new Regex(pattern, options);
//			}
//			return expando;
//		}
//	}
//}