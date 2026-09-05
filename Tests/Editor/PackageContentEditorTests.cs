using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class PackageContentEditorTests
{
    [Test]
    public void ModuleDocumentationAndFoundationBasicsSampleExist()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string[] roots =
        {
            projectRoot,
            Path.Combine(projectRoot, "Packages", "com.dann.gamefoundation"),
            Path.Combine(projectRoot, "Assets", "_GameFoundation")
        };

        Assert.That(roots.Any(root => File.Exists(Path.Combine(root, "Documentation", "modules.md"))), Is.True);
        Assert.That(roots.Any(root => File.Exists(Path.Combine(root, "Samples", "FoundationBasics", "Scenes", "FoundationBasics.unity"))), Is.True);
    }
}
