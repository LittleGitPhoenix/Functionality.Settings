using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Phoenix.Functionality.Settings.Json.Net.CustomJsonConverters;

namespace Phoenix.Functionality.Settings.Json.Net.Test
{
	public class TimeSpanConverterTest
	{
		[Test]
		public void Deserialize_TimeSpan_From_Numeric_Succeeds()
		{
			// Arrange
			var converter = new TimeSpanConverter();
			var targetMilliseconds = 5000;

			// Act
			var success = converter.TryDeserialize(targetMilliseconds, out var timeSpan);
			
			// Assert
			Assert.True(success);
			Assert.AreEqual(timeSpan.TotalMilliseconds, targetMilliseconds);
		}

		[Test]
		public void Deserialize_TimeSpan_From_String_Succeeds()
		{
			// Arrange
			var converter = new TimeSpanConverter();
			var targetMilliseconds = 5000;
			var value = $"{targetMilliseconds}";

			// Act
			var success = converter.TryDeserialize(value, out var timeSpan);
			
			// Assert
			Assert.True(success);
			Assert.AreEqual(timeSpan.TotalMilliseconds, targetMilliseconds);
		}

		[Test]
		public void Serialize_TimeSpan_Into_Numeric_Succeeds()
		{
			// Arrange
			var converter = new TimeSpanConverter();
			var targetMilliseconds = 5000;
			var timeSpan = TimeSpan.FromMilliseconds(targetMilliseconds);

			// Act
			var success = converter.TrySerialize(timeSpan, out long numeric);

			// Assert
			Assert.True(success);
			Assert.AreEqual(numeric, targetMilliseconds);
		}

		[Test]
		public void Serialize_TimeSpan_Into_String_Succeeds()
		{
			// Arrange
			var converter = new TimeSpanConverter();
			var targetMilliseconds = 5000;
			var targetValue = $"{targetMilliseconds}";
			var timeSpan = TimeSpan.FromMilliseconds(targetMilliseconds);
			
			// Act
			var success = converter.TrySerialize(timeSpan, out string value);

			// Assert
			Assert.True(success);
			Assert.AreEqual(value, targetValue);
		}
		
		//[Test]
		//public void Convert_RegEx_From_String()
		//{
		//	var converter = new TypeValueConverter();

		//	var targetType = typeof(Regex);
		//	object targetValue = @".*?";

		//	var convertedObject = converter.ConvertValue(targetValue, targetType);
		//	Assert.AreEqual(targetType, convertedObject.GetType());
		//	Assert.AreEqual(targetValue, ((Regex)convertedObject).ToString());
		//	Assert.AreEqual(RegexOptions.IgnoreCase | RegexOptions.Compiled, ((Regex)convertedObject).Options);
		//}

		//[Test]
		//public void Convert_RegEx_From_Expando()
		//{
		//	var converter = new TypeValueConverter();

		//	var targetType = typeof(Regex);
		//	var targetPattern = ".*?";
		//	var targetOptions = RegexOptions.CultureInvariant;

		//	IDictionary<string, Object> targetValue = new ExpandoObject();
		//	targetValue.Add("Pattern", targetPattern);
		//	targetValue.Add("Options", targetOptions.ToString());

		//	var convertedObject = converter.ConvertValue(targetValue, targetType);
		//	Assert.AreEqual(targetType, convertedObject.GetType());
		//	Assert.AreEqual(targetPattern, ((Regex)convertedObject).ToString());
		//	Assert.AreEqual(targetOptions, ((Regex)convertedObject).Options);
		//}
	}
}