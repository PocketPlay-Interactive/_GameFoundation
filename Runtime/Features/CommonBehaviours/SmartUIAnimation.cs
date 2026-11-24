using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Composable entrance and exit animation for UI or other local transforms.
/// Built-in effects (move, scale, rotate, fade) run together; add game-specific effects by deriving
/// <see cref="SmartUIEffect"/> and listing them in <c>Custom Effects</c>.
/// </summary>
public class SmartUIAnimation : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool restoreOnDisable = true;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float duration = 0.3f;
    [SerializeField, Min(0f)] private float delay;
    [SerializeField] private Ease ease = Ease.OutCubic;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField, Tooltip("-1 loops indefinitely; 1 plays once.")] private int loops = 1;
    [SerializeField] private LoopType loopType = LoopType.Restart;

    [Header("Effects")]
    [SerializeField] private bool move;
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, -100f, 0f);
    [SerializeField] private bool scale;
    [SerializeField] private Vector3 startScaleMultiplier = Vector3.zero;
    [SerializeField] private bool rotate;
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private bool fade;
    [SerializeField, Range(0f, 1f)] private float startAlpha;
    [SerializeField, Tooltip("Also set CanvasGroup interactable and blocksRaycasts: on when shown, off on exit.")] private bool controlRaycasts;

    [Header("Custom Effects")]
    [SerializeField] private SmartUIEffect[] customEffects;

    [Header("Events")]
    [SerializeField] private UnityEvent onStarted;
    [SerializeField] private UnityEvent onCompleted;

    private Sequence sequence;
    private Vector3 basePosition;
    private Vector3 baseScale;
    private Quaternion baseRotation;
    private float baseAlpha;
    private bool captured;

    /// <summary>True while an animation is running.</summary>
    public bool IsPlaying => sequence != null && sequence.IsActive() && sequence.IsPlaying();

    public UnityEvent OnStarted => onStarted;
    public UnityEvent OnCompleted => onCompleted;

    private void Awake()
    {
        if (target == null) target = transform;
        if (canvasGroup == null) canvasGroup = target.GetComponent<CanvasGroup>();
        CaptureState();
    }

    private void OnEnable()
    {
        if (playOnEnable) Play();
    }

    /// <summary>Save the current appearance as the destination for future animations.</summary>
    public void CaptureState()
    {
        if (target == null) target = transform;
        basePosition = target.localPosition;
        baseScale = target.localScale;
        baseRotation = target.localRotation;
        if (canvasGroup != null) baseAlpha = canvasGroup.alpha;
        foreach (SmartUIEffect effect in customEffects ?? System.Array.Empty<SmartUIEffect>())
            if (effect != null) effect.Capture(target, canvasGroup);
        captured = true;
    }

    /// <summary>Animate from the configured offsets to the captured appearance.</summary>
    public void Play() => Animate(false, -1f, -1f);

    /// <summary>Same as <see cref="Play()"/> with duration and delay (seconds) overriding the Inspector values.</summary>
    public void Play(float durationOverride, float delayOverride = -1f) => Animate(false, durationOverride, delayOverride);

    /// <summary>Animate from the captured appearance to the configured offsets.</summary>
    public void PlayExit() => Animate(true, -1f, -1f);

    /// <summary>Same as <see cref="PlayExit()"/> with duration and delay (seconds) overriding the Inspector values.</summary>
    public void PlayExit(float durationOverride, float delayOverride = -1f) => Animate(true, durationOverride, delayOverride);

    /// <summary>Stop the current animation without invoking completion.</summary>
    public void Stop()
    {
        sequence?.Kill();
        sequence = null;
    }

    private void Animate(bool exit, float durationOverride, float delayOverride)
    {
        if (!captured) CaptureState();
        Stop();

        float length = Mathf.Max(0f, durationOverride >= 0f ? durationOverride : duration);
        float wait = Mathf.Max(0f, delayOverride >= 0f ? delayOverride : delay);
        Vector3 offsetPosition = basePosition + positionOffset;
        Vector3 offsetScale = Vector3.Scale(baseScale, startScaleMultiplier);
        Quaternion offsetRotation = baseRotation * Quaternion.Euler(rotationOffset);

        if (move) target.localPosition = exit ? basePosition : offsetPosition;
        if (scale) target.localScale = exit ? baseScale : offsetScale;
        if (rotate) target.localRotation = exit ? baseRotation : offsetRotation;
        if (fade && canvasGroup != null) canvasGroup.alpha = exit ? baseAlpha : startAlpha;
        else if (fade) Debug.LogWarning("[SmartUIAnimation] Fade requires a CanvasGroup on the target or a configured reference.", this);
        if (controlRaycasts && canvasGroup != null)
        {
            canvasGroup.interactable = !exit;
            canvasGroup.blocksRaycasts = !exit;
        }

        SmartUIEffect[] effects = customEffects ?? System.Array.Empty<SmartUIEffect>();
        foreach (SmartUIEffect effect in effects)
            if (effect != null) effect.Prepare(target, canvasGroup, exit);

        // Insert at `wait` (not Join after an interval): Join would start the tweens at time 0 and ignore the delay.
        sequence = DOTween.Sequence().SetUpdate(useUnscaledTime).SetLink(gameObject);
        sequence.AppendInterval(wait + length);
        if (move) sequence.Insert(wait, target.DOLocalMove(exit ? offsetPosition : basePosition, length).SetEase(ease));
        if (scale) sequence.Insert(wait, target.DOScale(exit ? offsetScale : baseScale, length).SetEase(ease));
        if (rotate) sequence.Insert(wait, target.DOLocalRotateQuaternion(exit ? offsetRotation : baseRotation, length).SetEase(ease));
        if (fade && canvasGroup != null) sequence.Insert(wait, canvasGroup.DOFade(exit ? startAlpha : baseAlpha, length).SetEase(ease));
        foreach (SmartUIEffect effect in effects)
        {
            if (effect == null) continue;
            Tween custom = effect.Build(target, canvasGroup, exit, length, ease);
            if (custom != null) sequence.Insert(wait, custom);
        }

        sequence.SetLoops(loops == -1 ? -1 : Mathf.Max(1, loops), loopType);
        sequence.OnStart(() => onStarted?.Invoke());
        sequence.OnComplete(() => { sequence = null; onCompleted?.Invoke(); });
    }

    private void OnDisable()
    {
        Stop();
        if (!restoreOnDisable || !captured || target == null) return;
        if (move) target.localPosition = basePosition;
        if (scale) target.localScale = baseScale;
        if (rotate) target.localRotation = baseRotation;
        if (fade && canvasGroup != null) canvasGroup.alpha = baseAlpha;
        foreach (SmartUIEffect effect in customEffects ?? System.Array.Empty<SmartUIEffect>())
            if (effect != null) effect.Restore(target, canvasGroup);
    }
}
