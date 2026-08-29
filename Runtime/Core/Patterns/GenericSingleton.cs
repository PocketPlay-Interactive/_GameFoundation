public abstract class GenericSingleton<T> where T : class, new()
{
    private static T _instance;
    public static T Get() => _instance ??= new T();
}