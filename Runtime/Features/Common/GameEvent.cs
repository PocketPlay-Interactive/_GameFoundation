using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvent
{
    public delegate void ChangeLanguage();
    public static event ChangeLanguage OnChangeLanguage;
    public static void OnChangeLanguageMethod()
        => OnChangeLanguage?.Invoke();
        
    public delegate void CreateObject(GameObject go);
    public static event CreateObject OnCreateObject;
    public static void OnCreateObjectMethod(GameObject go)
        => OnCreateObject?.Invoke(go);

    public delegate void RemoveObject(GameObject obj);
    public static event RemoveObject OnRemoveObject;
    public static void RemoveObjectMethod(GameObject obj)
        => OnRemoveObject?.Invoke(obj);

    public delegate void IAPurchase(string productId);
    public static IAPurchase OnIAPurchase;
    public static void OnIAPurchaseMethod(string productId)
        => OnIAPurchase?.Invoke(productId);

    public delegate void ReciveMessage(string mesage, params string[] args);
    public static ReciveMessage OnReciveMessage;
    public static void OnSendMessageMethod(string mesage, params string[] args)
        => OnReciveMessage?.Invoke(mesage, args);
}
