using System.Collections;
using System.Linq;
using NUnit.Framework;

public sealed class GameFoundationModuleRegistryTests
{
    private sealed class TestModule : IGameFoundationModule
    {
        public TestModule(int order)
        {
            Order = order;
        }

        public int Order { get; }

        public IEnumerator Initialize()
        {
            yield break;
        }
    }

    private TestModule low;
    private TestModule high;

    [SetUp]
    public void SetUp()
    {
        low = new TestModule(1);
        high = new TestModule(10);
    }

    [TearDown]
    public void TearDown()
    {
        GameFoundationModuleRegistry.Unregister(low);
        GameFoundationModuleRegistry.Unregister(high);
    }

    [Test]
    public void RegisterSortsByOrderAndIgnoresDuplicateInstance()
    {
        GameFoundationModuleRegistry.Register(high);
        GameFoundationModuleRegistry.Register(low);
        GameFoundationModuleRegistry.Register(low);

        var modules = GameFoundationModuleRegistry.RegisteredModules
            .Where(module => module == low || module == high)
            .ToArray();

        Assert.That(modules, Is.EqualTo(new IGameFoundationModule[] { low, high }));
    }

    [Test]
    public void MergeWithCombinesSceneModulesAndSortsThem()
    {
        var sceneModule = new TestModule(5);
        GameFoundationModuleRegistry.Register(high);

        var modules = GameFoundationModuleRegistry.MergeWith(new[] { sceneModule }).ToArray();

        Assert.That(modules, Does.Contain(sceneModule));
        Assert.That(modules, Does.Contain(high));
        Assert.That(Array.IndexOf(modules, sceneModule), Is.LessThan(Array.IndexOf(modules, high)));
    }
}
