using System.Collections;
using Dann.GameFoundation;
using UnityEngine;
using UnityEngine.Analytics;

[DefaultExecutionOrder(-90)]
public class ManagerCoordinator : SingletonGlobal<ManagerCoordinator>
{
    private Manager manager;

    protected virtual void OnEnable()
    {
        manager = Manager.Get();
        if (manager == null) return;

        manager.OnSdkInitializationRequested += InitializeSdkAsync;
        manager.OnFoundationReady += HandleFoundationReady;
        manager.OnApplicationResumed += HandleApplicationResumed;
    }

    protected virtual void OnDisable()
    {
        if (manager == null) return;

        manager.OnSdkInitializationRequested -= InitializeSdkAsync;
        manager.OnFoundationReady -= HandleFoundationReady;
        manager.OnApplicationResumed -= HandleApplicationResumed;
        manager = null;
    }

    protected virtual IEnumerator InitializeSdkAsync()
    {
#if IAPPURCHASE_ENABLE
        InappPurchase.I.InitializePurchasing();
#endif

#if FIREBASE
        Debug.Log("Initialized AnalyticsServices");
        AnalyticsService.Get().Initialize();
        yield return new WaitUntil(() => AnalyticsService.Get().IsInitialized);
        LogSystem.LogSuccess($"[ManagerCoordinator] ANALYTICS SDK INITIALIZED: {AnalyticsService.Get().IsInitialized}");
#endif

#if ADMOB
        yield return null;
#if ADMOB_UMP_OMNI
        bool umpCompleted = false;
        yield return Omni.UMP.Instance.DOGatherConsent(() => umpCompleted = true);
        yield return new WaitUntil(() => umpCompleted);
        FirebaseManager.I?.LogUMP("initialized_consent");
#else
        AnalyticsService.Get().TrackEvent($"ump_consent_skipped_consent");
#endif

        yield return AdManager.Get().InitializeAsync();
        yield return new WaitUntil(() => Manager.Get().IsAdvertisementReady);
        LogSystem.LogSuccess($"[ManagerCoordinator] ADMOB SDK INITIALIZED: {Manager.Get().IsAdvertisementReady}");
#endif

        yield break;
    }

    protected virtual void HandleFoundationReady()
    {
#if ADMOB
        AdManager.Get()?.InitializedAllOfAds();
#endif
    }

    protected virtual void HandleApplicationResumed()
    {
#if ADMOB
        AdManager.Get()?.CheckingOpenAd();
#endif
    }
}
