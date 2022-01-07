#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Security.Cryptography;

namespace Phoenix.Functionality.Settings.Encryption;

class EncryptionHelper
{
	#region Delegates / Events
	#endregion

	#region Constants

	// Base64 of string (without quotes): "__@?9I*.d+F%R:ud3^@__"
	internal const string Marker = "X19APzlJKi5kK0YlUjp1ZDNeQF9f";

	#endregion

	#region Fields

	private static readonly Random Random = new Random();

	private static readonly byte[] DefaultKey = new byte[] { 99, 119, 58, 216, 72, 201, 226, 160, 240, 173, 211, 44, 113, 209, 162, 1, 71, 170, 69, 181, 236, 163, 17, 179, 231, 153, 163, 222, 181, 8, 11, 193 };

	private static readonly byte[] DefaultVector = new byte[] { 13, 48, 69, 91, 47, 97, 0, 169, 126, 212, 252, 221, 150, 34, 252, 216 };
		
	private readonly byte[] _key;
		
	private readonly byte[] _vector;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="key"> Optional secret key to use for the symmetric algorithm. Default will be <see cref="DefaultKey"/>. </param>
	/// <param name="vector"> Optional initialization vector to use for the symmetric algorithm. Default will be <see cref="DefaultVector"/>. </param>
	public EncryptionHelper(byte[]? key = null, byte[]? vector = null)
	{
		// Save parameters.
		_key = key ?? EncryptionHelper.DefaultKey;
		_vector = vector ?? EncryptionHelper.DefaultVector;

		// Initialize fields.
	}

	#endregion

	#region Methods

	/// <summary>
	/// Encrypts <paramref name="text"/>.
	/// </summary>
	/// <param name="text"> The text to encrypt. </param>
	/// <returns> An encrypted and base64 encoded string. </returns>
	internal string? Encrypt(string? text)
	{
		if (text is null) return null;
		var encryptedData = EncryptionHelper.Encrypt(text, _key, _vector);
		var encryptedBase64Text = Convert.ToBase64String(encryptedData);
		var encryptedText = EncryptionHelper.AddMark(encryptedBase64Text);
		return encryptedText;
	}

	private static byte[] Encrypt(string? text, byte[] key, byte[] vector)
	{
		using var aes = new AesManaged();
		var encryptor = aes.CreateEncryptor(key, vector);
		using var memoryStream = new MemoryStream();
		using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
		//! It seems to be necessary to dispose the writer instance before converting the memory stream into data, as otherwise the data won't contain all necessary bytes. Even just flushing the writer does not work.
		using (var writer = new StreamWriter(cryptoStream))
		{
			writer.Write(text);
		}
		var encrypted = memoryStream.ToArray();
		return encrypted;
	}

	/// <summary>
	/// Decrypts <paramref name="encryptedText"/>.
	/// </summary>
	/// <param name="encryptedText"> The encrypted and in base64 encoded text. </param>
	/// <returns> The plain text. </returns>
	internal string? Decrypt(string? encryptedText)
	{
		if (encryptedText is null) return null;
		if (!EncryptionHelper.TryRemoveMark(encryptedText, out var encryptedBase64Text)) return encryptedText;
		var encryptedData = Convert.FromBase64String(encryptedBase64Text);
		return EncryptionHelper.Decrypt(encryptedData, _key, _vector);
	}

	private static string Decrypt(byte[] encryptedData, byte[] key, byte[] vector)
	{
		using var aes = new AesManaged();
		ICryptoTransform decryptor = aes.CreateDecryptor(key, vector);
		using var memoryStream = new MemoryStream(encryptedData);
		using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
		using var reader = new StreamReader(cryptoStream);
		var text = reader.ReadToEnd();
		return text;
	}
		
	private static string AddMark(string encryptedBase64Text)
	{
		var insertPosition = EncryptionHelper.Random.Next(0, encryptedBase64Text.Length);
		return $"{encryptedBase64Text.Substring(0, insertPosition)}{EncryptionHelper.Marker}{encryptedBase64Text.Substring(insertPosition)}";
	}

#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
		private static bool TryRemoveMark(string encryptedText, out string? encryptedBase64Text)
#else
	private static bool TryRemoveMark(string encryptedText, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? encryptedBase64Text)
#endif
	{
		if (encryptedText.Contains(EncryptionHelper.Marker))
		{
			encryptedBase64Text = encryptedText.Replace(EncryptionHelper.Marker, String.Empty);
			return true;
		}
		else
		{
			encryptedBase64Text = null;
			return false;
		}
	}

	#endregion
}