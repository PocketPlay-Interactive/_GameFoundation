using System.Collections;
using UnityEngine;

public sealed class InappPurchaseModule : IGameFoundationModule
{
    public int Order => 0;

    public IEnumerator Initialize()
    {
        var iap = InappPurchase.Get();
        iap.InitializePurchasing();
        float timeout = 10f; // never block startup forever if store is unreachable
        while (!iap.IsInitializationFinished && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        GameFoundationModuleRegistry.Register(new InappPurchaseModule());
    }
}
