using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Dann.GameFoundation.Tests")]

public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> events = new();
    private static readonly object lockObj = new object();
    private static volatile Thread mainThread;

    // Unity invokes this on the main thread before scene loading (play mode) and at editor load
    // (edit mode / EditMode tests). null = not ready: fails closed.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#endif
    private static void CaptureMainThread() => mainThread = Thread.CurrentThread;

    // Test hook: simulate "not ready".
    internal static void ResetMainThreadForTests(bool ready) => mainThread = ready ? Thread.CurrentThread : null;

    internal static void RequireMainThread()
    {
        var thread = mainThread;
        if (thread == null)
            throw new InvalidOperationException("Canvas operations require Unity main thread initialization.");
        if (thread != Thread.CurrentThread)
            throw new InvalidOperationException("Canvas operations must run on the Unity main thread.");
    }

    public static void Subscribe<T>(Action<T> callback)
    {
        if (callback == null)
        {
            Debug.LogWarning("[EventBus] Tried to subscribe null callback");
            return;
        }

        lock (lockObj)
        {
            if (events.TryGetValue(typeof(T), out var del))
                events[typeof(T)] = Delegate.Combine(del, callback);
            else
                events[typeof(T)] = callback;
        }
    }

    public static void Unsubscribe<T>(Action<T> callback)
    {
        if (callback == null) return;

        lock (lockObj)
        {
            if (events.TryGetValue(typeof(T), out var del))
            {
                var currentDel = Delegate.Remove(del, callback);
                if (currentDel == null) 
                    events.Remove(typeof(T));
                else 
                    events[typeof(T)] = currentDel;
            }
        }
    }

    public static void Publish<T>(T evt)
    {
        if (typeof(T) == typeof(CanvasEvent)) RequireMainThread(); // contract: CanvasEvent = main thread only

        Delegate del;
        lock (lockObj)
        {
            if (!events.TryGetValue(typeof(T), out del))
                return;
        }

        // Invoke ngoài lock để tránh deadlock
        var action = del as Action<T>;
        if (action == null)
        {
            Debug.LogWarning($"[EventBus] Invalid delegate type for event {typeof(T).Name}");
            return;
        }

        // Gọi từng callback để catch exception riêng
        foreach (Action<T> callback in action.GetInvocationList())
        {
            try
            {
                callback?.Invoke(evt);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventBus] Error in event {typeof(T).Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    // Clear all events (gọi khi cleanup/restart game)
    public static void Clear()
    {
        lock (lockObj)
        {
            events.Clear();
        }
    }

    // Debug: Xem có bao nhiêu subscriber cho event type
    public static int GetSubscriberCount<T>()
    {
        lock (lockObj)
        {
            if (events.TryGetValue(typeof(T), out var del))
            {
                return del.GetInvocationList().Length;
            }
        }
        return 0;
    }

#if UNITY_EDITOR
    // Debug: Log tất cả events đang active (chỉ dùng trong Editor)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        // Reset static data khi enter play mode trong Editor
        lock (lockObj)
        {
            events.Clear();
        }
    }

    public static void LogAllEvents()
    {
        lock (lockObj)
        {
            Debug.Log($"[EventBus] Active events: {events.Count}");
            foreach (var kvp in events)
            {
                int count = kvp.Value.GetInvocationList().Length;
                Debug.Log($"  - {kvp.Key.Name}: {count} subscriber(s)");
            }
        }
    }
#endif
}

public static class EventBusExtensions
{
    public static GameMessageEvent _gameMessageEvent = new GameMessageEvent();

    public static GameMessageEvent GameMessageEvent(string key, object value = null) =>
        new GameMessageEvent { Key = key, Value = value };

    public static void PublishGameMessage(string key, object value = null) =>
        EventBus.Publish(GameMessageEvent(key, value));

    public static CanvasEvent CanvasEvent(string canvasId) =>
        new CanvasEvent { CanvasId = canvasId };

    public static void PublishCanvas(string canvasId) =>
        EventBus.Publish(CanvasEvent(canvasId));

    public static PopupEvent PopupEvent(string popupId) =>
        new PopupEvent { PopupId = popupId };

    public static void PublishPopup(string popupId) =>
        EventBus.Publish(PopupEvent(popupId));

    public static TouchBeganEvent TouchBeganEvent(Vector2 position) =>
        new TouchBeganEvent { Position = position };

    public static void PublishTouchBegan(Vector2 position) =>
        EventBus.Publish(TouchBeganEvent(position));

    public static TouchHoldEvent TouchHoldEvent(Vector2 position) =>
        new TouchHoldEvent { Position = position };

    public static void PublishTouchHold(Vector2 position) =>
        EventBus.Publish(TouchHoldEvent(position));

    public static TouchEndedEvent TouchEndedEvent(Vector2 position) =>
        new TouchEndedEvent { Position = position };

    public static void PublishTouchEnded(Vector2 position) =>
        EventBus.Publish(TouchEndedEvent(position));

    public static DeviceMotionEvent DeviceMotionEvent(DeviceMotionType type, float magnitude) =>
        new DeviceMotionEvent { Type = type, Magnitude = magnitude };

    public static void PublishDeviceMotion(DeviceMotionType type, float magnitude) =>
        EventBus.Publish(DeviceMotionEvent(type, magnitude));

    public static PurchaseCompletedEvent PurchaseCompletedEvent(string productId) =>
        new PurchaseCompletedEvent { ProductId = productId };

    public static void PublishPurchaseCompleted(string productId) =>
        EventBus.Publish(PurchaseCompletedEvent(productId));
}

// Event definitions
public class GameMessageEvent { public string Key; public object Value; }
public class CanvasEvent { public string CanvasId; }
public class PopupEvent { public string PopupId; }
public class TouchBeganEvent { public Vector2 Position; }
public class TouchHoldEvent { public Vector2 Position; }
public class TouchEndedEvent { public Vector2 Position; }
public enum DeviceMotionType { Shake, Tilt, Inertia }
public class DeviceMotionEvent { public DeviceMotionType Type; public float Magnitude; }
public class PurchaseCompletedEvent { public string ProductId; }
