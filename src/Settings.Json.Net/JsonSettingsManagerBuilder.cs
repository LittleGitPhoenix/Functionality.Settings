#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.IO;
using Phoenix.Functionality.Settings.Cache;

namespace Phoenix.Functionality.Settings.Json.Net
{
	#region Builder Interfaces

	/// <summary>
	/// Builder interface for <see cref="JsonSettingsManagerBuilder"/>.
	/// </summary>
	public interface IDirectoryJsonSettingsManagerBuilder
	{
		/// <summary>
		/// Use the default directory.
		/// </summary>
		ICacheJsonSettingsManagerBuilder UseDefaultDirectory();

		/// <summary>
		/// Use the directory at the provided path.
		/// </summary>
		ICacheJsonSettingsManagerBuilder WithDirectory(string settingsDirectoryPath);

		/// <summary>
		/// Use the provided directory.
		/// </summary>
		ICacheJsonSettingsManagerBuilder WithDirectory(DirectoryInfo settingsDirectory);
	}

	/// <summary>
	/// Builder interface for <see cref="JsonSettingsManagerBuilder"/>.
	/// </summary>
	public interface ICacheJsonSettingsManagerBuilder
	{
		/// <summary>
		/// Do not use caching.
		/// </summary>
		IJsonSettingsManagerCreator WithoutCache();

		/// <summary>
		/// Do use caching via <see cref="SettingsCache"/>.
		/// </summary>
		IJsonSettingsManagerCreator WithCache();

		/// <summary>
		/// Do use caching via <see cref="WeakSettingsCache"/>.
		/// </summary>
		IJsonSettingsManagerCreator WithWeakCache();
	}

	/// <summary>
	/// Builder interface for <see cref="JsonSettingsManagerBuilder"/>.
	/// </summary>
	public interface IJsonSettingsManagerCreator
	{
		/// <summary>
		/// Builds a new <see cref="JsonSettingsManager"/>.
		/// </summary>
		JsonSettingsManager Build();
	}

	#endregion

	/// <summary>
	/// Builder class for <see cref="JsonSettingsManager"/> instances.
	/// </summary>
	public class JsonSettingsManagerBuilder
		: IDirectoryJsonSettingsManagerBuilder,
			ICacheJsonSettingsManagerBuilder,
			IJsonSettingsManagerCreator
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private DirectoryInfo _settingsDirectory;

		private ISettingsCache _cache;

		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		internal JsonSettingsManagerBuilder()
		{
			// Save parameters.

			// Initialize fields.
			_settingsDirectory = null;
			_cache = null;
		}

		#endregion

		#region Methods
		#endregion

		#region Implementation of IDirectoryJsonSettingsManagerBuilder

		/// <inheritdoc />
		public ICacheJsonSettingsManagerBuilder UseDefaultDirectory()
		{
			return this;
		}

		/// <inheritdoc />
		public ICacheJsonSettingsManagerBuilder WithDirectory(string settingsDirectoryPath)
			=> this.WithDirectory(new DirectoryInfo(settingsDirectoryPath));
		
		/// <inheritdoc />
		public ICacheJsonSettingsManagerBuilder WithDirectory(DirectoryInfo settingsDirectory)
		{
			_settingsDirectory = settingsDirectory;
			return this;
		}

		#endregion

		#region Implementation of ICacheJsonSettingsManagerBuilder

		/// <inheritdoc />
		public IJsonSettingsManagerCreator WithoutCache()
		{
			_cache = new NoSettingsCache();
			return this;
		}

		/// <inheritdoc />
		public IJsonSettingsManagerCreator WithCache()
		{
			_cache = new SettingsCache();
			return this;
		}

		/// <inheritdoc />
		public IJsonSettingsManagerCreator WithWeakCache()
		{
			_cache = new WeakSettingsCache();
			return this;
		}

		#endregion

		#region Implementation of IJsonSettingsManagerCreator

		/// <inheritdoc />
		public JsonSettingsManager Build()
		{
			return new JsonSettingsManager(_settingsDirectory, _cache);
		}

		#endregion
	}
}