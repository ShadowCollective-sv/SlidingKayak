// ServiceLocator.cs — простой реестр. При желании замените на DI-контейнер.
using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _map = new();

    public static void Register<T>(T instance) => _map[typeof(T)] = instance;
    public static T Resolve<T>() => (T)_map[typeof(T)];
    public static bool TryResolve<T>(out T inst)
    {
        if (_map.TryGetValue(typeof(T), out var o)) { inst = (T)o; return true; }
        inst = default; return false;
    }
}