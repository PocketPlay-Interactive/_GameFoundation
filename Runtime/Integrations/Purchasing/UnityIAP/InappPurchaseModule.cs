using System.Collections;
using UnityEngine;

public sealed class InappPurchaseModule : IGameFoundationModule
{
    public int Order => 0;

    public IEnumerator Initialize()
    {
        InappPurchase.I.InitializePurchasing();
        yield break;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        GameFoundationModuleRegistry.Register(new InappPurchaseModule());
    }
}
