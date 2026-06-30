using PluginManager.Api.Capabilities.Implementations.Events;
using PluginManager.Api.Hooks;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Capabilities.Events;

public class EventDispatcher(EventRegistry registry) : ProxyObject, IEventRunner
{
    public string Name => nameof(EventRegistry);

    public HookResult Publish<T>(T evt, HookMode mode) where T : IGameEvent
    {
        return registry.TryGetBucket(typeof(T), out var bucket) ? bucket.Invoke(evt, mode) : HookResult.Continue;
    }
}