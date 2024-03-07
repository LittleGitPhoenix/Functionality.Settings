using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Settings.Serializers.Json.Net.CustomConverters;
using System.Net;

namespace Settings.Serializers.Json.Net.Test;

public class IpAddressConverterTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[OneTimeSetUp]
	public void BeforeAllTests() { }

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTest() { }

	#endregion

	#region Tests

	#region Deserialize

	[Test]
	public void DeserializeIpAddress()
	{
		// Arrange
		var host = "192.168.0.1";
		var converter = new IpAddressConverter();

		// Act
		var ipAddress = converter.Deserialize(host);

		// Assert
		Assert.That(host, Is.EqualTo(ipAddress.ToString()));
	}

	[Test]
	public void DeserializeIpAddressFromLocalhost()
	{
		// Arrange
		var host = "localhost";
		var converter = new IpAddressConverter();

		// Act
		var ipAddress = converter.Deserialize(host);

		// Assert
		Assert.That(IPAddress.Loopback, Is.EqualTo(ipAddress));
	}

	[Test]
	public void DeserializeIpAddressFromLoopback()
	{
		// Arrange
		var host = nameof(IPAddress.Loopback);
		var converter = new IpAddressConverter();

		// Act
		var ipAddress = converter.Deserialize(host);

		// Assert
		Assert.That(IPAddress.Loopback, Is.EqualTo(ipAddress));
	}

	[Test]
	public void DeserializeIpAddressFromLoopbackv6()
	{
		// Arrange
		var host = nameof(IPAddress.IPv6Loopback);
		var converter = new IpAddressConverter();

		// Act
		var ipAddress = converter.Deserialize(host);

		// Assert
		Assert.That(IPAddress.IPv6Loopback, Is.EqualTo(ipAddress));
	}

	[Test]
	public void DeserializeIpAddressFromBroadcast()
	{
		// Arrange
		var host = nameof(IPAddress.Broadcast);
		var converter = new IpAddressConverter();

		// Act
		var ipAddress = converter.Deserialize(host);

		// Assert
		Assert.That(IPAddress.Broadcast, Is.EqualTo(ipAddress));
	}

	#endregion

	#region Serialize

	[Test]
	public void SerializeIpAddress()
	{
		// Arrange
		var host = "192.168.0.1";
		var ip = IPAddress.Parse(host);
		var converter = new IpAddressConverter();

		// Act
		var actualHost = converter.Serialize(ip);

		// Assert
		Assert.That(host, Is.EqualTo(actualHost));
	}

	[Test]
	public void SerializeIpAddressToLoopback()
	{
		// Arrange
		var ip = IPAddress.Loopback;
		var converter = new IpAddressConverter();

		// Act
		var actualHost = converter.Serialize(ip);

		// Assert
		Assert.That(nameof(IPAddress.Loopback), Is.EqualTo(actualHost));
	}

	[Test]
	public void SerializeIpAddressToLoopbackv6()
	{
		// Arrange
		var ip = IPAddress.IPv6Loopback;
		var converter = new IpAddressConverter();

		// Act
		var actualHost = converter.Serialize(ip);

		// Assert
		Assert.That(nameof(IPAddress.IPv6Loopback), Is.EqualTo(actualHost));
	}

	[Test]
	public void SerializeIpAddressToBroadcast()
	{
		// Arrange
		var ip = IPAddress.Broadcast;
		var converter = new IpAddressConverter();

		// Act
		var actualHost = converter.Serialize(ip);

		// Assert
		Assert.That(nameof(IPAddress.Broadcast), Is.EqualTo(actualHost));
	}

	#endregion

	#endregion
}