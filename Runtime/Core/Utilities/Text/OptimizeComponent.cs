using System.Text;

public static class OptimizeComponent
{
    static OptimizeComponent()
    {
        _stringBuilderOS = new StringBuilder();
    }

    private static StringBuilder _stringBuilderOS;

    public static string GetStringOptimize(params object[] pieces)
    {
        _stringBuilderOS.Clear();

        for (int i = 0; i < pieces.Length; i++)
        {
            _stringBuilderOS.Append(pieces[i]);
        }

        return _stringBuilderOS.ToString();
    }

    public static string GetStringOptimize(string[] stringArr)
    {
        _stringBuilderOS.Clear();

        for (int i = 0; i < stringArr.Length; i++)
        {
            _stringBuilderOS.Append(stringArr[i]);
        }

        return _stringBuilderOS.ToString();
    }

    public static string AddStringOptimize(this string _builder, string _str)
    {
        _stringBuilderOS.Clear();
        _stringBuilderOS.Append(_builder);
        _stringBuilderOS.Append(", ");
        _stringBuilderOS.Append(_str);
        return _stringBuilderOS.ToString();
    }

    public static string AddString(this string _builder, string _str)
    {
        _stringBuilderOS.Clear();
        _stringBuilderOS.Append(_builder);
        _stringBuilderOS.Append(_str);
        _builder = _stringBuilderOS.ToString();
        return _builder;
    }
}