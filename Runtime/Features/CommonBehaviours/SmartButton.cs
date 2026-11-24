using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Press, hover and release feedback for buttons. Field names are kept for existing scenes.</summary>
[RequireComponent(typeof(ButtonSafe))]
public class SmartButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private enum ButtonMotion
    {
        PressOnly,
        HoverAndPress,
        Punch,
        Squash,
        Pop,
        Tilt
    }

    [Header("Motion")]
    [SerializeField] private Transform target;
    [SerializeField] private ButtonMotion motion = ButtonMotion.PressOnly;
    [SerializeField] private float hoverScale = 1.04f;
    [SerializeField] private float pressedScale = 0.96f;
    [SerializeField] private float punchScale = 0.08f;
    [SerializeField] private float tiltAngle = 4f;
    [SerializeField] private float duration = 0.08f;
    [SerializeField] private Ease ease = Ease.OutBack;

    [Header("Options")]
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField, Tooltip("Number of punch bounces for the Punch motion.")] private int punchVibrato = 8;
    [SerializeField, Range(0f, 1f), Tooltip("Elasticity of the Punch motion.")] private float punchElasticity = 0.8f;
    [SerializeField, Tooltip("Scale the X axis by this much for the Squash motion.")] private float squashStretch = 1.04f;

    [Header("Events")]
    [SerializeField] private UnityEvent onPressed;
    [SerializeField] private UnityEvent onReleased;

    private Button button;
    private Vector3 baseScale;
    private bool hovering;
    private bool pressing;
    private Tween scaleTween;
    private Tween rotateTween;

    public bool IsPressed => pressing;

    private void Awake()
    {
        target ??= transform;
        baseScale = target.localScale;
        button = GetComponent<Button>();
        DisableButtonTransition();
    }

    private void OnValidate()
    {
        target ??= transform;
        button = GetComponent<Button>();
        DisableButtonTransition();
    }

    private void OnDisable()
    {
        scaleTween?.Kill();
        rotateTween?.Kill();
        if (target != null)
        {
            target.localScale = baseScale;
            target.localRotation = Quaternion.identity;
        }

        hovering = false;
        pressing = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        if (!pressing && motion == ButtonMotion.HoverAndPress)
            ScaleTo(hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        // Keep the pressed look while the pointer is still held; OnPointerUp restores the scale.
        if (!pressing)
            ScaleTo(1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
            return;

        pressing = true;
        onPressed?.Invoke();

        switch (motion)
        {
            case ButtonMotion.Punch:
                scaleTween?.Kill();
                scaleTween = target.DOPunchScale(baseScale * punchScale, duration * 2f, Mathf.Max(1, punchVibrato), punchElasticity).SetUpdate(useUnscaledTime);
                break;
            case ButtonMotion.Squash:
                scaleTween?.Kill();
                scaleTween = target.DOScale(new Vector3(baseScale.x * squashStretch, baseScale.y * pressedScale, baseScale.z), duration).SetEase(ease).SetUpdate(useUnscaledTime);
                break;
            case ButtonMotion.Pop:
                ScaleTo(1f + punchScale);
                break;
            case ButtonMotion.Tilt:
                ScaleTo(pressedScale);
                rotateTween?.Kill();
                rotateTween = target.DORotate(new Vector3(0f, 0f, -tiltAngle), duration).SetEase(ease).SetUpdate(useUnscaledTime);
                break;
            default:
                ScaleTo(pressedScale);
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!pressing) return;

        pressing = false;
        onReleased?.Invoke();

        // Only Tilt rotates the target; skip the rotate tween for the other motions.
        if (motion == ButtonMotion.Tilt)
        {
            rotateTween?.Kill();
            rotateTween = target.DORotate(Vector3.zero, duration).SetEase(ease).SetUpdate(useUnscaledTime);
        }

        ScaleTo(hovering && motion == ButtonMotion.HoverAndPress ? hoverScale : 1f);
    }

    private void ScaleTo(float scale)
    {
        scaleTween?.Kill();
        scaleTween = target.DOScale(baseScale * scale, duration).SetEase(ease).SetUpdate(useUnscaledTime);
    }

    private void DisableButtonTransition()
    {
        if (button != null)
            button.transition = Selectable.Transition.None;
    }
}
