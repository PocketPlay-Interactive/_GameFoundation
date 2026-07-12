using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-90)]
public class ManagerCoordinator : SingletonGlobal<ManagerCoordinator>
{
    private Manager manager;

    protected virtual void OnEnable()
    {
        manager = Manager.I;
        if (manager == null) return;

        manager.OnSdkInitializationRequested += InitializeSdkAsync;
        manager.OnFirebaseInitialized += HandleFirebaseInitialized;
        manager.OnAdvertisementReady += HandleAdvertisementReady;
        manager.OnInAppPurchaseInitialized += HandleInAppPurchaseInitialized;
        manager.OnFoundationReady += HandleFoundationReady;
        manager.OnApplicationResumed += HandleApplicationResumed;
    }

    protected virtual void OnDisable()
    {
        if (manager == null) return;

        manager.OnSdkInitializationRequested -= InitializeSdkAsync;
        manager.OnFirebaseInitialized -= HandleFirebaseInitialized;
        manager.OnAdvertisementReady -= HandleAdvertisementReady;
        manager.OnInAppPurchaseInitialized -= HandleInAppPurchaseInitialized;
        manager.OnFoundationReady -= HandleFoundationReady;
        manager.OnApplicationResumed -= HandleApplicationResumed;
        manager = null;
    }

    protected virtual IEnumerator InitializeSdkAsync()
    {
#if IAPPURCHASE_ENABLE
        InappPurchase.I.InitializePurchasing();
        Manager.I.NotifyInAppPurchaseInitialized();
#endif

#if FIREBASE
        yield return FirebaseManager.I.InitializeAsync();
        yield return new WaitUntil(() => Manager.I.IsFirebaseInitialized);
        LogSystem.LogSuccess($"[ManagerCoordinator] FIREBASE SDK INITIALIZED: {Manager.I.IsFirebaseInitialized}");
#endif

#if ADMOB
        yield return null;
#if ADMOB_UMP_OMNI
        bool umpCompleted = false;
        yield return Omni.UMP.Instance.DOGatherConsent(() => umpCompleted = true);
        yield return new WaitUntil(() => umpCompleted);
        FirebaseManager.I?.LogUMP("initialized_consent");
#else
        FirebaseManager.I?.LogUMP("skipped_consent");
#endif

        yield return AdManager.I.InitializeAsync();
        yield return new WaitUntil(() => Manager.I.IsAdvertisementReady);
        LogSystem.LogSuccess($"[ManagerCoordinator] ADMOB SDK INITIALIZED: {Manager.I.IsAdvertisementReady}");
#endif

        yield break;
    }

    protected virtual void HandleFirebaseInitialized()
    {
        Manager.I.IsFirebaseInitialized = true;
    }

    protected virtual void HandleAdvertisementReady()
    {
        Manager.I.IsAdvertisementReady = true;
    }

    protected virtual void HandleInAppPurchaseInitialized()
    {
        Manager.I.IsInAppPurchaseInitialized = true;
    }

    protected virtual void HandleFoundationReady()
    {
#if ADMOB
        AdManager.I?.InitializedAllOfAds();
#endif
    }

    protected virtual void HandleApplicationResumed()
    {
#if ADMOB
        AdManager.I?.CheckingOpenAd();
#endif
    }
}
