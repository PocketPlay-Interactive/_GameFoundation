using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-90)]
public class ManagerCoordinator : MonoBehaviour
{
    [Header("Loading")]
    [SerializeField] private LoadingCanvas loadingCanvas;
    [SerializeField] private bool waitForExternalCompletion = true;
    [SerializeField] private bool autoCompleteOnTimeout = true;
    [SerializeField] private float completionDelaySeconds = 15f;

    private Manager manager;
    private bool completionStarted;
    private bool completionRequested;
    private bool loadingStarted;
    private float loadingElapsed;

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

        manager = GetComponent<Manager>();
        if (manager == null) return;

        manager.OnSdkInitializationRequested += InitializeSdkAsync;
        manager.OnApplicationResumed += HandleApplicationResumed;
    }

    private void Update()
    {
        if (!loadingStarted || manager == null || manager.IsFoundationReady)
            return;

        loadingElapsed += Time.unscaledDeltaTime;
        if (!completionStarted)
            return;

        if (loadingElapsed < Mathf.Max(0f, completionDelaySeconds))
            return;

        if (!IsCompletionReady() && !autoCompleteOnTimeout)
            return;

        manager.CompleteBootstrap();
        HideLoading();
    }

    public void BeginLoading()
    {
        BindManager();
        loadingStarted = true;
        loadingElapsed = 0f;
        loadingCanvas?.Show(null, 0.0f, 0.9f);
    }

    public void BeginCompletion()
    {
        BindManager();
        completionStarted = true;
        completionRequested = false;
        OnCompletionStarted();
    }

    public void SetCompletionDelay(float seconds)
    {
        completionDelaySeconds = Mathf.Max(0f, seconds);
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
