using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new();

    public static void Register<T>(T service) where T : class
        => services[typeof(T)] = service;

    public static T Get<T>() where T : class
        => services.TryGetValue(typeof(T), out var s) ? s as T : null;

    public static void Unregister<T>() where T : class
        => services.Remove(typeof(T));
}