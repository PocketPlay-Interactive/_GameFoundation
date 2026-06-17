using DG.Tweening;
using UnityEngine;

public enum DWPopupEffect
{
    None,
    PopPunch,                // Punch → PopPunch (hoa mỹ hơn)
    GlideInFromLeft,         // MoveFromLeft → GlideInFromLeft
    GlideInFromRight,        // MoveFromRight → GlideInFromRight
    GlideDownFromTop,        // MoveFromTop → GlideDownFromTop
    GlideUpFromBottom,       // MoveFromBottom → GlideUpFromBottom
    ScaleAppear,             // ScaleIn → ScaleAppear
    FadeReveal,              // FadeIn → FadeReveal

    // Các hiệu ứng mới, giữ nguyên tên:
    FadeInScaleOutParent,
    SlideInFromTopParent,
    GrowFromCenterParent
}

public static class DWVisual
{
    public static void PlayPopupEffect(GameObject popup, DWPopupEffect effect, float duration = 0.5f, bool outParent = false, System.Action onComplete = null)
    {
        var rect = popup.transform as RectTransform;
        Tween tween = null;
        CanvasGroup cg = null;

        // Nếu outParent, chỉ animate child 0
        if (outParent && popup.transform.childCount > 0)
        {
            popup.SetActive(true);
            var child = popup.transform.GetChild(0).gameObject;
            rect = child.transform as RectTransform;
            cg = child.GetComponent<CanvasGroup>();
            if (cg == null) cg = child.AddComponent<CanvasGroup>();
            popup = child;
        }

        switch (effect)
        {
            case DWPopupEffect.None:
                popup.SetActive(true);
                onComplete?.Invoke();
                return;

            case DWPopupEffect.PopPunch:
                popup.SetActive(true);
                rect.localScale = Vector3.one * 0.8f;
                tween = rect.DOScale(Vector3.one, duration).SetEase(Ease.OutBack);
                break;

            case DWPopupEffect.GlideInFromLeft:
                popup.SetActive(true);
                rect.anchoredPosition = new Vector2(-Screen.width, rect.anchoredPosition.y);
                tween = rect.DOAnchorPosX(0, duration).SetEase(Ease.OutCubic);
                break;

            case DWPopupEffect.GlideInFromRight:
                popup.SetActive(true);
                rect.anchoredPosition = new Vector2(Screen.width, rect.anchoredPosition.y);
                tween = rect.DOAnchorPosX(0, duration).SetEase(Ease.OutCubic);
                break;

            case DWPopupEffect.GlideDownFromTop:
                popup.SetActive(true);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, Screen.height);
                tween = rect.DOAnchorPosY(0, duration).SetEase(Ease.OutCubic);
                break;

            case DWPopupEffect.GlideUpFromBottom:
                popup.SetActive(true);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -Screen.height);
                tween = rect.DOAnchorPosY(0, duration).SetEase(Ease.OutCubic);
                break;

            case DWPopupEffect.ScaleAppear:
                popup.SetActive(true);
                rect.localScale = Vector3.zero;
                tween = rect.DOScale(Vector3.one, duration).SetEase(Ease.OutBack);
                break;

            case DWPopupEffect.FadeReveal:
                popup.SetActive(true);
                cg = popup.GetComponent<CanvasGroup>();
                if (cg == null) cg = popup.AddComponent<CanvasGroup>();
                cg.alpha = 0;
                tween = cg.DOFade(1, duration);
                break;

            case DWPopupEffect.FadeInScaleOutParent:
                popup.SetActive(true);
                cg = popup.GetComponent<CanvasGroup>();
                if (cg == null) cg = popup.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.blocksRaycasts = false;
                cg.interactable = false;
                rect.localScale = Vector3.one * 0.8f;
                cg.DOFade(1f, duration).SetEase(Ease.OutBack);
                tween = rect.DOScale(Vector3.one, duration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => {
                        cg.blocksRaycasts = true;
                        cg.interactable = true;
                        onComplete?.Invoke();
                    });
                break;

            case DWPopupEffect.SlideInFromTopParent:
            {
                popup.SetActive(true);
                Vector3 originalPos = rect.localPosition;
                rect.localPosition = originalPos + new Vector3(0, 500f, 0);
                tween = rect.DOLocalMove(originalPos, duration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => onComplete?.Invoke());
                break;
            }
            case DWPopupEffect.GrowFromCenterParent:
            {
                popup.SetActive(true);
                rect.localScale = Vector3.zero;
                tween = rect.DOScale(Vector3.one, duration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => onComplete?.Invoke());
                break;
            }
        }

        if (tween != null && effect != DWPopupEffect.FadeInScaleOutParent)
            tween.OnComplete(() => onComplete?.Invoke());
    }
}