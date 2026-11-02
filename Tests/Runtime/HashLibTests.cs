using NUnit.Framework;

public sealed class HashLibTests
{
    [Test]
    public void Base64RoundTrips() =>
        Assert.That(HashLib.Base64Decode(HashLib.Base64Encode("{\"a\":1,\"vi\":\"tiếng Việt\"}")), Is.EqualTo("{\"a\":1,\"vi\":\"tiếng Việt\"}"));

    [Test]
    public void DeviceBoundEncryptionRoundTrips() =>
        Assert.That(HashLib.DecryptAndDeviceID(HashLib.EncryptAndDeviceID("payload")), Is.EqualTo("payload"));
}
