using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class SmartMove : MonoBehaviour
{
    public float duration = 0.5f;
    public UnityEvent OnComplete;

    private CanvasGroup _canvasGroup;

    public void MoveX(float target)
    {
        var rect = transform as RectTransform;
        if (rect != null)
            rect.DOAnchorPosX(target, duration).OnComplete(() => OnComplete?.Invoke());
        else
            transform.DOMoveX(target, duration).OnComplete(() => OnComplete?.Invoke());
    }

    public void FadeTo(float targetAlpha)
    {
        _canvasGroup ??= gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        _canvasGroup.DOFade(targetAlpha, duration).OnComplete(() => OnComplete?.Invoke());
    }
}
