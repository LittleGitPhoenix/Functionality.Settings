#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Phoenix.Functionality.Settings.Encryption
{
	class EncryptSettingsManager : ISettingsManager
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private readonly ISettingsManager _underlyingSettingsManager;

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
		public EncryptSettingsManager(ISettingsManager underlyingSettingsManager, byte[]? key = null, byte[]? vector = null)
		{
			// Save parameters.
			_underlyingSettingsManager = underlyingSettingsManager;

			// Initialize fields.
			_encryptionHelper = new EncryptionHelper(key, vector);
		}

		#endregion

		#region Methods

		#region Implementation of ISettingsManager

		/// <inheritdoc />
		public TSettings Load<TSettings>(bool bypassCache = false, bool preventUpdate = false)
			where TSettings : class, ISettings, new()
		{
			var settings = _underlyingSettingsManager.Load<TSettings>(bypassCache, preventUpdate);
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
				allPropertiesAreEncrypted &= !stringValue.Equals(decryptedValue, StringComparison.OrdinalIgnoreCase);
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
			=> EncryptSettingsManager.GetRelevantProperties(settings).ToArray();

		internal static IEnumerable<(Action<object?> Setter, string? StringValue, string Name)> GetRelevantProperties(object? @object, int depth = 0)
		{
			var indentation = new string('\t', depth);
			if (depth >= 100)
			{
				Write($"{indentation}Maximum depth has been reached.");
				yield break;
			}
			if (@object is null)
			{
				Write($"{indentation}The object is null and can therefore not be handled.");
				yield break;
			}

			var type = @object.GetType();
			var properties = type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var propertyInfo in properties)
			{
				foreach (var tuple in CheckPropertyOfObject(@object, propertyInfo, depth))
				{
					yield return tuple;
				}
			}
		}

		internal static IEnumerable<(Action<object?> Setter, string? StringValue, string Name)> CheckPropertyOfObject(object @object, PropertyInfo propertyInfo, int depth, bool comesFromAttributedParent = false)
		{
			var propertyName = propertyInfo.Name;
			var propertyType = propertyInfo.PropertyType;
			var isAttributed = comesFromAttributedParent || IsAttributed(propertyInfo);

			var indentation = new string('\t', depth + 1);
			Write($"{new string('\t', depth)}• {propertyName} ({propertyType}):");
			//Write($"{indentation}{nameof(propertyType.IsPrimitive)}: {propertyType.IsPrimitive}");
			Write($"{indentation}IsAttributed: {isAttributed}");
			//Write($"{indentation}{nameof(propertyType.Module.ScopeName)}: {propertyType.Module.ScopeName}");
			//Write($"{indentation}{nameof(propertyType.Module.FullyQualifiedName)}: {propertyType.Module.FullyQualifiedName}");

			object? value;
			try
			{
				value = propertyInfo.GetValue(@object);
			}
			catch (Exception ex)
			{
				Write($"{indentation}⇒ An error occurred while trying to get the value of the property.");
				Write($"{indentation}⇒ {ex}");
				yield break;
			}

			if (!isAttributed)
			{
				if (IsNested(propertyType))
				{
					Write($"{indentation}⇒ The property is nested. Its value will be inspected.");
					foreach (var tuple in GetRelevantProperties(value, depth + 1))
					{
						yield return tuple;
					}
					yield break;
				}

				if (IsCollection(propertyType))
				{
					IEnumerable? enumerable = value as IEnumerable;
					Write($"{indentation}⇒ The property is a collection. Its items will be inspected.");
					foreach (var item in enumerable ?? Array.Empty<object>())
					{
						//TODO Check each item if it is either directly supported (string) or a nested type.
						foreach (var tuple in GetRelevantProperties(item, depth + 1))
						{
							yield return tuple;
						}
					}
					yield break;
				}

				Write($"{indentation}⇒ The property is not attributed and no nested or collection type.");
				yield break;
			}

			if (IsSupported(propertyType))
			{
				Write($"{indentation}⇒ The property is attributed and of a supported type. It will be de-/encrypted.");

				Action<object?> setter = (newValue) => propertyInfo.SetValue(@object, newValue);
				var stringValue = value?.ToString();
				yield return (setter, stringValue, propertyName);
				yield break;
			}

			if (IsCollection(propertyType))
			{
				IList list = value as IList ?? Array.Empty<object>();
				foreach (var tuple in HandleAttributedList(list, propertyName, depth))
				{
					yield return tuple;
				}
				yield break;
			}

			Write($"{indentation}⇒ The property is attributed but its type is not supported.");
		}

		internal static IEnumerable<(Action<object?> Setter, string? StringValue, string Name)> HandleAttributedList(IList list, string propertyName, int depth)
		{
			var indentation = new string('\t', depth + 1);
			if (depth >= 100)
			{
				Write($"{indentation}Maximum depth has been reached.");
				yield break;
			}

			if (!TryGetGenericListType(list.GetType(), out var genericType))
			{
				Write($"{indentation}⇒ The property is an attributed but non-generic list.");
			}
			else if (IsSupported(genericType))
			{
				// Prepare all values of the list for en-/decryption.
				Write($"{indentation}⇒ The property is an attributed list consisting of supported types. Its {list.Count} item(s) will be de-/encrypted.");
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
				Write($"{indentation}⇒ The property is an attributed but stacked list. Its {list.Count} item(s) will be inspected.");
				foreach (var item in list)
				{
					IList nestedList = item as IList ?? Array.Empty<object>();
					foreach (var tuple in HandleAttributedList(nestedList,propertyName, depth + 1))
					{
						yield return tuple;
					}
				}
			}
			else
			{
				Write($"{indentation}⇒ The property is an attributed list of an unsupported type '{genericType}'.");
			}
		}
		
		private static bool IsAttributed(PropertyInfo propertyInfo)
		{
			if (propertyInfo.GetCustomAttribute<EncryptAttribute>() is null) return false;
			return true;
		}
		
		private static bool IsSupported(Type propertyType)
		{
			if (propertyType != typeof(string) && propertyType != typeof(object)) return false;
			return true;
		}
		
		private static bool IsNested(Type type)
		{
			if (type == typeof(ISettings)) return true; //! Handle settings directly, as this is always the first object that will be checked.
			
			if (type.IsPrimitive) return false;
			if (type == typeof(string)) return false; //! Handle strings directly, because it will be one of the most commonly encountered types and it is also an IEnumerable, that shouldn't be enumerated.

			// This filters build-in types for .Net Framework.
			if (type.Module.ScopeName == "CommonLanguageRuntimeLibrary") return false;

			//TODO This should be improved.
			// This filters build-in types for .Net.
			if (type.Module.ScopeName.StartsWith("System.") && type.Module.FullyQualifiedName.Contains("dotnet")) return false;

			return true;
		}

		private static bool IsCollection(Type propertyType)
		{
			if (propertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propertyType)) return true;
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

		private static void Write(string message)
		{
#if DEBUG
			//System.Console.WriteLine(message);
#endif
		}

		#endregion
	}
}