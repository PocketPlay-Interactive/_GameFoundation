using System.Globalization;
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
        int currentValue = ParseNumber(text.text, prefix, suffix);

        if (currentValue == to)
        {
            text.text = prefix + to.ToString(format) + suffix;
            Tween completedTween = DOTween.Sequence();
            completedTween.Complete();
            return completedTween;
        }

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

    private static int ParseNumber(string value, string prefix, string suffix)
    {
        if (!string.IsNullOrEmpty(prefix) && value.StartsWith(prefix))
            value = value.Substring(prefix.Length);

        if (!string.IsNullOrEmpty(suffix) && value.EndsWith(suffix))
            value = value.Substring(0, value.Length - suffix.Length);

        return int.TryParse(
            value,
            NumberStyles.Integer | NumberStyles.AllowThousands,
            CultureInfo.CurrentCulture,
            out int parsedValue)
            ? parsedValue
            : 0;
    }
}
