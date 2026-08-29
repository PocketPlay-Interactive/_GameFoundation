using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    [SerializeField] private Transform target;
    [SerializeField] private ButtonMotion motion = ButtonMotion.PressOnly;
    [SerializeField] private float hoverScale = 1.04f;
    [SerializeField] private float pressedScale = 0.96f;
    [SerializeField] private float punchScale = 0.08f;
    [SerializeField] private float tiltAngle = 4f;
    [SerializeField] private float duration = 0.08f;
    [SerializeField] private Ease ease = Ease.OutBack;

    private Button button;
    private Vector3 baseScale;
    private bool hovering;
    private bool pressing;
    private Tween scaleTween;
    private Tween rotateTween;

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
        if (!pressing)
            ScaleTo(1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
            return;

        pressing = true;

        switch (motion)
        {
            case ButtonMotion.Punch:
                scaleTween?.Kill();
                scaleTween = target.DOPunchScale(baseScale * punchScale, duration * 2f, 8, 0.8f).SetUpdate(true);
                break;
            case ButtonMotion.Squash:
                scaleTween?.Kill();
                scaleTween = target.DOScale(new Vector3(baseScale.x * 1.04f, baseScale.y * pressedScale, baseScale.z), duration).SetEase(ease).SetUpdate(true);
                break;
            case ButtonMotion.Pop:
                ScaleTo(1f + punchScale);
                break;
            case ButtonMotion.Tilt:
                ScaleTo(pressedScale);
                rotateTween?.Kill();
                rotateTween = target.DORotate(new Vector3(0f, 0f, -tiltAngle), duration).SetEase(ease).SetUpdate(true);
                break;
            default:
                ScaleTo(pressedScale);
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressing = false;
        rotateTween?.Kill();
        rotateTween = target.DORotate(Vector3.zero, duration).SetEase(ease).SetUpdate(true);
        ScaleTo(hovering && motion == ButtonMotion.HoverAndPress ? hoverScale : 1f);
    }

    private void ScaleTo(float scale)
    {
        scaleTween?.Kill();
        scaleTween = target.DOScale(baseScale * scale, duration).SetEase(ease).SetUpdate(true);
    }

    private void DisableButtonTransition()
    {
        if (button != null)
            button.transition = Selectable.Transition.None;
    }
}
