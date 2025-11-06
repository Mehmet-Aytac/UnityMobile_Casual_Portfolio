using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new();

    public static void Register<T>(T service)
    {
        if (service == null) throw new ArgumentNullException(nameof(service));
        services[typeof(T)] = service;
    }

    public static void Register(Type type, object service)
    {
        if (service == null) throw new ArgumentNullException(nameof(service));
        if (!type.IsInstanceOfType(service))
            throw new ArgumentException($"Service is not of type {type}");
        services[type] = service;
    }

    public static T Get<T>()
    {
        if (services.TryGetValue(typeof(T), out var service))
            return (T)service;
        throw new Exception($"Service of type {typeof(T)} not found.");
    }

    public static bool TryGet<T>(out T service)
    {
        if (services.TryGetValue(typeof(T), out var obj))
        {
            service = (T)obj;
            return true;
        }
        service = default;
        return false;
    }

    public static bool TryGet(Type type, out object service)
        => services.TryGetValue(type, out service);

    public static void Unregister<T>() => services.Remove(typeof(T));

    public static void Clear() => services.Clear();

    // Helper to get all registered service instances
    public static IEnumerable<object> GetAllServices() => services.Values;
}
