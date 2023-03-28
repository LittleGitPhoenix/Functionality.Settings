using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Settings.Encryption;

namespace Settings.Encryption.Test;

public class EncryptionHelperTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	private readonly EncryptionHelper _encryptionHelper = new EncryptionHelper();

	[OneTimeSetUp]
	public void BeforeAllTests() { }

	[SetUp]
	public void Setup()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTest() { }

	#endregion

	#region Tests
	
	[Test]
	public void Encryption_And_Decrypt_Are_Identical()
	{
		// Arrange
		var unencrypted = "unencrypted";

		// Act
		var encrypted = _encryptionHelper.Encrypt(unencrypted);
		var decrypted = _encryptionHelper.Decrypt(encrypted);

		// Assert
		Assert.AreEqual(unencrypted, decrypted);
	}

	/// <summary>
	/// Checks that encrypting the same value yields different results each time. This is because the <see cref="EncryptionHelper.Marker"/> is placed randomly.
	/// </summary>
	[Test]
	public void Encryption_Yields_Different_Results()
	{
		// Arrange
		var unencrypted = String.Join("_", Enumerable.Repeat("unencrypted", 100)); //! Make this string a little bit longer, so that the random position of the encryption marker is accidentally not the same for both encryption attempts.

		// Act
		var encrypted1 = _encryptionHelper.Encrypt(unencrypted);
		var encrypted2 = _encryptionHelper.Encrypt(unencrypted);

		// Assert
		Assert.AreNotEqual(encrypted1, encrypted2, "This check may fail, if the random position of the encryption marker is accidentally the same for both independent encryption attempts.");
	}

	[Test]
	public void Encryption_Adds_Marker()
	{
		// Arrange
		var unencrypted = "unencrypted";

		// Act
		var encrypted = _encryptionHelper.Encrypt(unencrypted);

		// Assert
		Assert.True(encrypted?.Contains(EncryptionHelper.Marker) ?? false);
	}

	[Test]
	public void Unencrypted_Value_Is_Not_Decrypted()
	{
		// Arrange
		var pseudoEncrypted = "unencrypted";

		// Act
		var decrypted = _encryptionHelper.Decrypt(pseudoEncrypted);

		// Assert
		Assert.AreEqual(pseudoEncrypted, decrypted);
	}

	[Test]
	public void Null_Value_Is_Not_Encrypted()
	{
		// Act
		var encrypted = _encryptionHelper.Encrypt(null);

		// Assert
		Assert.IsNull(encrypted);
	}

	[Test]
	public void Null_Value_Is_Not_Decrypted()
	{
		// Act
		var decrypted = _encryptionHelper.Decrypt(null);

		// Assert
		Assert.IsNull(decrypted);
	}

	/// <summary>
	/// Checks that an decrypted value is always properly decrypted, no matter how often the encrypted value changes (due to the random <see cref="EncryptionHelper.Marker"/>).
	/// </summary>
	[Test]
	public void Encrypting_Does_Not_Degrade()
	{
		// Arrange
		var initialDecrypted = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. \\!?#~äöü|.:,;@";
		var decrypted = initialDecrypted;
		
		for (var iteration = 0; iteration < 10000; iteration++)
		{
			// Act
			var encrypted = _encryptionHelper.Encrypt(decrypted);
			decrypted = _encryptionHelper.Decrypt(encrypted);
			
			// Assert
			Assert.That(decrypted, Is.EqualTo(initialDecrypted));
		}
	}

	#endregion
}