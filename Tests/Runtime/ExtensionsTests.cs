using System.Numerics;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;

public sealed class ExtensionsTests
{
    [TestCase(999, "999")]
    [TestCase(1500, "1.5K")]
    [TestCase(-2500000, "-2.5M")]
    public void IntAbbreviation(int value, string expected) =>
        Assert.That(value.ToAbbreviatedString(), Is.EqualTo(expected));

    [Test]
    public void BigIntegerAbbreviationOverflowsToInfinity() =>
        Assert.That(BigInteger.Pow(1000, 11).ToAbbreviatedNumberString(), Is.EqualTo("∞"));

    [Test]
    public void MinuteSecondClampsNegativeAndNaN()
    {
        Assert.That((-5).ToMinuteSecondString(), Is.EqualTo("00:00"));
        Assert.That(float.NaN.ToMinuteSecondString(), Is.EqualTo("00:00"));
        Assert.That(125.ToMinuteSecondString(), Is.EqualTo("02:05"));
    }

    [Test]
    public void RemapHandlesDegenerateRange()
    {
        Assert.That(5f.Remap(0, 10, 0, 100), Is.EqualTo(50f).Within(1e-4f));
        Assert.That(5f.Remap(3, 3, 7, 9), Is.EqualTo(7f));
    }

    [Test]
    public void ArrayHelpers()
    {
        var array = new[] { 1, 2 };
        Assert.That(array.Append(3), Is.EqualTo(new[] { 1, 2, 3 }));
        Assert.That(array.AddIfMissing(2), Is.SameAs(array));
        Assert.That(new[] { 1, 2, 3 }.MoveFirstToLast(), Is.EqualTo(new[] { 2, 3, 1 }));
        Assert.That(new[] { 1, 2, 3 }.MoveLastToFirst(), Is.EqualTo(new[] { 3, 1, 2 }));
        Assert.That(((int[])null).Append(1), Is.Null);
    }

    [Test]
    public void StringHelpers()
    {
        Assert.That("NguyenSiHiep".AddSpaceBeforeUppercase(), Is.EqualTo("Nguyen Si Hiep"));
        Assert.That("nope".ToEnum(FontStyle.Bold), Is.EqualTo(FontStyle.Bold));
        Assert.That("italic".ToEnum(FontStyle.Normal), Is.EqualTo(FontStyle.Italic));
    }

    [Test]
    public void HierarchyFinders()
    {
        var root = new GameObject("root");
        var child = new GameObject("a").transform;
        child.SetParent(root.transform);
        var leaf = new GameObject("leaf").transform;
        leaf.SetParent(child);
        try
        {
            Assert.That(root.FindChildRecursive("leaf"), Is.EqualTo(leaf));
            Assert.That(root.FindDirectChild("leaf"), Is.Null);
            Assert.That(root.HasDirectChild("a"), Is.True);
        }
        finally { Object.DestroyImmediate(root); }
    }

    [Test]
    public void ActiveStateExtensionsAreNullSafe()
    {
        LogAssert.Expect(LogType.Warning, "Show() called on a null GameObject.");
        ((GameObject)null).Show();

        var go = new GameObject("x");
        try
        {
            go.Hide();
            Assert.That(go.IsActive(), Is.False);
            go.Show();
            Assert.That(go.IsActive(), Is.True);
        }
        finally { Object.DestroyImmediate(go); }
    }
}
