using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Manager : SingletonGlobal<Manager>
{
    [Header("Intertitial Ad Open")]
    public bool AlwayShowInterstitialAd = true;

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
    private bool hasSdkInitializationHandler = false;
    public event Func<IEnumerator> OnSdkInitializationRequested;
    public event Action OnFoundationReady;
    public event Action OnApplicationResumed;

    protected override void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        base.Awake();

        RuntimeStorageData.ReadData();
        DateTime now = DateTime.Now;
        if (RuntimeStorageData.PLAYER.LastLogin.Date != now.Date)
        {
            RuntimeStorageData.PLAYER.LastLogin = now;
            RuntimeStorageData.PLAYER.LoginDayCount += 1;
            RuntimeStorageData.SaveAllData();
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
            LogSystem.LogSuccess($"[Manager] Extension Module: {module.GetType().Name.AddSpaceBeforeUppercase()}");
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

    public void FinalizeAdSequence()
    {
        if (hasAdCallbackCompleted) return;
        hasAdCallbackCompleted = true;
        CurrentLoadingElapsed = 14.0f;
        OnFoundationReady?.Invoke();

        LogSystem.LogSuccess("Finalize AdSequence");
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
