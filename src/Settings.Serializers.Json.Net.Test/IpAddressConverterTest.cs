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

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	#endregion

	#region Deserialize

	[Test]
	public void Deserialize_IpAddress()
	{
		// Arrange
		var host = "192.168.0.1";
		var converter = new IpAddressConverter();

		// Act
		var ipAddress = converter.Deserialize(host);

		// Assert
		Assert.AreEqual(host, ipAddress.ToString());
	}

	[Test]
	public void Deserialize_IpAddress_From_Localhost()
	{
		// Arrange
		var host = "localhost";
		var converter = new IpAddressConverter();

		// Act
		var ipAddress = converter.Deserialize(host);

		// Assert
		Assert.AreEqual(IPAddress.Loopback, ipAddress);
	}

	[Test]
	public void Deserialize_IpAddress_From_Loopback()
	{
		// Arrange
		var host = nameof(IPAddress.Loopback);
		var converter = new IpAddressConverter();

		// Act
		var ipAddress = converter.Deserialize(host);

		// Assert
		Assert.AreEqual(IPAddress.Loopback, ipAddress);
	}

	[Test]
	public void Deserialize_IpAddress_From_Loopbackv6()
	{
		// Arrange
		var host = nameof(IPAddress.IPv6Loopback);
		var converter = new IpAddressConverter();

		// Act
		var ipAddress = converter.Deserialize(host);

		// Assert
		Assert.AreEqual(IPAddress.IPv6Loopback, ipAddress);
	}

	[Test]
	public void Deserialize_IpAddress_From_Broadcast()
	{
		// Arrange
		var host = nameof(IPAddress.Broadcast);
		var converter = new IpAddressConverter();

		// Act
		var ipAddress = converter.Deserialize(host);

		// Assert
		Assert.AreEqual(IPAddress.Broadcast, ipAddress);
	}

	#endregion

	#region Serialize

	[Test]
	public void Serialize_IpAddress()
	{
		// Arrange
		var host = "192.168.0.1";
		var ip = IPAddress.Parse(host);
		var converter = new IpAddressConverter();

		// Act
		var actualHost = converter.Serialize(ip);

		// Assert
		Assert.AreEqual(host, actualHost);
	}

	[Test]
	public void Serialize_IpAddress_To_Loopback()
	{
		// Arrange
		var ip = IPAddress.Loopback;
		var converter = new IpAddressConverter();

		// Act
		var actualHost = converter.Serialize(ip);

		// Assert
		Assert.AreEqual(nameof(IPAddress.Loopback), actualHost);
	}

	[Test]
	public void Serialize_IpAddress_To_Loopbackv6()
	{
		// Arrange
		var ip = IPAddress.IPv6Loopback;
		var converter = new IpAddressConverter();

		// Act
		var actualHost = converter.Serialize(ip);

		// Assert
		Assert.AreEqual(nameof(IPAddress.IPv6Loopback), actualHost);
	}

	[Test]
	public void Serialize_IpAddress_To_Broadcast()
	{
		// Arrange
		var ip = IPAddress.Broadcast;
		var converter = new IpAddressConverter();

		// Act
		var actualHost = converter.Serialize(ip);

		// Assert
		Assert.AreEqual(nameof(IPAddress.Broadcast), actualHost);
	}

	#endregion
}