using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> events = new();
    private static readonly object lockObj = new object();

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
    public static GameMessageEvent GameMessageEvent(string key, object value = null)
    {
        _gameMessageEvent.Key = key;
        _gameMessageEvent.Value = value;
        return _gameMessageEvent;
    }

    private static readonly CanvasEvent _canvasEvent = new CanvasEvent();
    public static CanvasEvent CanvasEvent(string canvasId)
    {
        _canvasEvent.CanvasId = canvasId;
        return _canvasEvent;
    }

    private static readonly PopupEvent _popupEvent = new PopupEvent();
    public static PopupEvent PopupEvent(string popupId)
    {
        _popupEvent.PopupId = popupId;
        return _popupEvent;
    }

    private static readonly TouchBeganEvent _touchBeganEvent = new TouchBeganEvent();
    private static readonly TouchHoldEvent _touchHoldEvent = new TouchHoldEvent();
    private static readonly TouchEndedEvent _touchEndedEvent = new TouchEndedEvent();

    public static TouchBeganEvent TouchBeganEvent(Vector2 position)
    {
        _touchBeganEvent.Position = position;
        return _touchBeganEvent;
    }

    public static TouchHoldEvent TouchHoldEvent(Vector2 position)
    {
        _touchHoldEvent.Position = position;
        return _touchHoldEvent;
    }

    public static TouchEndedEvent TouchEndedEvent(Vector2 position)
    {
        _touchEndedEvent.Position = position;
        return _touchEndedEvent;
    }

    private static readonly PhoneShakeEvent _phoneShakeEvent = new PhoneShakeEvent();
    public static PhoneShakeEvent PhoneShakeEvent(float magnitude)
    {
        _phoneShakeEvent.Magnitude = magnitude;
        return _phoneShakeEvent;
    }

    private static readonly SystemControlEvent _systemControlEvent = new SystemControlEvent();
    public static SystemControlEvent SystemControlEvent(SystemType system, bool? enabled = null, float? value = null, object customData = null)
    {
        _systemControlEvent.System = system;
        _systemControlEvent.SetEnabled = enabled;
        _systemControlEvent.SetValue = value;
        _systemControlEvent.CustomData = customData;
        return _systemControlEvent;
    }
}

// Event definitions
public class GameMessageEvent { public string Key; public object Value; }
public class CanvasEvent { public string CanvasId; }
public class PopupEvent { public string PopupId; }
public class TouchBeganEvent { public Vector2 Position; }
public class TouchHoldEvent { public Vector2 Position; }
public class TouchEndedEvent { public Vector2 Position; }
public class PhoneShakeEvent { public float Magnitude; }

public enum SystemType
{
    Shake,
    Touch,
    Raycast,
    Audio,
    Vibration,
    Flash
}

public class SystemControlEvent 
{ 
    public SystemType System;
    public bool? SetEnabled;
    public float? SetValue;
    public object CustomData;
}

/*
-------------------------
CÁCH SỬ DỤNG EventBus (Cải tiến)
-------------------------

// 1. Định nghĩa event
public class PlayerDiedEvent
{
    public int playerId;
}

// 2. Subscribe/Unsubscribe (Luôn pair OnEnable/OnDisable)
void OnEnable()
{
    EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
}

void OnDisable()
{
    EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
}

void OnPlayerDied(PlayerDiedEvent evt)
{
    Debug.Log("Player died: " + evt.playerId);
}

// 3. Publish event
EventBus.Publish(new PlayerDiedEvent { playerId = 123 });

// 4. Debug (Editor only)
#if UNITY_EDITOR
void OnValidate()
{
    EventBus.LogAllEvents();
    Debug.Log($"PlayerDiedEvent subscribers: {EventBus.GetSubscriberCount<PlayerDiedEvent>()}");
}
#endif

// 5. Clear all (khi restart/quit)
void OnApplicationQuit()
{
    EventBus.Clear();
}
*/