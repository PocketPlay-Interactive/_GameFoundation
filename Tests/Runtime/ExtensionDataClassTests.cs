using NUnit.Framework;

public sealed class ExtensionDataClassTests
{
    [Test]
    public void SetAndGetPrimitiveValues()
    {
        var data = new ExtensionDataClass();

        data.Set("score", 123);
        data.Set("speed", 2.5f);
        data.Set("vip", true);
        data.Set("name", "Player One");

        Assert.That(data.Get("score", 0), Is.EqualTo(123));
        Assert.That(data.Get("speed", 0f), Is.EqualTo(2.5f));
        Assert.That(data.Get("vip", false), Is.True);
        Assert.That(data.Get("name", ""), Is.EqualTo("Player One"));
    }

    [Test]
    public void MissingValuesReturnDefaults()
    {
        var data = new ExtensionDataClass();

        Assert.That(data.Get("missing_int", 10), Is.EqualTo(10));
        Assert.That(data.Get("missing_bool", true), Is.True);
        Assert.That(data.Get("missing_string", "fallback"), Is.EqualTo("fallback"));
    }

    [Test]
    public void KeysAreCaseInsensitive()
    {
        var data = new ExtensionDataClass();
        data.Set("My_Game.Score", 42);

        Assert.That(data.Get("my_game.score", 0), Is.EqualTo(42));
    }
}
