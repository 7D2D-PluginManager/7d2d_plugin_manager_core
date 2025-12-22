using PluginManager.Api.Capabilities.Implementations.Events;
using PluginManager.Api.Hooks;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Capabilities.Events;

public class EventDispatcher : ProxyObject, IEventRunner
{
    public string Name => nameof(EventRegistry);

    private readonly EventRegistry _registry;

    public EventDispatcher(EventRegistry registry)
    {
        _registry = registry;
    }

    public HookResult Publish<T>(T evt, HookMode mode) where T : IGameEvent
    {
        if (_registry.TryGetBucket(typeof(T), out var bucket))
        {
            return bucket.Invoke(evt, mode);
        }

        return HookResult.Continue;
    }
}