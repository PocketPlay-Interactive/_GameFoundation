using System.Collections;
using UnityEngine;

public sealed class SampleRuntimeExtensionModule : IGameFoundationModule
{
    public int Order => 10;

    public IEnumerator Initialize()
    {
        LogSystem.LogSuccess("Runtime extension module initialized.");
        yield break;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        GameFoundationModuleRegistry.Register(new SampleRuntimeExtensionModule());
    }
}
