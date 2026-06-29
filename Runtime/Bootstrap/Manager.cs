using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Scene
{
    Loading,
    Ingame,
}

[DefaultExecutionOrder(-100)]
public class Manager : SingletonGlobal<Manager>
{
    [Header("Sdk State")]
    public bool IsFirebaseInitialized = false;
    public bool IsAdvertisementReady = false;
    public bool IsInAppPurchaseInitialized = false;

    [Header("Intertitial Ad Open")]
    public bool AlwayShowInterstitialAd = true;
    public bool IsAdvertisementIntertitialOpen = false;

    [Header("Loading State")]
    public bool IsLoadingActive = false;
    public bool AutoHideLoadingAfterTimeout = true;

    public float MaxLoadingDuration = 15f;
    public float CurrentLoadingElapsed = 0f;

    [Header("References")]
    [SerializeField] private LoadingCanvas loadingCanvas;
    [SerializeField] private MonoBehaviour[] extensionModules;

    private bool hasAdCallbackCompleted = false;
    [Header("SDK Initialization State")]
    public bool _hasInitialized = false;
    public event Action OnAdCallbackCompleted;

    protected override void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        base.Awake();

        RuntimeStorageData.ReadData();
        if (RuntimeStorageData.Player.LastLoginDay != DateTime.Now.Day)
        {
            RuntimeStorageData.Player.LastLoginDay = DateTime.Now.Day;
            RuntimeStorageData.Player.TotalLoginDays += 1;
        }
    }

    private void Start()
    {
        Time.timeScale = 1;
        ShowLoading();
#if IAPPURCHASE_ENABLE
        InappPurchase.I.InitializePurchasing();
#endif
        StartCoroutine(InitializeSDK());
    }

    private IEnumerator InitializeSDK()
    {
#if FIREBASE
        yield return FirebaseManager.I.InitializeAsync();
        yield return new WaitUntil(() => IsFirebaseInitialized == true);
        LogSystem.LogSuccess($"[Manager] FIREBASE SDK INITIALIZED: {IsFirebaseInitialized}");
#endif
#if ADMOB
        yield return null;
#if ADMOB_UMP_BACON
        bool umpCompleted = false;
        yield return Bacon.UMP.Instance.DOGatherConsent(() => umpCompleted = true);
        yield return new WaitUntil(() => umpCompleted);
        FirebaseManager.I?.LogUMP("initialized_consent");
#else
        FirebaseManager.I?.LogUMP("skipped_consent");
#endif

        yield return AdManager.I.InitializeAsync();
        yield return new WaitUntil(() => IsAdvertisementReady == true);
        LogSystem.LogSuccess($"[Manager] ADMOB SDK INITIALIZED: {IsAdvertisementReady}");
#endif

        yield return InitializeExtensionModules();

#if !ADMOB
        yield return null;
        FinalizeAdSequence();
#endif
    }

    private IEnumerator InitializeExtensionModules()
    {
        foreach (var module in GetExtensionModules())
        {
            LogSystem.LogSuccess($"[Manager] INITIALIZE MODULE: {module.GetType().Name}");
            yield return module.Initialize();
        }
    }

    private IEnumerable<IGameFoundationModule> GetExtensionModules()
    {
        var sceneModules = extensionModules == null
            ? Enumerable.Empty<IGameFoundationModule>()
            : extensionModules.OfType<IGameFoundationModule>();

        return GameFoundationModuleRegistry.MergeWith(sceneModules);
    }

    private void Update()
    {
        if (IsLoadingActive && AutoHideLoadingAfterTimeout)
        {
            CurrentLoadingElapsed += Time.deltaTime;
            if (CurrentLoadingElapsed >= MaxLoadingDuration)
            {
                HideLoading(0.0f);
            }
        }
    }

    private void ShowLoading()
    {
        IsLoadingActive = true;
        if (loadingCanvas != null)
            loadingCanvas.Show(null, 0.0f, 0.9f);
    }

    public void HideLoading(float delay = 1.0f)
    {
        IsLoadingActive = false;
        if (loadingCanvas != null)
            loadingCanvas.Hide(delay);
    }

    public LoadingCanvas LoadingUI() => loadingCanvas;
    public bool IsCanShowAdOpen()
    {
        if (CurrentLoadingElapsed >= MaxLoadingDuration)
            return false;
        return true;
    }

    public void FinalizeAdSequence()
    {
        Debug.Log("[Manager] FinalizeAdSequence called");
        if (hasAdCallbackCompleted) return;
        hasAdCallbackCompleted = true;
        _hasInitialized = true;
        CurrentLoadingElapsed = 14.0f;
        OnAdCallbackCompleted?.Invoke();

        AdManager.I?.InitializedAllOfAds();
    }

    private void OnApplicationPause(bool pause)
    {
        RuntimeStorageData.SaveAllData();
        if (!pause) AdManager.I?.CheckingOpenAd();
    }

    private void OnApplicationQuit()
    {
        RuntimeStorageData.SaveAllData();
    }
}
