using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Tween position, scale, rotation and alpha of this object from Inspector events or code.</summary>
public class SmartMove : MonoBehaviour
{
    [Header("Timing")]
    public float duration = 0.5f;
    public float delay;
    public Ease ease = Ease.OutQuad;
    public bool useUnscaledTime;
    [Tooltip("Kill a running tween of the same kind before starting a new one.")]
    public bool replaceRunning = true;

    [Header("Events")]
    public UnityEvent OnComplete;

    private CanvasGroup _canvasGroup;
    private Tween _move, _scale, _rotate, _fade;

    private RectTransform Rect => transform as RectTransform;

    /// <summary>Move along X (anchored position for UI, world position otherwise).</summary>
    public void MoveX(float target) => Run(ref _move, Rect != null ? Rect.DOAnchorPosX(target, duration) : transform.DOMoveX(target, duration));

    /// <summary>Move along Y (anchored position for UI, world position otherwise).</summary>
    public void MoveY(float target) => Run(ref _move, Rect != null ? Rect.DOAnchorPosY(target, duration) : transform.DOMoveY(target, duration));

    /// <summary>Move by an offset from the current position.</summary>
    public void MoveBy(Vector2 offset) => Run(ref _move, Rect != null
        ? Rect.DOAnchorPos(Rect.anchoredPosition + offset, duration)
        : transform.DOMove(transform.position + (Vector3)offset, duration));

    /// <summary>Move to a position (anchored for UI, world otherwise).</summary>
    public void MoveTo(Vector3 position) => Run(ref _move, Rect != null
        ? Rect.DOAnchorPos(position, duration)
        : transform.DOMove(position, duration));

    /// <summary>Scale uniformly to <paramref name="target"/>.</summary>
    public void ScaleTo(float target) => Run(ref _scale, transform.DOScale(target, duration));

    /// <summary>Rotate around Z by an absolute angle in degrees.</summary>
    public void RotateTo(float zAngle) => Run(ref _rotate, transform.DOLocalRotate(new Vector3(0f, 0f, zAngle), duration));

    public void FadeTo(float targetAlpha)
    {
        _canvasGroup ??= GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        Run(ref _fade, _canvasGroup.DOFade(targetAlpha, duration));
    }

    /// <summary>Stop every tween started by this component without raising <see cref="OnComplete"/>.</summary>
    public void StopAll()
    {
        _move?.Kill(); _scale?.Kill(); _rotate?.Kill(); _fade?.Kill();
        _move = _scale = _rotate = _fade = null;
    }

    private void Run(ref Tween slot, Tween tween)
    {
        if (replaceRunning && slot != null && slot.IsActive()) slot.Kill();
        slot = tween.SetDelay(delay).SetEase(ease).SetUpdate(useUnscaledTime).SetLink(gameObject).OnComplete(() => OnComplete?.Invoke());
    }

    private void OnDisable() => StopAll();
}
