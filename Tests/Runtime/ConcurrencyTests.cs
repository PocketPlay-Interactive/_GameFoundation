using System;
using NUnit.Framework;
using UnityEngine;

public sealed class ConcurrencyTests
{
    [Test]
    public void InstanceThrowsWithSetupHintWhenMissing()
    {
        if (Concurrency.Exists()) Assert.Ignore("A Concurrency object exists in the test scene.");

        var ex = Assert.Throws<Exception>(() => Concurrency.Instance());
        StringAssert.Contains("Create Minimal Setup", ex.Message);
    }

    [Test]
    public void InstanceExistsAfterAwake()
    {
        if (Concurrency.Exists()) Assert.Ignore("A Concurrency object exists in the test scene.");

        var go = new GameObject("Concurrency", typeof(Concurrency));
        try { Assert.That(Concurrency.Instance(), Is.Not.Null); }
        finally { UnityEngine.Object.DestroyImmediate(go); }
        Assert.That(Concurrency.Exists(), Is.False);
    }
}
