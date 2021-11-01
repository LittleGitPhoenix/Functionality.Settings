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

		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		public EncryptSettingsManager(ISettingsManager underlyingSettingsManager)
		{
			// Save parameters.
			_underlyingSettingsManager = underlyingSettingsManager;

			// Initialize fields.
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
			//var hasAttributedProperties = false;
			var allPropertiesAreEncrypted = true;
			//var wasSomethingDecrypted = false;
			var propertiesToDecrypt = this.GetRelevantProperties(settings);
			foreach (var (property, containingObject, stringValue) in propertiesToDecrypt)
			{
				if (String.IsNullOrWhiteSpace(stringValue)) continue;
				//hasAttributedProperties = true;
				var decryptedValue = EncryptionHelper.Decrypt(stringValue);
				allPropertiesAreEncrypted &= !stringValue.Equals(decryptedValue, StringComparison.OrdinalIgnoreCase);
				if (stringValue != decryptedValue) property.SetValue(containingObject, decryptedValue);
			}
			return !allPropertiesAreEncrypted;
		}

		internal void EncryptProperties<TSettings>(TSettings settings)
		{
			var propertiesToEncrypt = this.GetRelevantProperties(settings);
			foreach (var (property, containingObject, stringValue) in propertiesToEncrypt)
			{
				if (String.IsNullOrWhiteSpace(stringValue)) continue;
				var encryptedValue = EncryptionHelper.Encrypt(stringValue);
				if (stringValue != encryptedValue) property.SetValue(containingObject, encryptedValue);
			}
		}

		internal IReadOnlyCollection<(PropertyInfo Property, object ContainingObject, string? StringValue)> GetRelevantProperties<TSettings>(TSettings settings)
			=> EncryptSettingsManager.GetRelevantProperties(settings).ToArray();

		internal static IEnumerable<(PropertyInfo Property, object ContainingObject, string? StringValue)> GetRelevantProperties(object? @object, int depth = 0)
		{
			if (depth++ == 100) yield break;
			if (@object is null) yield break;
			
			var type = @object.GetType();
			var properties = type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			
			foreach (var propertyInfo in properties)
			{
				if (EncryptSettingsManager.IsRelevant(propertyInfo))
				{
					var stringValue = propertyInfo.GetValue(@object)?.ToString();
					yield return (propertyInfo, @object, stringValue);
					continue;
				}

				//Console.WriteLine($"{propertyInfo.PropertyType}");
				//Console.WriteLine($"\t{nameof(propertyInfo.PropertyType.IsPrimitive)}: {propertyInfo.PropertyType.IsPrimitive}");
				//Console.WriteLine($"\t{nameof(propertyInfo.PropertyType.Module.ScopeName)}: {propertyInfo.PropertyType.Module.ScopeName}");
				//Console.WriteLine($"\t{nameof(propertyInfo.PropertyType.Module.FullyQualifiedName)}: {propertyInfo.PropertyType.Module.FullyQualifiedName}");

				var nestedValue = propertyInfo.GetValue(@object);
				if (EncryptSettingsManager.IsNested(propertyInfo))
				{
					foreach (var tuple in EncryptSettingsManager.GetRelevantProperties(nestedValue, depth))
					{
						yield return tuple;
					}
				}
				else if (EncryptSettingsManager.IsCollection(propertyInfo))
				{
					IEnumerable? enumerable = nestedValue as IEnumerable;
					foreach (var item in enumerable ?? Array.Empty<object>())
					{
						foreach (var tuple in EncryptSettingsManager.GetRelevantProperties(item, depth))
						{
							yield return tuple;
						}
					}
				}
			}
		}

		private static bool IsRelevant(PropertyInfo propertyInfo)
		{
			if (propertyInfo.GetCustomAttribute<EncryptAttribute>() is null) return false;
			if (propertyInfo.PropertyType != typeof(string) && propertyInfo.PropertyType != typeof(object)) return false;
			return true;
		}

		private static bool IsNested(PropertyInfo propertyInfo)
		{
			if (propertyInfo.PropertyType.IsPrimitive) return false;
			if (propertyInfo.PropertyType == typeof(string)) return false; //! Handle strings directly, because it will be one of the most commonly encountered types and it is also an IEnumerable, that shouldn't be enumerated.

			// This filters build-in types for .Net Framework.
			if (propertyInfo.PropertyType.Module.ScopeName == "CommonLanguageRuntimeLibrary") return false;

			//TODO This should be improved.
			// This filters build-in types for .Net.
			if (propertyInfo.PropertyType.Module.ScopeName.StartsWith("System.") && propertyInfo.PropertyType.Module.FullyQualifiedName.Contains("dotnet")) return false;

			return true;
		}

		private static bool IsCollection(PropertyInfo propertyInfo)
		{
			if (propertyInfo.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType)) return true;
			return false;
		}

		#endregion
	}
}