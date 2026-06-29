using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class SmartCanvas : MonoBehaviour
{
    public bool AutoFadeInOnEnable = false;
    public float FadeInDelay = 0.0f;
    public float FadeDuration = 0.3f;
    public UnityEvent OnFadeInComplete;
    public UnityEvent OnFadeOutComplete;

    private CanvasGroup _canvasGroup;
    private Tween _fadeTween;

    private void OnEnable()
    {
        if (AutoFadeInOnEnable) Concurrency.Instance().Enqueue(() => FadeIn());
    }

    private void OnDisable()
    {
        // Kill tween directly without queuing to avoid dependency on Concurrency during cleanup
        if (_fadeTween != null && _fadeTween.IsActive()) 
            _fadeTween.Kill();
    }

    public void FadeIn()
    {
        _canvasGroup ??= GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        if (_fadeTween != null && _fadeTween.IsActive()) _fadeTween.Kill();
        _fadeTween = _canvasGroup.DOFade(1f, FadeDuration).SetDelay(FadeInDelay).OnComplete(() =>
        {
            if (this != null && gameObject != null)
                OnFadeInComplete?.Invoke();
        });
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public void FadeOut()
    {
        _canvasGroup ??= GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 1f;
        _fadeTween = _canvasGroup.DOFade(0f, FadeDuration).SetDelay(FadeInDelay).OnComplete(() =>
        {
            if (this != null && gameObject != null)
                OnFadeOutComplete?.Invoke();
        });
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}