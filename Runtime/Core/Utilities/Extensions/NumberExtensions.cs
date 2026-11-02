using System;
using System.Globalization;
using UnityEngine;

public static class NumberExtensions
{
    // Suffix map for number formatting (K, M, B, ...)
    private static readonly string[] suffixes = { "", "K", "M", "B", "T", "Q", "S", "O", "N", "D" };


    /// <summary>
    /// Format very large BigInteger into short form (e.g., 1.23K, 4.56M).
    /// Returns "∞" if exceeds suffix table.
    /// </summary>
    public static string ToAbbreviatedString(this System.Numerics.BigInteger number)
    {
        if (number < 0) return "-" + ToAbbreviatedString(System.Numerics.BigInteger.Negate(number));
        if (number < 1000) return number.ToString();

        int index = (int)Math.Floor(System.Numerics.BigInteger.Log(number, 1000));
        if (index >= suffixes.Length) return "∞";
        //Debug.Log(index);

        double scaled = (double)number / Math.Pow(1000, index);
        return scaled.ToString("F2", CultureInfo.InvariantCulture) + suffixes[index];
    }

    /// <summary>
    /// Format int into short form (e.g., 12.3K). Negative-safe.
    /// </summary>
    public static string ToAbbreviatedString(this int number)
    {
        if (number < 0) return "-" + ToAbbreviatedString(-number);
        if (number < 1000) return number.ToString();

        // index = floor(log10(n)/3)
        int index = (int)Math.Floor(Math.Log10(number) / 3.0);
        if (index <= 0) return number.ToString();
        if (index >= suffixes.Length) return "∞";
        //Debug.Log(index);

        double scaled = number / Math.Pow(1000, index);
        return scaled.ToString("F1", CultureInfo.InvariantCulture) + suffixes[index];
    }

    /// <summary>
    /// Format numeric string using BigInteger (handles very large numbers).
    /// </summary>
    public static string ToAbbreviatedNumberString(this string numberString)
    {
        if (!System.Numerics.BigInteger.TryParse(numberString, out var number))
            return numberString;

        if (number < 0) return "-" + ToAbbreviatedNumberString(number);
        if (number < 1000) return number.ToString();

        int index = (int)Math.Floor(System.Numerics.BigInteger.Log(number, 1000));
        if (index >= suffixes.Length) return "∞";
        //Debug.Log(index);

        double scaled = (double)number / Math.Pow(1000, index);
        return scaled.ToString("F1", CultureInfo.InvariantCulture) + suffixes[index];
    }

    /// <summary>
    /// Format numeric string using BigInteger (handles very large numbers).
    /// </summary>
    public static string ToAbbreviatedNumberString(this System.Numerics.BigInteger number)
    {
        if (number < 0)
            return "-" + ToAbbreviatedNumberString(System.Numerics.BigInteger.Negate(number));

        if (number < 1000)
            return number.ToString();

        int index = (int)Math.Floor(System.Numerics.BigInteger.Log(number, 1000));
        if (index >= suffixes.Length)
            return "∞";
        //Debug.Log(index);

        double scaled = (double)number / Math.Pow(1000, index);
        return scaled.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + suffixes[index];
    }

    public static string ToMinuteSecondString(this int totalSeconds)
    {
        if (totalSeconds < 0) totalSeconds = 0;
        int m = totalSeconds / 60;
        int s = totalSeconds % 60;
        return string.Format("{0:00}:{1:00}", m, s);
    }

    public static string ToMinuteSecondString(this float seconds)
    {
        if (float.IsNaN(seconds) || float.IsInfinity(seconds)) seconds = 0f;
        return ToMinuteSecondString(Mathf.FloorToInt(seconds));
    }

    public static float Round(this float original)
    {
        return Mathf.Round(original * 100f) / 100f;
    }

    /// <summary>
    /// Convert sign of float to -1/0/1.
    /// </summary>
    public static int ToSignInt(this float original)
    {
        if (original < 0) return -1;
        if (original > 0) return 1;
        return 0;
    }

    // Remap giá trị từ [a,b] sang [c,d]
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
        => Mathf.Approximately(from1, to1)
            ? from2
            : (value - from1) / (to1 - from1) * (to2 - from2) + from2;

    // Lerp 2 giá trị
    public static float Lerp(this float a, float b, float t) => Mathf.Lerp(a, b, t);
}
