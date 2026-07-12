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
    private bool hasSdkInitializationHandler = false;
    public event Action OnAdCallbackCompleted;
    public event Func<IEnumerator> OnSdkInitializationRequested;
    public event Action OnFirebaseInitialized;
    public event Action OnAdvertisementReady;
    public event Action OnInAppPurchaseInitialized;
    public event Action OnFoundationReady;
    public event Action OnApplicationResumed;

    protected override void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        base.Awake();

        RuntimeStorageData.ReadData();
        if (RuntimeStorageData.PLAYER.LastLoginDayOfMonth != DateTime.Now.Day)
        {
            RuntimeStorageData.PLAYER.LastLoginDayOfMonth = DateTime.Now.Day;
            RuntimeStorageData.PLAYER.LoginDayCount += 1;
        }
    }

    private void Start()
    {
        Time.timeScale = 1;
        ShowLoading();
        StartCoroutine(InitializeBootstrap());
    }

    private IEnumerator InitializeBootstrap()
    {
        yield return InitializeSdkThroughCoordinator();
        yield return InitializeExtensionModules();

#if !ADMOB
        yield return null;
        FinalizeAdSequence();
#else
        if (!hasSdkInitializationHandler)
            FinalizeAdSequence();
#endif
    }

    private IEnumerator InitializeSdkThroughCoordinator()
    {
        var handlers = OnSdkInitializationRequested?.GetInvocationList();
        hasSdkInitializationHandler = handlers != null;
        if (!hasSdkInitializationHandler) yield break;

        foreach (Func<IEnumerator> handler in handlers)
        {
            yield return handler();
        }
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

    public void NotifyFirebaseInitialized() => OnFirebaseInitialized?.Invoke();
    public void NotifyAdvertisementReady() => OnAdvertisementReady?.Invoke();
    public void NotifyInAppPurchaseInitialized() => OnInAppPurchaseInitialized?.Invoke();

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
        OnFoundationReady?.Invoke();
    }

    private void OnApplicationPause(bool pause)
    {
        RuntimeStorageData.SaveAllData();
        if (!pause) OnApplicationResumed?.Invoke();
    }

    private void OnApplicationQuit()
    {
        RuntimeStorageData.SaveAllData();
    }
}
