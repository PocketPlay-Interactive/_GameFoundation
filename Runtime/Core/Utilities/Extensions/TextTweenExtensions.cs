using TMPro;
using DG.Tweening;
using UnityEngine;

public static class TextTweenExtensions
{
    public static Tween DONumber(
        this TextMeshProUGUI text,
        int to,
        float duration,
        string prefix = "",
        string suffix = "",
        string format = "N0",
        Ease ease = Ease.OutCubic)
    {
        int currentValue = int.TryParse(text.text, out int parsedValue)
            ? parsedValue
            : 0;

        return DOTween.To(
            () => currentValue,
            value =>
            {
                currentValue = value;
                text.text = prefix + value.ToString(format) + suffix;
            },
            to,
            duration
        ).SetEase(ease);
    }
}