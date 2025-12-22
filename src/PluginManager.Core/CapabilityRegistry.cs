using System;
using System.Collections.Generic;
using System.Linq;
using PluginManager.Api.Capabilities;
using PluginManager.Api.Proxy;

namespace PluginManager.Core;

public sealed class CapabilityRegistry : ProxyObject, ICapabilityRegistry
{
    private readonly Dictionary<(Type, string), Lazy<object>> _instances = new();
    
    public void Register<T>(T capability) where T : ICapability
    {
        var key = (typeof(T), capability.Name);

        if (_instances.ContainsKey(key))
            throw new InvalidOperationException(
                $"Capability '{capability.Name}' for {typeof(T).Name} is already registered");

        _instances[key] = new Lazy<object>(() => capability);
    }

    public void Deregister<T>(T capability) where T : ICapability
    {
        var key = (typeof(T), capability.Name);
        _instances.Remove(key);
    }

    public T Get<T>() where T : ICapability
    {
        var matches = _instances.Keys.Where(k => k.Item1 == typeof(T)).ToList();

        return matches.Count switch
        {
            0 => throw new InvalidOperationException($"Capability {typeof(T).Name} was not found"),
            > 1 => throw new InvalidOperationException(
                $"Multiple capabilities registered for {typeof(T).Name}, use Get<T>(name)"),
            _ => (T)_instances[matches[0]].Value
        };
    }

    public T Get<T>(string name) where T : ICapability
    {
        var key = (typeof(T), name);

        if (_instances.TryGetValue(key, out var lazy))
            return (T)lazy.Value;

        throw new InvalidOperationException(
            $"Capability '{name}' for {typeof(T).Name} was not found");
    }

    public IEnumerable<ICapability> GetAll()
    {
        foreach (var lazy in _instances.Values)
        {
            yield return (ICapability)lazy.Value;
        }
    }
}