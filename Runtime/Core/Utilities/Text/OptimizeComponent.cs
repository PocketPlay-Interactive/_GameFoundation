// string.Concat is thread-safe and reentrant (no shared builder, safe if a piece's ToString() calls back in).
public static class OptimizeComponent
{
    public static string GetStringOptimize(params object[] pieces) => string.Concat(pieces);

    public static string GetStringOptimize(string[] stringArr) => string.Concat(stringArr);

    public static string AddStringOptimize(this string _builder, string _str) => string.Concat(_builder, ", ", _str);

    public static string AddString(this string _builder, string _str) => string.Concat(_builder, _str);
}
