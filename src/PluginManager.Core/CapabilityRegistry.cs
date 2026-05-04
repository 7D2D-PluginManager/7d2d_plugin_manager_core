using System;
using System.Collections.Generic;
using PluginManager.Api.Capabilities;
using PluginManager.Api.Proxy;

namespace PluginManager.Core;

public sealed class CapabilityRegistry : ProxyObject, ICapabilityRegistry
{
    private readonly Dictionary<Type, Dictionary<string, object>> _instances = new();

    public void Register<T>(T capability) where T : ICapability
    {
        var type = typeof(T);

        if (!_instances.TryGetValue(type, out var dict))
        {
            dict = new Dictionary<string, object>();
            _instances[type] = dict;
        }

        if (dict.ContainsKey(capability.Name))
            throw new InvalidOperationException(
                $"Capability '{capability.Name}' for {type.Name} is already registered");

        dict[capability.Name] = capability;
    }

    public void Deregister<T>(T capability) where T : ICapability
    {
        var type = typeof(T);

        if (_instances.TryGetValue(type, out var dict))
        {
            dict.Remove(capability.Name);

            if (dict.Count == 0)
                _instances.Remove(type);
        }
    }

    public T Get<T>() where T : ICapability
    {
        var type = typeof(T);

        if (!_instances.TryGetValue(type, out var dict) || dict.Count == 0)
            throw new InvalidOperationException($"Capability {type.Name} was not found");

        if (dict.Count > 1)
            throw new InvalidOperationException(
                $"Multiple capabilities registered for {type.Name}, use Get<T>(name)");

        foreach (var value in dict.Values)
            return (T)value;

        throw new InvalidOperationException($"Capability for {type.Name} was not found");
    }

    public T Get<T>(string name) where T : ICapability
    {
        var type = typeof(T);

        if (_instances.TryGetValue(type, out var dict) &&
            dict.TryGetValue(name, out var value))
        {
            return (T)value;
        }

        throw new InvalidOperationException($"Capability '{name}' for {type.Name} was not found");
    }

    public IEnumerable<ICapability> GetAll()
    {
        foreach (var dict in _instances.Values)
        {
            foreach (var value in dict.Values)
            {
                yield return (ICapability)value;
            }
        }
    }
}