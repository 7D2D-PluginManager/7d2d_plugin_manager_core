using System;
using System.Collections.Generic;
using PluginManager.Api.Capabilities.Implementations.Events;
using PluginManager.Api.Hooks;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Capabilities.Events;

public sealed class EventRegistry : ProxyObject, IEventHandlers
{
    public string Name => nameof(EventRegistry);

    private readonly Dictionary<Type, HandlerBucket> _handlers = new();

    public void RegisterHandler<T>(DelegateProxy proxy, HookMode mode) where T : IGameEvent
    {
        if (!_handlers.TryGetValue(typeof(T), out var bucket))
            _handlers[typeof(T)] = bucket = new HandlerBucket();

        bucket.Add(proxy, mode);
    }

    public void DeregisterHandler<T>(DelegateProxy proxy, HookMode mode) where T : IGameEvent
    {
        if (_handlers.TryGetValue(typeof(T), out var bucket))
            bucket.Remove(proxy, mode);
    }

    internal bool TryGetBucket(Type type, out HandlerBucket bucket)
        => _handlers.TryGetValue(type, out bucket);
}