using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Settings.Encryption;

namespace Settings.Encryption.Test
{
	public class EncryptionHelperTest
	{
#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
		private IFixture _fixture;
#pragma warning restore 8618

		[SetUp]
		public void Setup()
		{
			_fixture = new Fixture().Customize(new AutoMoqCustomization());
		}

		[Test]
		public void Check_Encryption_And_Decrypt_Are_Identical()
		{
			// Arrange
			var unencrypted = "unencrypted";

			// Act
			var encrypted = EncryptionHelper.Encrypt(unencrypted);
			var decrypted = EncryptionHelper.Decrypt(encrypted);

			// Assert
			Assert.AreEqual(unencrypted, decrypted);
		}

		/// <summary>
		/// Checks that encrypting the same value yields different results each time. This is because the <see cref="EncryptionHelper.Marker"/> is placed randomly.
		/// </summary>
		[Test]
		public void Check_Encryption_Yields_Different_Results()
		{
			// Arrange
			var unencrypted = "unencrypted";

			// Act
			var encrypted1 = EncryptionHelper.Encrypt(unencrypted);
			var encrypted2 = EncryptionHelper.Encrypt(unencrypted);

			// Assert
			Assert.AreNotEqual(encrypted1, encrypted2);
		}

		[Test]
		public void Check_Encryption_Adds_Marker()
		{
			// Arrange
			var unencrypted = "unencrypted";

			// Act
			var encrypted = EncryptionHelper.Encrypt(unencrypted);

			// Assert
			Assert.True(encrypted.Contains(EncryptionHelper.Marker));
		}

		[Test]
		public void Check_Unencrypted_Value_Is_Not_Decrypted()
		{
			// Arrange
			var pseudoEncrypted = "unencrypted";

			// Act
			var decrypted = EncryptionHelper.Decrypt(pseudoEncrypted);

			// Assert
			Assert.AreEqual(pseudoEncrypted, decrypted);
		}

		[Test]
		public void Check_Null_Value_Is_Not_Encrypted()
		{
			// Act
			var encrypted = EncryptionHelper.Encrypt(null);

			// Assert
			Assert.IsNull(encrypted);
		}

		[Test]
		public void Check_Null_Value_Is_Not_Decrypted()
		{
			// Act
			var decrypted = EncryptionHelper.Decrypt(null);

			// Assert
			Assert.IsNull(decrypted);
		}
	}
}