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
        manager.OnApplicationResumed += HandleApplicationResumed;
    }

    protected virtual void OnDisable()
    {
        if (manager == null) return;

        manager.OnSdkInitializationRequested -= InitializeSdkAsync;
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
        LogSystem.LogSuccess($"[ManagerCoordinator] Analytics Service: {AnalyticsService.Get().IsInitialized}");
#endif

#if ADMOB
        yield return null;
#if ADMOB_UMP
        yield return UMP.Instance.DOGatherConsent(EmptyCoroutine());
        AnalyticsService.Get().TrackEvent("ump_consent_initialized_consent");
#else
        AnalyticsService.Get().TrackEvent($"ump_consent_skipped_consent");
#endif

        AdvertisingService.Get().Initialize();
        yield return new WaitUntil(() => AdvertisingService.Get().IsInitialized);
        LogSystem.LogSuccess($"[ManagerCoordinator] Advertising Service: {AdvertisingService.Get().IsInitialized}");
#endif

        yield break;
    }

    private static IEnumerator EmptyCoroutine()
    {
        yield break;
    }

    protected virtual void HandleApplicationResumed()
    {
#if ADMOB
        // AdManager.Get()?.CheckingOpenAd();
#endif
    }
}
