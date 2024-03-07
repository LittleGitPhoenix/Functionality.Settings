#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Phoenix.Functionality.Settings.Cache;

namespace Phoenix.Functionality.Settings;

/// <summary>
/// Extension methods for <see cref="ISettings"/>.
/// </summary>
public static class SettingsExtensions
{
	#region Fields

	/// <summary>
	/// Cache of all <see cref="ISettingsManager"/> mapped to the concrete <see cref="ISettings"/> type that where loaded with it.
	/// </summary>
	internal static readonly ConcurrentDictionary<Type, ISettingsManager> Cache = new();

	#endregion

	#region Properties
	#endregion

	#region Methods

	#region Name

	/// <summary>
	/// Gets the name of a <paramref name="settings"/> instance respecting the <see cref="SettingsNameAttribute"/>.
	/// </summary>
	/// <param name="settings"> The <see cref="ISettings"/> instance for which to obtain the name. </param>
	/// <returns> The name of the settings class. </returns>
	public static string GetSettingsName(this ISettings settings)
		=> GetSettingsName(settings.GetType());
	
	/// <summary>
	/// Gets the name of a settings class respecting the <see cref="SettingsNameAttribute"/>.
	/// </summary>
	/// <param name="settingsType"> The type of the settings class. </param>
	/// <returns> The name of the settings class. </returns>
	internal static string GetSettingsName(Type settingsType)
	{
		if (!typeof(ISettings).IsAssignableFrom(settingsType)) throw new ArgumentException($"The passed type '{settingsType}' must implement the interface '{nameof(ISettings)}'.");

		// First check for the SettingsFileNameAttribute.
		var settingsFileNameAttribute = settingsType.GetCustomAttribute<SettingsNameAttribute>();
		return settingsFileNameAttribute?.Name ?? $"{settingsType.Namespace}.{settingsType.Name}";
	}

	#endregion

	#region Reload, Save, Delete

	/// <summary>
	/// Initializes the usage of the <see cref="Reload{TSettings}"/>, <see cref="Save{TSettings}"/> and <see cref="Delete{TSettings}"/> extesnion methods by internally saving the <paramref name="settingsManager"/> used to originally obtain the <paramref name="settings"/>.
	/// </summary>
	/// <param name="settings"> The extended <see cref="ISettings"/> instance. </param>
	/// <param name="settingsManager"> The <see cref="ISettingsManager"/> that was used to load the <paramref name="settings"/> and is needed for (most) of the extension methods for <see cref="ISettings"/> of this class. </param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void InitializeExtensionMethods(this ISettings settings, ISettingsManager settingsManager)
	{
		Cache.AddOrUpdate(settings.GetType(), settingsManager, (_, _) => settingsManager);
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
		if (!Cache.TryGetValue(typeof(TSettings), out var settingsManager))
		{
			throw new MissingSettingsManagerException(settings);
		}

		return settingsManager.Load<TSettings>(true, preventUpdate);
	}

	/// <inheritdoc cref="ISettingsManager.Save{TSettings}"/>
	/// <exception cref="MissingSettingsManagerException"> Thrown if the <paramref name="settings"/> instance is not linked to a cached <see cref="ISettingsManager"/>. Use the hidden <see cref="InitializeExtensionMethods"/> method for initialization. </exception>
	/// <remarks> This extension method will only work for <see cref="ISettings"/> instances that called the <see cref="InitializeExtensionMethods"/> method so that the <see cref="ISettingsManager"/> that was used to load the instance has been cached internally and can be used for saving. </remarks>
	public static void Save<TSettings>(this TSettings settings, bool createBackup = default)
		where TSettings : ISettings
	{
		if (!Cache.TryGetValue(settings.GetType(), out var settingsManager))
		{
			throw new MissingSettingsManagerException(settings);
		}

		settingsManager.Save(settings, createBackup);
	}

	/// <inheritdoc cref="ISettingsManager.Delete{TSettings}"/>
	/// <exception cref="MissingSettingsManagerException"> Thrown if the <paramref name="settings"/> instance is not linked to a cached <see cref="ISettingsManager"/>. Use the hidden <see cref="InitializeExtensionMethods"/> method for initialization. </exception>
	/// <remarks> This extension method will only work for <see cref="ISettings"/> instances that called the <see cref="InitializeExtensionMethods"/> method so that the <see cref="ISettingsManager"/> that was used to load the instance has been cached internally and can be used for saving. </remarks>
	public static void Delete<TSettings>(this TSettings settings, bool createBackup = default)
		where TSettings : ISettings
	{
		if (!Cache.TryGetValue(settings.GetType(), out var settingsManager))
		{
			throw new MissingSettingsManagerException(settings);
		}

		settingsManager.Delete<TSettings>(createBackup);
	}
	
	#endregion

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
/// <para> • <see cref="SettingsExtensions.Save{TSettings}"/> </para>
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
/// Exception thrown if the generic <b>TSettings</b> parameter of an extension methods of <see cref="ISettings"/> does not match the type of the settings instance.
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