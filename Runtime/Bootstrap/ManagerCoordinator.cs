using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-90)]
public class ManagerCoordinator : SingletonGlobal<ManagerCoordinator>
{
    [Header("Loading")]
    [SerializeField] private LoadingCanvas loadingCanvas;
    [SerializeField] private bool waitForExternalCompletion;
    [SerializeField] private bool autoCompleteOnTimeout = true;
    [SerializeField] private float maxCompletionWaitSeconds = 15f;

    private Manager manager;
    private bool completionStarted;
    private bool completionRequested;
    private float completionElapsed;

    protected virtual void OnEnable()
    {
        BindManager();
    }

    protected virtual void OnDisable()
    {
        if (manager == null) return;

        manager.OnSdkInitializationRequested -= InitializeSdkAsync;
        manager.OnApplicationResumed -= HandleApplicationResumed;
        manager = null;
    }

    private void BindManager()
    {
        if (manager != null) return;

        manager = Manager.Get();
        if (manager == null) return;

        manager.OnSdkInitializationRequested += InitializeSdkAsync;
        manager.OnApplicationResumed += HandleApplicationResumed;
    }

    private void Update()
    {
        if (!completionStarted || manager == null || manager.IsFoundationReady)
            return;

        completionElapsed += Time.unscaledDeltaTime;
        if (!IsCompletionReady() && (!autoCompleteOnTimeout || completionElapsed < Mathf.Max(0f, maxCompletionWaitSeconds)))
            return;

        manager.CompleteBootstrap();
        HideLoading();
    }

    public void BeginLoading()
    {
        BindManager();
        loadingCanvas?.Show(null, 0.0f, 0.9f);
    }

    public void BeginCompletion()
    {
        BindManager();
        completionStarted = true;
        completionElapsed = 0f;
        OnCompletionStarted();
    }

    public void CompleteLoading()
    {
        completionRequested = true;
    }

    public void HideLoading(float delay = 1.0f)
    {
        loadingCanvas?.Hide(delay);
    }

    public LoadingCanvas LoadingUI() => loadingCanvas;

    protected virtual bool IsCompletionReady()
    {
        return !waitForExternalCompletion || completionRequested;
    }

    protected virtual void OnCompletionStarted()
    {
    }

    protected virtual IEnumerator InitializeSdkAsync()
    {
        yield break;
    }

    protected virtual void HandleApplicationResumed()
    {
    }
}
