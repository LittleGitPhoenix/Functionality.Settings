#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.Settings.Sinks.File;

/// <summary>
/// <see cref="ISettingsSink"/> that stores <see cref="ISettings"/> as a file to a file system.
/// </summary>
public class FileSettingsSink : ISettingsSink<string>
{
	#region Delegates / Events
	#endregion

	#region Constants

	/// <summary> The default file extensions of settings files. </summary>
	public const string DefaultSettingsFileExtension = ".settings";

	/// <summary> The name of the default settings folder. </summary>
	public const string DefaultSettingsFolderName = ".settings";
	
	/// <summary> The name of the default backup folder. </summary>
	public const string DefaultBackupFolderName = ".backup";

	#endregion

	#region Fields

	/// <summary> The file extension of settings files. </summary>
	private readonly string _fileExtension;

	/// <summary>
	/// The directory where all setting files reside.
	/// </summary>
	/// <remarks> By default this is a folder named <see cref="DefaultSettingsFolderName"/> in <see cref="Directory.GetCurrentDirectory"/>. </remarks>
	private readonly DirectoryInfo _baseDirectory;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="baseDirectory"> Optional directory where all setting files reside. Default will be a folder named <see cref="DefaultSettingsFolderName"/> in <see cref="Directory.GetCurrentDirectory"/>. </param>
	public FileSettingsSink(DirectoryInfo? baseDirectory = null)
		: this(null, baseDirectory) {}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="fileExtension"> Optional file extension of settings files. Default is <see cref="DefaultSettingsFileExtension"/>. </param>
	/// <param name="baseDirectory"> Optional directory where all setting files reside. Default will be a folder named <see cref="DefaultSettingsFolderName"/> in <see cref="Directory.GetCurrentDirectory"/>. </param>
	public FileSettingsSink(string? fileExtension = null, DirectoryInfo? baseDirectory = null)
	{
		// Save parameters.
		_fileExtension = $".{(fileExtension ?? DefaultSettingsFileExtension).TrimStart('.')}";
		_baseDirectory = baseDirectory ?? new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), DefaultSettingsFolderName));

		// Initialize fields.


		// Create the settings base folder.
		_baseDirectory.Create();
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public string? Retrieve<TSettings>(bool throwIfNoDataIsAvailable = false) where TSettings : ISettings
	{
		var settingsFile = this.GetSettingsFile(this.GetSettingsFileNameWithoutExtension<TSettings>(), _baseDirectory);
		if (!settingsFile.Exists) return throwIfNoDataIsAvailable ? throw new SettingsLoadNoDataAvailableException() : null;
		try
		{
			using var stream = settingsFile.OpenRead();
			using var reader = new StreamReader(stream);
			var content = reader.ReadToEnd();
			return content;
		}
		catch (Exception ex)
		{
			throw new SettingsLoadException($"Could not load the settings data from file '{settingsFile.FullName}'. See the inner exception for further details.", ex);
		}
	}

	/// <inheritdoc />
	public void Store<TSettings>(string settingsData, bool createBackup = default) where TSettings : ISettings
	{
		var settingsFile = this.GetSettingsFile(this.GetSettingsFileNameWithoutExtension<TSettings>(), _baseDirectory);
		try
		{
			// Create a backup.
			if (createBackup) this.CreateBackup(settingsFile);

			// Delete existing files if there is no data.
			if (String.IsNullOrWhiteSpace(settingsData))
			{
				this.DeleteExistingSettingsFile(settingsFile);
				return;
			}

			// Check if the directory of the file exists. Trying to create a file, whose directory is also not existing, will throw an exception.
			if (!settingsFile.Directory!.Exists) settingsFile.Directory.Create();

			using var fileStream = settingsFile.Open(FileMode.Create);
			var data = System.Text.Encoding.UTF8.GetBytes(settingsData);
			fileStream.Write(data, offset: 0, count: data.Length);
		}
		catch (Exception ex)
		{
			throw new SettingsSaveException($"Could save settings data to file '{settingsFile.FullName}'. See the inner exception for further details.", ex);
		}
	}

	#region Helper

	/// <summary>
	/// Gets a reference to the settings file.
	/// </summary>
	/// <param name="settingsName"> The name of the settings file. </param>
	/// <param name="settingsDirectory"> The base directory of the settings. </param>
	/// <returns> A <see cref="FileInfo"/> reference to the settings file. </returns>
	private FileInfo GetSettingsFile(string settingsName, DirectoryInfo settingsDirectory)
	{
		var fileName = $"{settingsName}{_fileExtension}";
		return new FileInfo(Path.Combine(settingsDirectory.FullName, fileName));
	}

	/// <summary>
	/// Deletes the <paramref name="settingsFile"/> if it exists.
	/// </summary>
	/// <param name="settingsFile"> The settings file to delete. </param>
	internal virtual void DeleteExistingSettingsFile(FileInfo settingsFile)
	{
		if (settingsFile.Exists) settingsFile.Delete();
	}

	/// <summary>
	/// Creates a backup of the <paramref name="settingsFile"/>. The backup will be suffixed by 'yyyy-MM-dd_HH-mm-ss'. Existing backup files will be overridden.
	/// </summary>
	/// <param name="settingsFile"> The settings file of which to create a backup. </param>
	internal virtual void CreateBackup(FileInfo settingsFile)
	{
		settingsFile.Refresh();
		if (settingsFile.Exists)
		{
			// Create a backup of the current file.
			var backupFolder = Path.Combine(settingsFile.DirectoryName!, DefaultBackupFolderName);
			var backupFile = $"{Path.GetFileNameWithoutExtension(settingsFile.Name)}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}{settingsFile.Extension}";

			Directory.CreateDirectory(backupFolder);
			settingsFile.CopyTo(Path.Combine(backupFolder, backupFile), overwrite: true);
		}
	}

	#endregion

	#endregion
}