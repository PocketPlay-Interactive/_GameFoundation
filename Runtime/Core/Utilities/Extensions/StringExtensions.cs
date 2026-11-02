using System;
using System.Globalization;
using System.Text;

public static class StringExtensions
{
    // Reusable string builder to reduce allocations for AddSpaceBeforeUppercase
    private static readonly StringBuilder output = new StringBuilder(64);

    /// <summary>
    /// Insert spaces before uppercase letters: "NguyenSiHiep" -> "Nguyen Si Hiep".
    /// </summary>
    public static string AddSpaceBeforeUppercase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        output.Clear();
        for (int i = 0; i < input.Length; i++)
        {
            if (i > 0 && char.IsUpper(input[i])) output.Append(' ');
            output.Append(input[i]);
        }
        return output.ToString();
    }

    // Extension cho string: ToTitleCase
    public static string ToTitleCase(this string str)
        => string.IsNullOrEmpty(str)
            ? str
            : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());

    /// <summary>
    /// Safe Enum.TryParse with default fallback.
    /// </summary>
    public static T ToEnum<T>(this string value, T defaultValue) where T : struct
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        return Enum.TryParse<T>(value, true, out var result) ? result : defaultValue;
    }
}
