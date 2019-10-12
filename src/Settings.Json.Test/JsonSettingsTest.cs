using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phoenix.Functionality.Settings.Json.Net;

namespace Phoenix.Functionality.Settings.Json.Test
{
	[TestClass]
	public class JsonSettingsTest
	{
		#region Data

		class TestData
		{
			internal DirectoryInfo SettingsDirectory { get; }

			internal DirectoryInfo BackupDirectory { get; }

			internal Guid FixedGuid { get; }

			internal object Lock { get; }

			public TestData()
			{
				this.SettingsDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), ".settings"));
				this.BackupDirectory = new DirectoryInfo(Path.Combine(this.SettingsDirectory.FullName, ".backup"));
				this.FixedGuid = Guid.NewGuid();

				this.Lock = new object();
			}
		}

		enum TestEnum
		{
			Zero = 0,
			One = 1,
			Two = 2,
			Three = 3,
		}

		[SettingsFileName(nameof(ExistingSettings))]
		class ExistingSettings : ISettings
		{
			public int Numeric { get; set; }

			public TestEnum Enumeration { get; set; }

			public string Message { get; set; }

			public TimeSpan WaitTime { get; set; }

			public FileInfo File { get; set; }

			public Regex RegularExpression { get; set; }

			public IPAddress Ip { get; set; }

			public SecondNested SecondNested { get; set; } = new SecondNested();
		}

		[SettingsFileName(nameof(ModifiedSettings))]
		class ModifiedSettings : ISettings
		{
			public int Numeric { get; set; }

			public TestEnum Enumeration { get; set; }

			public string Message { get; set; }

			public TimeSpan WaitTime { get; set; }

			public Regex RegEx { get; set; }

			public DirectoryInfo Directory { get; set; }

			public SecondNested SecondNested { get; set; } = new SecondNested();
		}
		
		TestData Data { get; } = new TestData();

		#endregion

		#region Complex Tests
		
		[TestMethod]
		public void Load_Existing_Settings()
		{
			var settingsManager = new JsonSettingsManager();
			var settings = settingsManager.Load<ExistingSettings>();

			Assert.IsNotNull(settings);
			Assert.AreEqual(123, settings.Numeric);
			Assert.AreEqual("Some message", settings.Message);
			Assert.AreEqual(TestEnum.Three, settings.Enumeration);
			Assert.AreEqual(5000, settings.WaitTime.TotalMilliseconds);
			Assert.AreEqual(@"C:\no.file", settings.File.FullName);
			Assert.AreEqual(@".*?", settings.RegularExpression.ToString());
			Assert.AreEqual(RegexOptions.IgnoreCase | RegexOptions.Compiled, settings.RegularExpression.Options);
			Assert.AreEqual("192.168.0.1", settings.Ip.ToString());
			Assert.AreEqual(true, settings.SecondNested.Flag);
			Assert.AreEqual(new Guid("5805324d-9ca0-44f7-aa74-8dbb3375c129"), settings.SecondNested.VeryNested.Guid);
		}

		[TestMethod]
		public void Change_And_Save_Settings()
		{
			FileInfo settingsFile = null;

			try
			{
				settingsFile = new FileInfo(Path.Combine(Data.SettingsDirectory.FullName, $"{nameof(ModifiedSettings)}.json"));
				settingsFile.Delete();

				var settingsManager = new JsonSettingsManager();
				var settings = settingsManager.Load<ModifiedSettings>();

				settingsFile.Refresh();
				Assert.IsTrue(settingsFile.Exists);

				Assert.IsNotNull(settings);
				Assert.AreEqual(default, settings.Numeric);
				Assert.AreEqual(default, settings.Message);
				Assert.AreEqual(default, settings.Enumeration);
				Assert.AreEqual(default, settings.WaitTime.TotalMilliseconds);
				Assert.AreEqual(default, settings.RegEx);
				Assert.AreEqual(default, settings.Directory);
				Assert.AreEqual(default, settings.SecondNested.Flag);
				Assert.AreEqual(default, settings.SecondNested.VeryNested.Guid);

				var targetNumeric = 123;
				var targetMessage = "Some message";
				var targetEnum = TestEnum.One;
				var targetWaitTime = TimeSpan.FromSeconds(2);
				var targetRegEx = new Regex(pattern: ".*?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
				var targetDirectory = new DirectoryInfo(@"C:\");
				var targetFlag = true;
				var targetGuid = Guid.NewGuid();

				// Change all values.
				settings.Numeric = targetNumeric;
				settings.Message = targetMessage;
				settings.Enumeration = targetEnum;
				settings.WaitTime = targetWaitTime;
				settings.RegEx = targetRegEx;
				settings.Directory = targetDirectory;
				settings.SecondNested.Flag = targetFlag;
				settings.SecondNested.VeryNested.Guid = targetGuid;

				// Save the settings to file.
				settingsManager.Save(settings);

				// Reload the settings.
				settings = settingsManager.Load<ModifiedSettings>();

				Assert.IsNotNull(settings);
				Assert.AreEqual(targetNumeric, settings.Numeric);
				Assert.AreEqual(targetMessage, settings.Message);
				Assert.AreEqual(targetEnum, settings.Enumeration);
				Assert.AreEqual(targetWaitTime.TotalMilliseconds, settings.WaitTime.TotalMilliseconds);
				Assert.AreEqual(targetRegEx.ToString(), settings.RegEx.ToString());
				Assert.AreEqual(targetRegEx.Options, settings.RegEx.Options);
				Assert.AreEqual(targetDirectory.FullName, settings.Directory.FullName);
				Assert.AreEqual(targetFlag, settings.SecondNested.Flag);
				Assert.AreEqual(targetGuid, settings.SecondNested.VeryNested.Guid);
			}
			finally
			{
				settingsFile?.Delete();
			}
		}

		#endregion

		#region String

		[SettingsFileName(nameof(StringPropertySettings))]
		class StringPropertySettings : ISettings
		{
			public string String { get; set; } = "Test";
		}

		[TestMethod]
		public void Check_String_Property() => this.CheckSettings<StringPropertySettings>
		(
			settings =>
			{
				settings.String = "Changed";
			},
			settings =>
			{
				Assert.AreEqual("Changed", settings.String);
			}
		);

		#endregion

		#region Enumeration

		[SettingsFileName(nameof(EnumPropertySettings))]
		class EnumPropertySettings : ISettings
		{
			public TestEnum Enumeration { get; set; } = TestEnum.Two;
		}

		[TestMethod]
		public void Check_Enum_Property() => this.CheckSettings<EnumPropertySettings>
		(
			settings =>
			{
				settings.Enumeration = TestEnum.Three;
			},
			settings =>
			{
				Assert.AreEqual(TestEnum.Three, settings.Enumeration);
			}
		);

		#endregion

		#region Guid

		[SettingsFileName(nameof(GuidPropertySettings))]
		class GuidPropertySettings : ISettings
		{
			public Guid Guid { get; set; } = Guid.NewGuid();
		}

		[TestMethod]
		public void Check_Guid_Property() => this.CheckSettings<GuidPropertySettings>
		(
			settings =>
			{
				settings.Guid = this.Data.FixedGuid;
			},
			settings =>
			{
				Assert.AreEqual(this.Data.FixedGuid, settings.Guid);
			}
		);

		#endregion

		#region TimeSpan

		[SettingsFileName(nameof(TimeSpanPropertySettings))]
		class TimeSpanPropertySettings : ISettings
		{
			public TimeSpan WaitTime { get; set; } = TimeSpan.FromMilliseconds(5000);
		}

		[TestMethod]
		public void Check_TimeSpan_Property() => this.CheckSettings<TimeSpanPropertySettings>
			(
				settings =>
				{
					settings.WaitTime = TimeSpan.FromMilliseconds(ushort.MaxValue);
				},
				settings =>
				{
					Assert.AreEqual(TimeSpan.FromMilliseconds(ushort.MaxValue), settings.WaitTime);
				}
			);

		#endregion

		#region IPAddress

		[SettingsFileName(nameof(IpAddressPropertySettings))]
		class IpAddressPropertySettings : ISettings
		{
			public IPAddress Ip { get; set; } = IPAddress.Broadcast;
		}

		[TestMethod]
		public void Check_IPAddress_Property() => this.CheckSettings<IpAddressPropertySettings>
			(
				settings =>
				{
					settings.Ip = IPAddress.Loopback;
				},
				settings =>
				{
					Assert.AreEqual(IPAddress.Loopback.ToString(), settings.Ip.ToString());
				}
			);

		#endregion
		
		#region Regular Expression

		[SettingsFileName(nameof(RegexPropertySettings))]
		class RegexPropertySettings : ISettings
		{
			public Regex RegularExpression { get; set; } = new Regex(@".*?");
		}

		[TestMethod]
		public void Check_Regex_Property() => this.CheckSettings<RegexPropertySettings>
		(
			settings =>
			{
				settings.RegularExpression = new Regex(@"^Test: ?(?<Test>.*?) ?\| ?Value: ?(?<Value>\d*)$");
			},
			settings =>
			{
				Assert.IsTrue(settings.RegularExpression.Options.HasFlag(RegexOptions.Compiled));
				Assert.IsTrue(settings.RegularExpression.Options.HasFlag(RegexOptions.IgnoreCase));

				var match = settings.RegularExpression.Match(@"Test: RegEx | Value: 123");
				Assert.IsTrue(match.Groups["Test"].Success);
				Assert.IsTrue(match.Groups["Value"].Success);
			}
		);

		#endregion
		
		#region FileInfo

		[SettingsFileName(nameof(FileInfoPropertySettings))]
		class FileInfoPropertySettings : ISettings
		{
			public FileInfo File { get; set; } = new FileInfo(@"C:\not_existing_file");
		}

		[TestMethod]
		public void Check_FileInfo_Property() => this.CheckSettings<FileInfoPropertySettings>
		(
			settings =>
			{
				settings.File = this.GetSettingsFile<FileInfoPropertySettings>();
			},
			settings =>
			{
				Assert.IsTrue(settings.File.Exists);
			}
		);

		#endregion
		
		#region DiectoryInfo

		[SettingsFileName(nameof(DirectoryInfoPropertySettings))]
		class DirectoryInfoPropertySettings : ISettings
		{
			public DirectoryInfo Directory { get; set; } = new DirectoryInfo(@"C:\not_existing_folder");
		}

		[TestMethod]
		public void Check_DirectoryInfo_Property() => this.CheckSettings<DirectoryInfoPropertySettings>
		(
			settings =>
			{
				settings.Directory = this.Data.SettingsDirectory;
			},
			settings =>
			{
				Assert.IsTrue(settings.Directory.Exists);
			}
		);

		#endregion

		#region Array

		class ArrayPropertySettings : ISettings
		{
			public Guid[] Array { get; set; } = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
		}

		[TestMethod]
		public void Check_Array_Property() => this.CheckSettings<ArrayPropertySettings>
		(
			settings =>
			{
				settings.Array = new[] {this.Data.FixedGuid};
			},
			settings =>
			{
				Assert.AreEqual(1, settings.Array.Length);
				Assert.AreEqual(Data.FixedGuid, settings.Array.Last());
			}
		);

		#endregion

		#region List

		[SettingsFileName(nameof(ListPropertySettings))]
		class ListPropertySettings : ISettings
		{
			public List<Guid> List { get; set; } = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };
		}

		[TestMethod]
		public void Check_List_Property() => this.CheckSettings<ListPropertySettings>
		(
			settings =>
			{
				settings.List.Add(Guid.NewGuid());
				settings.List.Add(this.Data.FixedGuid);
			},
			settings =>
			{
				Assert.AreEqual(4, settings.List.Count); // Expected are only 4 values. The (de)serializer should not append values.
				Assert.AreEqual(Data.FixedGuid, settings.List.Last());
			}
		);

		#endregion

		#region Dictionary

		#region Key: String | Value: Primitve

		class StringPrimitiveDictionaryPropertySettings : ISettings
		{
			public Dictionary<string, string> Dictionary { get; set; } = new Dictionary<string, string>() { { "First", Guid.NewGuid().ToString() } };
		}

		[TestMethod]
		public void Check_StringPrimitiveDictionaryDictionary_Property() => this.CheckSettings<StringPrimitiveDictionaryPropertySettings>
		(
			settings =>
			{
				settings.Dictionary.Add("Second", this.Data.FixedGuid.ToString());
			},
			settings =>
			{
				Assert.AreEqual(2, settings.Dictionary.Count);
				Assert.AreEqual("Second", settings.Dictionary.Last().Key);
				Assert.AreEqual(Data.FixedGuid.ToString(), settings.Dictionary.Last().Value);
			}
		);

		#endregion

		#region Key: Non-String primitve | Value: Primitve

		class PrimitivePrimitiveDictionaryPropertySettings : ISettings
		{
			public Dictionary<int, string> Dictionary { get; set; } = new Dictionary<int, string>() { { int.MinValue, Guid.NewGuid().ToString() } };
		}

		[Ignore("Currently this is not supported by JsonSerializer: https://github.com/dotnet/corefx/issues/36024, https://github.com/dotnet/corefx/issues/37077")]
		[TestMethod]
		public void Check_NonStringPrimitivePrimitiveDictionary_Property() => this.CheckSettings<PrimitivePrimitiveDictionaryPropertySettings>
			(
				settings =>
				{
					settings.Dictionary.Add(Int32.MaxValue, this.Data.FixedGuid.ToString());
				},
				settings =>
				{
					Assert.AreEqual(2, settings.Dictionary.Count);
					Assert.AreEqual(int.MaxValue, settings.Dictionary.Last().Key);
					Assert.AreEqual(Data.FixedGuid.ToString(), settings.Dictionary.Last().Value);
				}
			);

		#endregion

		#endregion

		#region Nested

		[SettingsFileName(nameof(NestedPropertySettings))]
		class NestedPropertySettings : ISettings
		{
			public Nested Nested { get; set; } = new Nested();
		}

		[SettingsFileName(nameof(NestedPropertySettings))]
		class ModifiedNestedPropertySettings : NestedPropertySettings
		{
			public SecondNested SecondNested { get; set; } = new SecondNested();
		}

		class Nested
		{
			public bool Flag { get; set; } = true;
		}

		class SecondNested
		{
			public bool Flag { get; set; }

			public VeryNested VeryNested { get; set; } = new VeryNested();
		}

		class VeryNested
		{
			public Guid Guid { get; set; }
		}

		[TestMethod]
		public void Check_Nested_Property()
		{
			var settingsFile = this.GetSettingsFile<NestedPropertySettings>();
			try
			{
				// Always delete any existing settings file beforehand.
				this.DeleteSettingsFile(settingsFile);

				// New:
				this.LoadAsNew<NestedPropertySettings>();

				// Existing:
				this.LoadAsExisting<NestedPropertySettings>();

				// Modified:
				var settings = this.LoadAsExisting<ModifiedNestedPropertySettings>();
			}
			finally
			{
				this.DeleteSettingsFile(settingsFile);
			}
		}

		#endregion
		
		#region Validation

		private void CheckSettings<TSettings>(Action<TSettings> modifyCallback, Action<TSettings> validateCallback) where TSettings : class, ISettings, new()
		{
			var settingsFile = this.GetSettingsFile<TSettings>();
			try
			{
				// Always delete any existing settings file beforehand.
				this.DeleteSettingsFile(settingsFile);

				// New:
				this.LoadAsNew<TSettings>();

				// Existing:
				var settings = this.LoadAsExisting<TSettings>();
			
				// Modified:
				this.LoadAsModified(settings, modifyCallback, validateCallback);
			}
			finally
			{
				this.DeleteSettingsFile(settingsFile);
			}
		}

		private void LoadAsNew<TSettings>() where TSettings : class, ISettings, new()
		{
			try
			{
				// Load the settings.
				var settings = this.LoadSettings<TSettings>();

				// Check if a new settings file has been created.
				var settingsFile = this.GetSettingsFile<TSettings>();
				Assert.IsTrue(settingsFile.Exists);

				// Compare the file to the settings instance.
				this.CompareSettingsFileToInstance(settingsFile, settings);
			}
			finally
			{
				this.DeleteBackupDirectory();
			}
		}

		private TSettings LoadAsExisting<TSettings>() where TSettings : class, ISettings, new()
		{
			try
			{
				// Check if the settings file already exists.
				var settingsFile = this.GetSettingsFile<TSettings>();
				Assert.IsTrue(settingsFile.Exists);

				// Load the settings.
				var settings = this.LoadSettings<TSettings>();

				// Compare the file to the settings instance.
				this.CompareSettingsFileToInstance(settingsFile, settings);

				return settings;
			}
			finally
			{
				this.DeleteBackupDirectory();
			}
		}

		private void LoadAsModified<TSettings>(TSettings settings, Action<TSettings> modifyCallback, Action<TSettings> validateCallback) where TSettings : class, ISettings, new()
		{
			try
			{
				// Modify the settings instance.
				modifyCallback.Invoke(settings);

				// Save it.
				this.SaveSettings(settings);

				// Re-Load the settings.
				settings = this.LoadSettings<TSettings>();

				validateCallback.Invoke(settings);
			}
			finally
			{
				this.DeleteBackupDirectory();
			}
		}

		#region Helper

		private FileInfo GetSettingsFile<TSettings>() where TSettings : ISettings
		{
			var settingsManager = new JsonSettingsManager();
			var settingsFileName = settingsManager.GetSettingsFileNameWithoutExtension<TSettings>() + ".json";

			return new FileInfo(Path.Combine(Data.SettingsDirectory.FullName, settingsFileName));
		}

		private void DeleteSettingsFile(FileInfo settingsFile)
		{
			settingsFile.Refresh();
			if (settingsFile.Exists)
			{
				settingsFile.Delete();
				settingsFile.Refresh();
			}
			Assert.IsFalse(settingsFile.Exists);
		}

		private TSettings LoadSettings<TSettings>() where TSettings : class, ISettings, new()
		{
			var settingsManager = new JsonSettingsManager();
			var settings = settingsManager.Load<TSettings>();
			Assert.IsNotNull(settings);

			return settings;
		}
		
		private void SaveSettings<TSettings>(TSettings settings) where TSettings : class, ISettings, new()
		{
			var settingsManager = new JsonSettingsManager();
			settingsManager.Save(settings);

			// Check if a settings exists.
			var settingsFile = this.GetSettingsFile<TSettings>();
			Assert.IsTrue(settingsFile.Exists);
		}
		
		private void CompareSettingsFileToInstance<TSettings>(FileInfo settingsFile, TSettings settings) where TSettings : class, ISettings, new()
		{
			var areIdentical = !JsonSettingsManager.ShouldSettingsFileBeUpdatedAsync(settingsFile, settings).Result;
			Assert.IsTrue(areIdentical);
		}

		private void DeleteBackupDirectory()
		{
			lock (Data.Lock)
			{
				try
				{
					if (Data.BackupDirectory.Exists) Data.BackupDirectory.Delete(true);
				}
				catch { /* ignore */ }
			}
		}

		#endregion

		#endregion
	}
}