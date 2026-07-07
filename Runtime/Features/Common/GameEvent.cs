using System;
using UnityEngine;

public static class GameEvent
{
    public delegate void MessageReceivedHandler(string message, params string[] args);

    public static event Action LanguageChanged;
    public static event Action<GameObject> ObjectCreated;
    public static event Action<GameObject> ObjectRemoved;
    public static event Action<string> PurchaseCompleted;
    public static event MessageReceivedHandler MessageReceived;

    public static void NotifyLanguageChanged()
    {
        LanguageChanged?.Invoke();
    }

    public static void NotifyObjectCreated(GameObject gameObject)
    {
        ObjectCreated?.Invoke(gameObject);
    }

    public static void NotifyObjectRemoved(GameObject gameObject)
    {
        ObjectRemoved?.Invoke(gameObject);
    }

    public static void NotifyPurchaseCompleted(string productId)
    {
        PurchaseCompleted?.Invoke(productId);
    }

    public static void NotifyMessageReceived(string message, params string[] args)
    {
        MessageReceived?.Invoke(message, args);
    }
}
