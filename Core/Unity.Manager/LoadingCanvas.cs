using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LoadingCanvas : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    private bool _isFading = false;
    private bool _isFadingIn = false;
    private float _fadeTimer = 0f;
    private float _fadeDuration = 0.2f;
    private float _startAlpha = 0f;
    private float _targetAlpha = 1f;
    private Action _onFadeComplete;

    private bool _isFilling = false;
    private float _fillTimer = 0f;
    private float _fillDuration = 1f;
    private float _fillStartValue = 0f;
    private float _fillEndValue = 1f;
    private Action _onFillComplete;

    private float _hideDelay = 0f;
    private float _hideTimer = 0f;
    private bool _waitingToHide = false;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show(Action CALLBACK_PROGRESS_COMPLETE, float startValue = 0f, float endValue = 1f, float fillTime = 1f)
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        
        _isFading = true;
        _isFadingIn = true;
        _fadeTimer = 0f;
        _startAlpha = canvasGroup.alpha;
        _targetAlpha = 1f;
        _fadeDuration = 0.2f;
        
        _onFadeComplete = () =>
        {
            _isFilling = true;
            _fillTimer = 0f;
            _fillStartValue = startValue;
            _fillEndValue = endValue;
            _fillDuration = fillTime;
            _onFillComplete = CALLBACK_PROGRESS_COMPLETE;
        };
    }

    public void Hide(float delay = 1.0f)
    {
        _waitingToHide = true;
        _hideDelay = delay;
        _hideTimer = 0f;
    }

    private void Update()
    {
        // Handle fade animation
        if (_isFading)
        {
            _fadeTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_fadeTimer / _fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(_startAlpha, _targetAlpha, t);

            if (t >= 1f)
            {
                _isFading = false;
                _onFadeComplete?.Invoke();
                _onFadeComplete = null;
            }
        }

        // Handle fill animation
        if (_isFilling)
        {
            _fillTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_fillTimer / _fillDuration);

            if (t >= 1f)
            {
                _isFilling = false;
                _onFillComplete?.Invoke();
                _onFillComplete = null;
            }
        }

        // Handle hide delay
        if (_waitingToHide)
        {
            _hideTimer += Time.unscaledDeltaTime;
            
            if (_hideTimer >= _hideDelay)
            {
                _waitingToHide = false;
                StartFadeOut();
            }
        }
    }

    private void StartFadeOut()
    {
        _isFading = true;
        _isFadingIn = false;
        _fadeTimer = 0f;
        _startAlpha = canvasGroup.alpha;
        _targetAlpha = 0f;
        _fadeDuration = 0.2f;
        
        _onFadeComplete = () =>
        {
            gameObject.SetActive(false);
        };
    }

    private void OnDisable()
    {
        _isFading = false;
        _isFilling = false;
        _waitingToHide = false;
        _onFadeComplete = null;
        _onFillComplete = null;
    }
}
