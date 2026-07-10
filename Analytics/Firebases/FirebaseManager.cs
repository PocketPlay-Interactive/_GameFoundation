using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
#endif

public class FirebaseManager : SingletonGlobal<FirebaseManager>
{
#if FIREBASE
    Firebase.FirebaseApp app;
    private Queue<System.Action> pendingEvents = new Queue<System.Action>();
    private bool isProcessingEvents = false;

    public IEnumerator InitializeAsync()
    {
        if (Manager.I.IsFirebaseInitialized) yield break;
        Firebase.FirebaseApp.Create();
        yield return null;
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                app = Firebase.FirebaseApp.DefaultInstance;
                Manager.I.IsFirebaseInitialized = true;
                LogSystem.LogSuccess("FIREBASE INITIALIZED");
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
        yield return new WaitForEndOfFrame();
    }

    private void Update()
    {
        if (!Manager.I.IsFirebaseInitialized || isProcessingEvents || pendingEvents.Count == 0)
            return;

        isProcessingEvents = true;
        
        int processedCount = 0;
        int maxEventsPerFrame = 10; // Giới hạn số events xử lý mỗi frame
        
        while (pendingEvents.Count > 0 && processedCount < maxEventsPerFrame)
        {
            var action = pendingEvents.Dequeue();
            action?.Invoke();
            processedCount++;
        }

        if (processedCount > 0)
        {
            LogSystem.LogSuccess($"FIREBASE - Processed {processedCount} queued events. Remaining: {pendingEvents.Count}");
        }

        isProcessingEvents = false;
    }

    public void TrackingEvent(string eventName)
    {
        if (!Manager.I.IsFirebaseInitialized)
        {
            pendingEvents.Enqueue(() => TrackingEvent(eventName));
            return;
        }

        FirebaseAnalytics.LogEvent(eventName);
        LogSystem.LogSuccess($"FIREBASE TRACKING_EVENT --> {eventName}");
    }

    public void TrackingEvent(string eventName, params (string key, object value)[] parameters)
    {
        if (!Manager.I.IsFirebaseInitialized)
        {
            pendingEvents.Enqueue(() => TrackingEvent(eventName, parameters));
            return;
        }

        Parameter[] firebaseParams = new Parameter[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var (key, value) = parameters[i];
            if (value is int intValue)
            {
                firebaseParams[i] = new Parameter(key, intValue);
            }
            else if (value is long longValue)
            {
                firebaseParams[i] = new Parameter(key, longValue);
            }
            else if (value is float floatValue)
            {
                firebaseParams[i] = new Parameter(key, floatValue);
            }
            else if (value is double doubleValue)
            {
                firebaseParams[i] = new Parameter(key, doubleValue);
            }
            else if (value is string stringValue)
            {
                firebaseParams[i] = new Parameter(key, stringValue);
            }
            else
            {
                LogSystem.LogWarning($"FIREBASE TRACKING_EVENT --> Unsupported parameter type for key: {key}");
            }
        }

        FirebaseAnalytics.LogEvent(eventName, firebaseParams);
        LogSystem.LogSuccess($"FIREBASE TRACKING_EVENT --> {eventName} with {parameters.Length} parameters");
    }

    public void TrackingAdsEvent(string eventName, string placement_id, string value)
    {
        TrackingEvent(eventName, 
            ("placement_id", placement_id), 
            ("value", value)
        );
    }

    /// <summary>
    /// Log UMP consent
    /// FirebaseManager.I.LogUMP("consent_accepted");
    /// </summary>
    public void LogUMP(string status)
    {
        TrackingEvent($"ump_consent_{status}");
    }

#else
    public bool IsInitialized => false;

    public IEnumerator InitializeAsync()
    {
        yield return null;
        LogSystem.LogWarning("FIREBASE is disabled. Add 'FIREBASE' to Scripting Define Symbols to enable.");
    }

    public void TrackingEvent(string eventName) { }
    public void TrackingEvent(string eventName, params (string key, object value)[] parameters) { }
    public void TrackingAdsEvent(string eventName, string placement_id, string value) { }
    public void LogUMP(string status) { }
#endif
}
