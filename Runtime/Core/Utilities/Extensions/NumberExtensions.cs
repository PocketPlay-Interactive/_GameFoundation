using System;
using System.Globalization;
using System.Numerics;
using UnityEngine;

public static class NumberExtensions
{
    // Suffix map for number formatting (K, M, B, ...)
    private static readonly string[] suffixes = { "", "K", "M", "B", "T", "Q", "S", "O", "N", "D" };

    // ---- Abbreviation: one implementation, every overload forwards to it -----------------------

    /// <summary>Format very large numbers into short form (e.g. 1.2K, 4.6M). Returns "∞" past the suffix table.</summary>
    public static string ToAbbreviatedString(this BigInteger number)
    {
        if (number < 0) return "-" + BigInteger.Negate(number).ToAbbreviatedString();
        if (number < 1000) return number.ToString();

        int index = (int)Math.Floor(BigInteger.Log(number, 1000));
        if (index >= suffixes.Length) return "∞";

        double scaled = (double)number / Math.Pow(1000, index);
        return scaled.ToString("F1", CultureInfo.InvariantCulture) + suffixes[index];
    }

    /// <summary>Format int into short form (e.g. 12.3K). Negative-safe, including int.MinValue.</summary>
    public static string ToAbbreviatedString(this int number) => ((BigInteger)number).ToAbbreviatedString();

    /// <summary>Format a numeric string (any length). Returns the input unchanged when it is not a number.</summary>
    public static string ToAbbreviatedNumberString(this string numberString) =>
        BigInteger.TryParse(numberString, out var number) ? number.ToAbbreviatedString() : numberString;

    public static string ToAbbreviatedNumberString(this BigInteger number) => number.ToAbbreviatedString();

    // ---- Time -----------------------------------------------------------------------------------

    public static string ToMinuteSecondString(this int totalSeconds)
    {
        if (totalSeconds < 0) totalSeconds = 0;
        return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }

    public static string ToMinuteSecondString(this float seconds) =>
        ToMinuteSecondString(float.IsNaN(seconds) || float.IsInfinity(seconds) ? 0 : Mathf.FloorToInt(seconds));

    // ---- Math -----------------------------------------------------------------------------------

    /// <summary>Round to two decimals.</summary>
    public static float Round(this float original) => Mathf.Round(original * 100f) / 100f;

    /// <summary>Convert the sign of a float to -1, 0 or 1.</summary>
    public static int ToSignInt(this float original) => original < 0f ? -1 : original > 0f ? 1 : 0;

    /// <summary>Remap a value from range [from1, to1] to [from2, to2]; a degenerate source range returns from2.</summary>
    public static float Remap(this float value, float from1, float to1, float from2, float to2) =>
        Mathf.Approximately(from1, to1) ? from2 : (value - from1) / (to1 - from1) * (to2 - from2) + from2;

    public static float Lerp(this float a, float b, float t) => Mathf.Lerp(a, b, t);
}
