using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class SliderTweenExtentions
{
    public static Tween DOSlider(
        this Slider slider,
        float to,
        float duration,
        Ease ease = Ease.OutCubic)
    {
        if (slider == null) return null;

        float currentValue = slider.value;

        return DOTween.To(
            () => currentValue,
            value =>
            {
                currentValue = value;
                slider.value = value;
            },
            to,
            duration
        ).SetEase(ease);
    }
}
