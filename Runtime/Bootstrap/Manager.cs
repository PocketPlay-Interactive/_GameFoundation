using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Manager : SingletonGlobal<Manager>
{
    [Header("Logging")]
    public bool DisableLogsInReleaseBuild = false;

    [Header("References")]
    [SerializeField] private MonoBehaviour[] extensionModules;

    private bool foundationCompleted;
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

        if (DisableLogsInReleaseBuild)
            Debug.unityLogger.logEnabled = false;
    }

    private void Start()
    {
        Time.timeScale = 1;
        ManagerCoordinator.Get()?.BeginLoading();
        StartCoroutine(InitializeBootstrap());
    }

    private IEnumerator InitializeBootstrap()
    {
        yield return InitializeSdkThroughCoordinator();
        yield return InitializeExtensionModules();

        ManagerCoordinator coordinator = ManagerCoordinator.Get();
        if (coordinator == null)
        {
            CompleteBootstrap();
            yield break;
        }

        coordinator.BeginCompletion();
        yield return new WaitUntil(() => foundationCompleted);
    }

    private IEnumerator InitializeSdkThroughCoordinator()
    {
        var handlers = OnSdkInitializationRequested?.GetInvocationList();
        if (handlers == null) yield break;

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

    public bool IsFoundationReady => foundationCompleted;

    public void CompleteBootstrap()
    {
        if (foundationCompleted) return;
        foundationCompleted = true;
        OnFoundationReady?.Invoke();

        LogSystem.LogSuccess("Foundation ready");
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
