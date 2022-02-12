using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Settings;
using Phoenix.Functionality.Settings.Sinks.File;

namespace Settings.Sinks.File.Test;

public class FileSettingsSinkTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[OneTimeSetUp]
	public void BeforeAllTests() { }

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());

		var targetWorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase?.TrimEnd(Path.DirectorySeparatorChar);
		var actualWorkingDirectory = Directory.GetCurrentDirectory().TrimEnd(Path.DirectorySeparatorChar);
		if (targetWorkingDirectory != actualWorkingDirectory)
		{
			/*
			! If running under .NET Framework 4.8, the working directory is not the build directory, but rather the solution root.
			! Therefor the existing settings file won't be found and the test will fail.
			*/
			Console.WriteLine($"The working directory '{actualWorkingDirectory}' mismatches its expectation '{targetWorkingDirectory}'. It has been changed for the unit test to properly run.");
			Directory.SetCurrentDirectory(targetWorkingDirectory);
		}

		////! This has to be done AFTER the working directory may have been changed.
		//this.Data = new TestData();
	}

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTest() { }

	#endregion

	#region Data

	[SettingsName("Settings")]
	// ReSharper disable once ClassNeverInstantiated.Local → Only the type is used for unit tests.
	class Settings : ISettings { }

	#endregion

	#region Retrieve

	[Test]
	public void Retrieve_Settings_Data_Is_Null_If_No_File_Exists()
	{
		// Arrange
		using var testDirectory = new TestDirectory();
		var sink = new FileSettingsSink(testDirectory);

		// Act
		var settingsData = sink.Retrieve<Settings>();

		// Assert
		Assert.Null(settingsData);
	}
	
	[Test]
	public void Retrieve_Settings_Data_Is_Not_Null_If_File_Exists()
	{
		// Arrange
		using var testDirectory = new TestDirectory();
		var content = Guid.NewGuid().ToString();
		testDirectory.CreateFile($"{nameof(Settings)}.json", content);
		var sink = new FileSettingsSink(".json", testDirectory);

		// Act
		var settingsData = sink.Retrieve<Settings>();

		// Assert
		Assert.That(settingsData, Is.EqualTo(content));
	}

	[Test]
	[Ignore("The implementation of the 'FileSettingsSink.Retrieve' function cannot be made to throw because it does not contain custom code.")]
	public void Retrieve_Wraps_Exceptions()
	{
		// Arrange

		// Act + Assert
		//Assert.Catch<SettingsLoadException>(() => );
	}

	#endregion

	#region Store

	[Test]
	public void Store_Succeeds()
	{
		// Arrange
		using var testDirectory = new TestDirectory();
		var content = Guid.NewGuid().ToString();
		_fixture.Inject(testDirectory.Directory);
		var mockSink = _fixture.Create<Mock<FileSettingsSink>>();
		mockSink
			.Setup(mock => mock.CreateBackup(It.IsAny<FileInfo>()))
			.Verifiable()
			;
		mockSink
			.Setup(mock => mock.DeleteExistingSettingsFile(It.IsAny<FileInfo>()))
			.Verifiable()
			;
		var sink = mockSink.Object;

		// Act
		sink.Store<Settings>(content);

		// Assert
		var settingsFile = testDirectory.Directory.EnumerateFileSystemInfos().Single();
		Assert.NotNull(settingsFile);
		mockSink.Verify(mock => mock.CreateBackup(It.IsAny<FileInfo>()), Times.Never());
		mockSink.Verify(mock => mock.DeleteExistingSettingsFile(It.IsAny<FileInfo>()), Times.Never());
	}

	[Test]
	public void Store_Creates_Backup()
	{
		// Arrange
		using var testDirectory = new TestDirectory();
		var content = Guid.NewGuid().ToString();
		_fixture.Inject(testDirectory.Directory);
		var mockSink = _fixture.Create<Mock<FileSettingsSink>>();
		mockSink
			.Setup(mock => mock.CreateBackup(It.IsAny<FileInfo>()))
			.Verifiable()
			;
		var sink = mockSink.Object;

		// Act
		sink.Store<Settings>(content, true);

		// Assert
		mockSink.Verify(mock => mock.CreateBackup(It.IsAny<FileInfo>()), Times.Once());
	}

	[Test]
	public void Store_Creates_Identical_Backup_Files()
	{
		// Arrange
		using var testDirectory = new TestDirectory();
		var backupDirectory = new DirectoryInfo(Path.Combine(testDirectory.Directory.FullName, FileSettingsSink.DefaultBackupFolderName));
		var content = Guid.NewGuid().ToString();
		var settingsFile = testDirectory.CreateFile($"{nameof(Settings)}.json", content);
		var sink = new FileSettingsSink(testDirectory);

		// Act
		sink.CreateBackup(settingsFile);
		backupDirectory.Refresh();

		// Assert
		Assert.True(backupDirectory.Exists);
		var backupFile = backupDirectory.EnumerateFiles().Single();
		var backupContent = System.IO.File.ReadAllBytes(backupFile.FullName);
		Assert.AreEqual(content, backupContent);
	}

	/// <summary> Checks that if the settings data is empty, the underlying file would be deleted. This just checks if the <see cref="FileSettingsSink.DeleteExistingSettingsFile"/> method is invoked. </summary>
	[Test]
	public void Store_Calls_Delete()
	{
		// Arrange
		using var testDirectory = new TestDirectory();
		_fixture.Inject(testDirectory.Directory);
		var mockSink = _fixture.Create<Mock<FileSettingsSink>>();
		mockSink
			.Setup(mock => mock.DeleteExistingSettingsFile(It.IsAny<FileInfo>()))
			.Verifiable()
			;
		var sink = mockSink.Object;

		// Act
		sink.Store<Settings>(String.Empty);

		// Assert
		mockSink.Verify(mock => mock.DeleteExistingSettingsFile(It.IsAny<FileInfo>()), Times.Once());
	}

	/// <summary> Checks that <see cref="FileSettingsSink.DeleteExistingSettingsFile"/> deleted files successfully. </summary>
	[Test]
	public void Store_Delete_Call_Removes_File()
	{
		// Arrange
		using var testDirectory = new TestDirectory();
		var content = Guid.NewGuid().ToString();
		var settingsFile = testDirectory.CreateFile($"{nameof(Settings)}.json", content);
		var sink = new FileSettingsSink(testDirectory);

		// Act
		Assert.True(settingsFile.Exists);
		sink.DeleteExistingSettingsFile(settingsFile);
		settingsFile.Refresh();

		// Assert
		Assert.False(settingsFile.Exists);
	}

	[Test]
	public void Store_Wraps_Exceptions()
	{
		// Arrange
		using var testDirectory = new TestDirectory();
		var content = Guid.NewGuid().ToString();
		_fixture.Inject(testDirectory.Directory);
		var mockSink = _fixture.Create<Mock<FileSettingsSink>>();
		mockSink
			.Setup(mock => mock.CreateBackup(It.IsAny<FileInfo>()))
			.Throws<Exception>()
			.Verifiable()
			;
		var sink = mockSink.Object;

		// Act + Assert
		Assert.Catch<SettingsSaveException>(() => sink.Store<Settings>(content, true));
	}

	#endregion
}