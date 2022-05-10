#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Collections;
using System.Reflection;

namespace Phoenix.Functionality.Settings.Encryption;

class EncryptSettingsManager : ISettingsManager
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	private readonly ISettingsManager _underlyingSettingsManager;

	private readonly Action<string>? _writeCallback;
		
	private readonly EncryptionHelper _encryptionHelper;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="underlyingSettingsManager"> The <see cref="ISettingsManager"/> that will be used under the hood. </param>
	/// <param name="key"> Optional secret key to use for the symmetric algorithm. </param>
	/// <param name="vector"> Optional initialization vector to use for the symmetric algorithm. </param>
	/// <param name="writeCallback"> Optional callback for the internal output. </param>
	public EncryptSettingsManager(ISettingsManager underlyingSettingsManager, byte[]? key = null, byte[]? vector = null, Action<string>? writeCallback = null)
	{
		// Save parameters.
		_underlyingSettingsManager = underlyingSettingsManager;
		_writeCallback = writeCallback;

		// Initialize fields.
		_encryptionHelper = new EncryptionHelper(key, vector);
	}

	#endregion

	#region Methods

	#region Implementation of ISettingsManager

	/// <inheritdoc />
	public TSettings Load<TSettings>(bool bypassCache = false, bool preventUpdate = false, bool throwIfNoDataIsAvailable = false)
		where TSettings : class, ISettings, new()
	{
		var settings = _underlyingSettingsManager.Load<TSettings>(bypassCache, preventUpdate, throwIfNoDataIsAvailable);
		var needsEncryption = this.DecryptProperties(settings);
		if (needsEncryption) this.Save(settings);
		return settings;
	}

	/// <inheritdoc />
	public void Save<TSettings>(TSettings settings, bool createBackup = default)
		where TSettings : ISettings
	{
		this.EncryptProperties(settings);
		_underlyingSettingsManager.Save(settings, createBackup);
		this.DecryptProperties(settings);
	}

	/// <inheritdoc />
	public void Delete<TSettings>(bool createBackup = default)
		where TSettings : ISettings
	{
		_underlyingSettingsManager.Delete<TSettings>(createBackup);
	}

	#endregion

	/// <summary>
	/// Decrypts all properties attributed with <see cref="EncryptAttribute"/> in <paramref name="settings"/>.
	/// </summary>
	/// <typeparam name="TSettings"> The type of the settings. </typeparam>
	/// <param name="settings"> The settings to decrypt. </param>
	/// <returns> True if any properties attributed with <see cref="EncryptAttribute"/> are not encrypted. </returns>
	internal bool DecryptProperties<TSettings>(TSettings settings)
	{
		var allPropertiesAreEncrypted = true;
		var propertiesToDecrypt = this.GetRelevantProperties(settings);
		foreach (var (setter, stringValue, _) in propertiesToDecrypt)
		{
			if (String.IsNullOrWhiteSpace(stringValue)) continue;
			var decryptedValue = _encryptionHelper.Decrypt(stringValue);
			allPropertiesAreEncrypted &= !String.Equals(stringValue, decryptedValue, StringComparison.OrdinalIgnoreCase);
			if (stringValue != decryptedValue) setter.Invoke(decryptedValue);
		}
		return !allPropertiesAreEncrypted;
	}

	internal void EncryptProperties<TSettings>(TSettings settings)
	{
		var propertiesToEncrypt = this.GetRelevantProperties(settings);
		foreach (var (setter, stringValue, _) in propertiesToEncrypt)
		{
			if (String.IsNullOrWhiteSpace(stringValue)) continue;
			var encryptedValue = _encryptionHelper.Encrypt(stringValue);
			if (stringValue != encryptedValue) setter.Invoke(encryptedValue);
		}
	}

	internal IReadOnlyCollection<(Action<object?> Setter, string? StringValue, string Name)> GetRelevantProperties<TSettings>(TSettings settings)
		=> EncryptSettingsManager.GetRelevantProperties(settings, _writeCallback).ToArray();

	internal static IEnumerable<(Action<object?> Setter, string? StringValue, string Name)> GetRelevantProperties(object? @object, Action<string>? writeCallback, int depth = 0)
	{
		var indentation = new string('\t', depth);
		if (depth >= 100)
		{
			writeCallback?.Invoke($"{indentation}Maximum depth has been reached.");
			yield break;
		}
		if (@object is null)
		{
			writeCallback?.Invoke($"{indentation}The object is null and can therefore not be handled.");
			yield break;
		}

		var type = @object.GetType();
		var properties = type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		foreach (var propertyInfo in properties)
		{
			foreach (var tuple in CheckPropertyOfObject(@object, propertyInfo, depth, writeCallback))
			{
				yield return tuple;
			}
		}
	}

	private static IEnumerable<(Action<object?> Setter, string? StringValue, string Name)> CheckPropertyOfObject(object @object, PropertyInfo propertyInfo, int depth, Action<string>? writeCallback, bool comesFromAttributedParent = false)
	{
		var propertyName = propertyInfo.Name;
		var propertyType = propertyInfo.PropertyType;
		var isAttributed = comesFromAttributedParent || IsAttributed(propertyInfo);

		var indentation = new string('\t', depth + 1);
		writeCallback?.Invoke($"{new string('\t', depth)}• {propertyName} ({propertyType}):");
		//writeCallback?.Invoke($"{indentation}{nameof(propertyType.IsPrimitive)}: {propertyType.IsPrimitive}");
		//writeCallback?.Invoke($"{indentation}IsAttributed: {isAttributed}");
		//writeCallback?.Invoke($"{indentation}{nameof(propertyType.Module.ScopeName)}: {propertyType.Module.ScopeName}");
		//writeCallback?.Invoke($"{indentation}{nameof(propertyType.Module.FullyQualifiedName)}: {propertyType.Module.FullyQualifiedName}");
		//writeCallback?.Invoke($"{indentation}{nameof(propertyType.Assembly.Location)}: {propertyType.Assembly.Location}");

		object? value;
		try
		{
			value = propertyInfo.GetValue(@object);
		}
		catch (Exception ex)
		{
			writeCallback?.Invoke($"{indentation}⇒ An error occurred while trying to get the value of the property.");
			writeCallback?.Invoke($"{indentation}⇒ {ex}");
			yield break;
		}

		if (!isAttributed)
		{
			if (ShouldNotFollow(propertyInfo))
			{
				writeCallback?.Invoke($"{indentation}⇒ The property is attributed with {nameof(EncryptDoNotFollowAttribute)} and will not be analyzed further.");
				yield break;
			}

			var isNested = false;
			if (ShouldFollow(propertyInfo))
			{
				isNested = true;
				writeCallback?.Invoke($"{indentation}⇒ The property is attributed with {nameof(EncryptForceFollowAttribute)}. Its value will be inspected.");
			}
			if (!isNested && IsNested(propertyType))
			{
				isNested = true;
				writeCallback?.Invoke($"{indentation}⇒ The property is nested. Its value will be inspected.");
			}
			if (isNested)
			{
				foreach (var tuple in GetRelevantProperties(value, writeCallback, depth + 1))
				{
					yield return tuple;
				}
				yield break;
			}

			if (IsCollection(propertyType))
			{
				IEnumerable? enumerable = value as IEnumerable;
				writeCallback?.Invoke($"{indentation}⇒ The property is a collection. Its items will be inspected.");
				foreach (var item in enumerable ?? Array.Empty<object>())
				{
					// Check each item if it is either directly supported (string) or a nested type.
					foreach (var tuple in GetRelevantProperties(item, writeCallback, depth + 1))
					{
						yield return tuple;
					}
				}
				yield break;
			}

			writeCallback?.Invoke($"{indentation}⇒ The property is not attributed and no nested or collection type.");
			yield break;
		}

		if (IsSupported(propertyType))
		{
			writeCallback?.Invoke($"{indentation}⇒ The property is attributed and of a supported type. It will be de-/encrypted.");

			Action<object?> setter = (newValue) => propertyInfo.SetValue(@object, newValue);
			var stringValue = value?.ToString();
			yield return (setter, stringValue, propertyName);
			yield break;
		}

		if (IsCollection(propertyType))
		{
			IList list = value as IList ?? Array.Empty<object>();
			foreach (var tuple in HandleAttributedList(list, propertyName, depth, writeCallback))
			{
				yield return tuple;
			}
			yield break;
		}

		writeCallback?.Invoke($"{indentation}⇒ The property is attributed but its type is not supported.");
	}

	private static IEnumerable<(Action<object?> Setter, string? StringValue, string Name)> HandleAttributedList(IList list, string propertyName, int depth, Action<string>? writeCallback)
	{
		var indentation = new string('\t', depth + 1);
		if (depth >= 100)
		{
			writeCallback?.Invoke($"{indentation}Maximum depth has been reached (List handling).");
			yield break;
		}

		if (!TryGetGenericListType(list.GetType(), out var genericType))
		{
			writeCallback?.Invoke($"{indentation}⇒ The property is an attributed but non-generic list.");
		}
		else if (IsSupported(genericType))
		{
			// Prepare all values of the list for en-/decryption.
			writeCallback?.Invoke($"{indentation}⇒ The property is an attributed list consisting of supported types. Its {list.Count} item(s) will be de-/encrypted.");
			for (var index = 0; index < list.Count; index++)
			{
				var localIndex = index;
				Action<object?> setter = (newValue) => list[localIndex] = newValue;
				yield return (setter, list[localIndex]?.ToString(), propertyName);
			}
		}
		else if (IsCollection(genericType))
		{
			// This seems to be a nested collection. Execute a recursive call with the values.
			writeCallback?.Invoke($"{indentation}⇒ The property is an attributed but stacked list. Its {list.Count} item(s) will be inspected.");
			foreach (var item in list)
			{
				IList nestedList = item as IList ?? Array.Empty<object>();
				foreach (var tuple in HandleAttributedList(nestedList,propertyName, depth + 1, writeCallback))
				{
					yield return tuple;
				}
			}
		}
		else
		{
			writeCallback?.Invoke($"{indentation}⇒ The property is an attributed list of an unsupported type '{genericType}'.");
		}
	}
		
	private static bool IsAttributed(PropertyInfo propertyInfo)
	{
		return propertyInfo.GetCustomAttribute<EncryptAttribute>() switch
		{
			null => false,
			_ => true
		};
	}
		
	private static bool IsSupported(Type propertyType)
	{
		return propertyType == typeof(string) || propertyType == typeof(object);
	}

	internal static bool ShouldNotFollow(PropertyInfo propertyInfo)
	{
		// Manual specification overrules the automatic mechanism.
		return propertyInfo.GetCustomAttribute<EncryptDoNotFollowAttribute>() is not null;
	}

	internal static bool ShouldFollow(PropertyInfo propertyInfo)
	{
		// Manual specification overrules the automatic mechanism.
		return propertyInfo.GetCustomAttribute<EncryptForceFollowAttribute>() is not null;
	}

	internal static bool IsNested(Type type)
	{
		if (type == typeof(ISettings)) return true; // Handle settings directly, as this is always the first object that will be checked.
			
		if (type.IsPrimitive) return false;
		if (type == typeof(string)) return false; // Strings will be one of the most commonly encountered types and they are also an IEnumerable, that shouldn't be enumerated.
		if (IsCollection(type)) return false;     // Collections must also not be treated as nested.
			
		// This filters build-in types for .Net Framework.
		if (type.Module.ScopeName == "CommonLanguageRuntimeLibrary") return false;

		// This filters build-in types for .Net.
		/*
		! Unfortunately the 'type.Module.FullyQualifiedName' is not set when running an application published as a single-file executable. Same goes for 'type.Assembly.Location'.
		! Therefor, as to prevent all properties being treated as nested when running single-file executables, the whole namespace 'System' is excluded. This my cause problems,
		! if custom code uses this namespace too.
		*/
		//if (type.Module.ScopeName.StartsWith("System.") && type.Module.FullyQualifiedName.Contains("dotnet")) return false;
		if (type.Module.ScopeName.StartsWith("System.")) return false;
			
		return true;
	}

	internal static bool IsCollection(Type type)
	{
		if (type == typeof(string)) return false;                     // Since strings are enumerable, they must be excluded separately.
		if (typeof(IDictionary).IsAssignableFrom(type)) return false; // Dictionaries are enumerable but are currently not supported.
		if (typeof(IEnumerable).IsAssignableFrom(type)) return true;
		return false;
	}

#if NETSTANDARD2_0
		private static bool TryGetGenericListType(Type propertyType, out Type? genericType)
#else
	private static bool TryGetGenericListType(Type propertyType, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Type? genericType)
#endif
	{
		genericType = null;
			
		// Quick check if the type even is a list.
		if (!typeof(IList).IsAssignableFrom(propertyType)) return false;
			
		// Array
		if (propertyType.IsArray)
		{
			genericType = propertyType.GetElementType();
		}
		// Generic list
		else if (propertyType.IsGenericType)
		{
			genericType = propertyType.GetGenericArguments().First();
		}
		// Inheritance
		else
		{
			var interfaceType = propertyType.GetInterfaces().FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>));
			genericType = interfaceType?.GetGenericArguments().First();
		}

		return  genericType is not null;
	}

	#endregion
}