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

    // Changing these orphans every existing save file.
    [Test]
    public void SaveKeysAreStable()
    {
        Assert.That(RuntimeStorageData.DATA_PLAYER, Is.EqualTo("player.data"));
        Assert.That(RuntimeStorageData.DATA_SETTING, Is.EqualTo("setting.data"));
    }

    [Test]
    public void ReadNewReturnsDefaultsPerType()
    {
        Assert.That(RuntimeStorageData.ReadNew<SettingSerializable>(RuntimeStorageData.DATATYPE.SETTING), Is.Not.Null);
        Assert.That(RuntimeStorageData.ReadNew<PlayerSerializable>(RuntimeStorageData.DATATYPE.PLAYER), Is.Not.Null);
        Assert.That(RuntimeStorageData.ReadNew<PlayerSerializable>(RuntimeStorageData.DATATYPE.NULL), Is.Null);
    }

    [Test]
    public void ExtensionStorageRoundTripsInMemory()
    {
        RuntimeStorageData.PLAYER = new PlayerSerializable();
        RuntimeStorageData.PLAYER.ExtensionData.Set("k", 7);
        Assert.That(RuntimeStorageData.PLAYER.ExtensionData.Get("k", 0), Is.EqualTo(7));
        Assert.That(RuntimeStorageData.PLAYER.ExtensionData.HasKey("missing"), Is.False);
    }
}
