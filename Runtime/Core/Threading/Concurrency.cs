using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

[DefaultExecutionOrder(-200)]
public class Concurrency: MonoBehaviour {
    private static Concurrency _instance;
    private static readonly Queue < Action > _executionQueue = new Queue < Action > ();

    private readonly List<Action> _drain = new List<Action>();

    public void Update() {
        // Copy under lock, run outside it: actions may enqueue/block without deadlocking worker threads.
        lock(_executionQueue) {
            while (_executionQueue.Count > 0) _drain.Add(_executionQueue.Dequeue());
        }
        for (int i = 0; i < _drain.Count; i++) {
            try { _drain[i].Invoke(); }
            catch (Exception e) { Debug.LogException(e); }
        }
        _drain.Clear();
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
        throw new Exception("Concurrency could not find the Concurrency object. Create it via menu GameFoundation/Scene/Create Minimal Setup.");
        }
        return _instance;
    }

    void Awake() {
        if (_instance == null) {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else if (_instance != this) {
            Destroy(this); // duplicate dispatcher: drop the component only, keep the original and the GameObject
        }
    }

    void OnDestroy() {
        if (_instance == this) _instance = null;
    }
}