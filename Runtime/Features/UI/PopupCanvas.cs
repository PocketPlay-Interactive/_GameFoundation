using DG.Tweening;
using EditorCools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public abstract class PopupCanvas : MonoBehaviour
{
    public virtual void Show()
    {
        if (IsActive()) return;
        // Initialized(_PRIMARY_CALLBACK, _SECONDARY_CALLBACK, msg);
        gameObject.Show();
        OnShow();
    }

    public abstract void Initialized(UnityAction _PRIMARY_CALLBACK, UnityAction _SECONDARY_CALLBACK, params string[] msg);

    public virtual void Hide()
    {
        if (!IsActive()) return;
        gameObject.Hide();
        OnHide();
    }

    public virtual void OnShow() {}
    public virtual void OnHide() {}

    public bool IsActive() => gameObject.IsActive();
}
