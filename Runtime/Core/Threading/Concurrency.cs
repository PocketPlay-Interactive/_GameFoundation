using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

[DefaultExecutionOrder(-200)]
public class Concurrency: MonoBehaviour {
    private static Concurrency _instance;
    private static readonly Queue < Action > _executionQueue = new Queue < Action > ();

    public void Update() {
        lock(_executionQueue) {
        while (_executionQueue.Count > 0) {
            _executionQueue.Dequeue().Invoke();
        }
        }
    }


    public void Enqueue(IEnumerator action) {
        lock(_executionQueue) {
            _executionQueue.Enqueue(() => {
                StartCoroutine(action);
            });
        }
    }

    // Enqueue IEnumerator with delay (seconds)
    public void Enqueue(IEnumerator action, float delaySeconds) {
        lock(_executionQueue) {
            _executionQueue.Enqueue(() => {
                StartCoroutine(DelayedCoroutine(action, delaySeconds));
            });
        }
    }


    public void Enqueue(Action action) {
        Enqueue(ActionWrapper(action));
    }

    // Enqueue Action with delay (seconds)
    public void Enqueue(Action action, float delaySeconds) {
        Enqueue(DelayedActionWrapper(action, delaySeconds));
    }
    // Coroutine for delaying IEnumerator
    IEnumerator DelayedCoroutine(IEnumerator action, float delaySeconds) {
        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);
        yield return StartCoroutine(action);
    }

    // Coroutine for delaying Action
    IEnumerator DelayedActionWrapper(Action action, float delaySeconds) {
        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);
        action();
        yield return null;
    }

    public Task EnqueueAsync(Action action) {
        var tcs = new TaskCompletionSource < bool > ();

        void WrappedAction() {
        try {
            action();
            tcs.TrySetResult(true);
        } catch (Exception ex) {
            tcs.TrySetException(ex);
        }
        }

        Enqueue(ActionWrapper(WrappedAction));
        return tcs.Task;
    }

    IEnumerator ActionWrapper(Action a) {
        yield return new WaitForEndOfFrame();
        a();
        yield
        return null;
    }

    public static bool Exists() {
        return _instance != null;
    }

    public static Concurrency Instance() {
        if (!Exists()) {
        throw new Exception("Concurrency could not find the Concurrency object. Please ensure you have added the Concurrency Prefab to your scene.");
        }
        return _instance;
    }

    void Awake() {
        if (_instance == null) {
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
        }
    }

    void OnDestroy() {
        _instance = null;
    }
}