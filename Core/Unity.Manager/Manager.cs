using System;
using System.Collections;
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
#if USE_INAPPURCHASE
        InappPurchase.I.InitializePurchasing();
#endif
        StartCoroutine(InitializeSDK());
        Time.timeScale = 1;
        ShowLoading();
    }

    private IEnumerator InitializeSDK()
    {
#if FIREBASE
        yield return FirebaseManager.I.InitializeAsync();
        yield return new WaitUntil(() => IsFirebaseInitialized == true);
        LogSystem.LogSuccess($"[Manager] FIREBASE SDK INITIALIZED: {IsFirebaseInitialized}");
#endif
#if USE_ADMOB && ADMOB
        yield return null;
        bool umpCompleted = false;
        yield return Bacon.UMP.Instance.DOGatherConsent(() => umpCompleted = true);
        yield return new WaitUntil(() => umpCompleted);
        FirebaseManager.I.LogUMP("initialized_consent");


        yield return AdManager.I.InitializeAsync();
        yield return new WaitUntil(() => IsAdvertisementReady == true);
        LogSystem.LogSuccess($"[Manager] ADMOB SDK INITIALIZED: {IsAdvertisementReady}");
#endif
        yield return null;

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
        loadingCanvas.Show(null, 0.0f, 0.9f);
    }

    public void HideLoading(float delay = 1.0f)
    {
        IsLoadingActive = false;
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
        if (!pause) AdManager.I.CheckingOpenAd();
    }

    private void OnApplicationQuit()
    {
        RuntimeStorageData.SaveAllData();
    }
}
