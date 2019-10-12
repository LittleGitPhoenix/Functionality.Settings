//using System;
//using System.Collections.Generic;
//using System.Dynamic;
//using System.IO;
//using System.Text.RegularExpressions;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Phoenix.Functionality.Settings.Json.Test
//{
//	[TestClass]
//	public class TypeValueConverterTest
//	{
//		[TestMethod]
//		public void Convert_Guid()
//		{
//			var converter = new TypeValueConverter();

//			var targetType = typeof(Guid);
//			object targetValue = Guid.NewGuid().ToString();

//			var convertedObject = converter.ConvertValue(targetValue, targetType);
//			Assert.AreEqual(targetType, convertedObject.GetType());
//			Assert.AreEqual(targetValue, convertedObject.ToString());
//		}

//		[TestMethod]
//		public void Convert_TimeSpan_From_Int()
//		{
//			var converter = new TypeValueConverter();

//			var targetType = typeof(TimeSpan);
//			object targetValue = (int) 5000;

//			var convertedObject = converter.ConvertValue(targetValue, targetType);
//			Assert.AreEqual(targetType, convertedObject.GetType());
//			Assert.AreEqual(targetValue, (int)((TimeSpan) convertedObject).TotalMilliseconds);
//		}

//		[TestMethod]
//		public void Convert_TimeSpan_From_Long()
//		{
//			var converter = new TypeValueConverter();

//			var targetType = typeof(TimeSpan);
//			object targetValue = (long) 5000;

//			var convertedObject = converter.ConvertValue(targetValue, targetType);
//			Assert.AreEqual(targetType, convertedObject.GetType());
//			Assert.AreEqual(targetValue, (long) ((TimeSpan) convertedObject).TotalMilliseconds);
//		}

//		[TestMethod]
//		public void Convert_TimeSpan_From_3_Part_String()
//		{
//			var converter = new TypeValueConverter();

//			var targetType = typeof(TimeSpan);
//			object targetValue = "00:00:03";

//			var convertedObject = converter.ConvertValue(targetValue, targetType);
//			Assert.AreEqual(targetType, convertedObject.GetType());
//			Assert.AreEqual(3000, (int) ((TimeSpan) convertedObject).TotalMilliseconds);
//		}

//		[TestMethod]
//		public void Convert_TimeSpan_From_4_Part_String()
//		{
//			var converter = new TypeValueConverter();

//			var targetType = typeof(TimeSpan);
//			object targetValue = "00:00:00:03";

//			var convertedObject = converter.ConvertValue(targetValue, targetType);
//			Assert.AreEqual(targetType, convertedObject.GetType());
//			Assert.AreEqual(3000, (int) ((TimeSpan) convertedObject).TotalMilliseconds);
//		}

//		[TestMethod]
//		public void Convert_TimeSpan_From_5_Part_String()
//		{
//			var converter = new TypeValueConverter();

//			var targetType = typeof(TimeSpan);
//			object targetValue = "00:00:00:03:200";

//			var convertedObject = converter.ConvertValue(targetValue, targetType);
//			Assert.AreEqual(targetType, convertedObject.GetType());
//			Assert.AreEqual(3200, (int) ((TimeSpan) convertedObject).TotalMilliseconds);
//		}

//		[TestMethod]
//		public void Convert_FileInfo()
//		{
//			var converter = new TypeValueConverter();

//			var targetType = typeof(FileInfo);
//			object targetValue = $@"C:\{Guid.NewGuid()}.txt";

//			var convertedObject = converter.ConvertValue(targetValue, targetType);
//			Assert.AreEqual(targetType, convertedObject.GetType());
//			Assert.AreEqual(targetValue, ((FileInfo) convertedObject).FullName);
//		}

//		[TestMethod]
//		public void Convert_DirectoryInfo()
//		{
//			var converter = new TypeValueConverter();

//			var targetType = typeof(DirectoryInfo);
//			object targetValue = $@"C:\{Guid.NewGuid()}";

//			var convertedObject = converter.ConvertValue(targetValue, targetType);
//			Assert.AreEqual(targetType, convertedObject.GetType());
//			Assert.AreEqual(targetValue, ((DirectoryInfo) convertedObject).FullName);
//		}

//		[TestMethod]
//		public void Convert_RegEx_From_String()
//		{
//			var converter = new TypeValueConverter();

//			var targetType = typeof(Regex);
//			object targetValue = @".*?";

//			var convertedObject = converter.ConvertValue(targetValue, targetType);
//			Assert.AreEqual(targetType, convertedObject.GetType());
//			Assert.AreEqual(targetValue, ((Regex) convertedObject).ToString());
//			Assert.AreEqual(RegexOptions.IgnoreCase | RegexOptions.Compiled, ((Regex) convertedObject).Options);
//		}

//		[TestMethod]
//		public void Convert_RegEx_From_Expando()
//		{
//			var converter = new TypeValueConverter();

//			var targetType = typeof(Regex);
//			var targetPattern = ".*?";
//			var targetOptions = RegexOptions.CultureInvariant;
			
//			IDictionary<string, Object> targetValue = new ExpandoObject();
//			targetValue.Add("Pattern", targetPattern);
//			targetValue.Add("Options", targetOptions.ToString());

//			var convertedObject = converter.ConvertValue(targetValue, targetType);
//			Assert.AreEqual(targetType, convertedObject.GetType());
//			Assert.AreEqual(targetPattern, ((Regex) convertedObject).ToString());
//			Assert.AreEqual(targetOptions, ((Regex) convertedObject).Options);
//		}
//	}
//}