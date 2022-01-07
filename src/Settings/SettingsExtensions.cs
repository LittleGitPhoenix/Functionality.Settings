#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Phoenix.Functionality.Settings.Cache;

namespace Phoenix.Functionality.Settings;

/// <summary>
/// Extension methods for <see cref="ISettings"/>.
/// </summary>
public static class SettingsExtensions
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	/// <summary>
	/// Cache of all <see cref="ISettingsManager"/> mapped to the concrete <see cref="ISettings"/> type that where loaded with it.
	/// </summary>
	internal static readonly ConcurrentDictionary<Type, ISettingsManager> Cache;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	static SettingsExtensions()
	{
		// Save parameters.

		// Initialize fields.
		Cache = new ConcurrentDictionary<Type, ISettingsManager>();
	}

	#endregion

	#region Methods

	/// <summary>
	/// 
	/// </summary>
	/// <param name="settings"> The extended <see cref="ISettings"/> instance. </param>
	/// <param name="settingsManager"> The <see cref="ISettingsManager"/> that was used to load the <paramref name="settings"/> and is needed for (most) of the extension methods for <see cref="ISettings"/> of this class. </param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void InitializeExtensionMethods(this ISettings settings, ISettingsManager settingsManager)
	{
		SettingsExtensions.Cache.AddOrUpdate(settings.GetType(), settingsManager, (_, _) => settingsManager);
	}

	/// <summary>
	/// Reloads the settings inferred from the type of the passed <paramref name="settings"/> instance and returns a new instance.
	/// </summary>
	/// <typeparam name="TSettings"> The concrete type of the <paramref name="settings"/>. </typeparam>
	/// <param name="settings"> The extended <see cref="ISettings"/> instance. </param>
	/// <param name="preventUpdate"> This prevents the <see cref="ISettingsManager"/> used for reloading from updating the underlying data source of the settings instance in case later differs from it. </param>
	/// <returns> A new instance of <typeparamref name="TSettings"/>. </returns>
	/// <exception cref="MissingSettingsManagerException"> Thrown if the <paramref name="settings"/> instance is not linked to a cached <see cref="ISettingsManager"/>. Use the hidden <see cref="InitializeExtensionMethods"/> method for initialization. </exception>
	/// <remarks>
	/// <para> Any <see cref="ISettingsCache"/> will be ignored. </para>
	/// <para> This extension method will only work for <see cref="ISettings"/> instances that called the <see cref="InitializeExtensionMethods"/> method so that the <see cref="ISettingsManager"/> that was used to load the instance has been cached internally and can be used for saving. </para>
	/// </remarks>
	public static TSettings Reload<TSettings>(this TSettings settings, bool preventUpdate = false)
		where TSettings : class, ISettings, new()
	{
		if (!SettingsExtensions.Cache.TryGetValue(typeof(TSettings), out var settingsManager))
		{
			throw new MissingSettingsManagerException(settings);
		}

		return settingsManager.Load<TSettings>(true, preventUpdate);
	}

	/// <inheritdoc cref="ISettingsManager.Save{TSettings}"/>
	/// <exception cref="MissingSettingsManagerException"> Thrown if the <paramref name="settings"/> instance is not linked to a cached <see cref="ISettingsManager"/>. Use the hidden <see cref="InitializeExtensionMethods{TSettings}"/> method for initialization. </exception>
	/// <remarks> This extension method will only work for <see cref="ISettings"/> instances that called the <see cref="InitializeExtensionMethods"/> method so that the <see cref="ISettingsManager"/> that was used to load the instance has been cached internally and can be used for saving. </remarks>
	public static void Save(this ISettings settings, bool createBackup = default)
	{
		if (!SettingsExtensions.Cache.TryGetValue(settings.GetType(), out var settingsManager))
		{
			throw new MissingSettingsManagerException(settings);
		}

		settingsManager.Save(settings, createBackup);
	}

	#endregion
}

/// <summary>
/// Base exception for errors of extension methods of <see cref="ISettings"/>.
/// </summary>
public abstract class SettingsExtensionMethodException : Exception
{
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="message"> The exception message. </param>
	protected SettingsExtensionMethodException(string message) : base(message) { }
}

/// <summary>
/// <para> Exception thrown when invoking either of the following extension methods without the <see cref="ISettings"/> instance used for those methods hasn't been loaded via an <see cref="ISettingsManager"/>. </para>
/// <para> • <see cref="SettingsExtensions.Reload{TSettings}"/> </para>
/// <para> • <see cref="SettingsExtensions.Save"/> </para>
/// </summary>
public sealed class MissingSettingsManagerException : SettingsExtensionMethodException
{
	/// <summary> The settings instance. </summary>
	public ISettings Settings { get; }
		
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="settings"> <see cref="Settings"/> </param>
	/// <param name="methodName"> The name of the method that raised the exception. Automatically filled via <see cref="CallerMemberNameAttribute"/>. </param>
	public MissingSettingsManagerException(ISettings settings, [CallerMemberName] string? methodName = null)
		: base($"Could not find a {nameof(ISettingsManager)} to use with the '{methodName}'. Has the hidden '{nameof(SettingsExtensions.InitializeExtensionMethods)}' been called to setup usage of this extension method?")
	{
		this.Settings = settings;
	}
}

/// <summary>
/// Exception thrown if the generic <c>TSettings</c> parameter of an extension methods of <see cref="ISettings"/> does not match the type of the settings instance.
/// </summary>
[Obsolete("The 'Reload' extension method - that was the only one to throw this exception - now has its generic parameter match the extended settings type. therefor making this exception superfluous.")]
public sealed class SettingsTypeMismatchException : SettingsExtensionMethodException
{
	/// <summary> The expected type of the settings instance. </summary>
	public Type TargetType { get; }

	/// <summary> The actual type of the settings instance. </summary>
	public Type ActualType { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="targetType"> <see cref="TargetType"/> </param>
	/// <param name="actualType"> <see cref="ActualType"/> </param>
	public SettingsTypeMismatchException(Type targetType, Type actualType)
		: base($"The expected settings type '{targetType}' does not match the type of the instance '{actualType}'.")
	{
		this.TargetType = targetType;
		this.ActualType = actualType;
	}
}