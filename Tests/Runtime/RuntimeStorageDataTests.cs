using NUnit.Framework;

public sealed class RuntimeStorageDataTests
{
    private SettingSerializable previousSetting;
    private PlayerSerializable previousPlayer;

    [SetUp]
    public void SetUp()
    {
        previousSetting = RuntimeStorageData.SETTING;
        previousPlayer = RuntimeStorageData.PLAYER;
    }

    [TearDown]
    public void TearDown()
    {
        RuntimeStorageData.SETTING = previousSetting;
        RuntimeStorageData.PLAYER = previousPlayer;
    }

    [Test]
    public void IsReadyRequiresSettingAndPlayerData()
    {
        RuntimeStorageData.SETTING = null;
        RuntimeStorageData.PLAYER = null;
        Assert.That(RuntimeStorageData.IsReady, Is.False);

        RuntimeStorageData.SETTING = new SettingSerializable();
        Assert.That(RuntimeStorageData.IsReady, Is.False);

        RuntimeStorageData.PLAYER = new PlayerSerializable();
        Assert.That(RuntimeStorageData.IsReady, Is.True);
    }
}
