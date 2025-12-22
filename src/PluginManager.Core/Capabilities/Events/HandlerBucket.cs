using System.Collections.Generic;
using PluginManager.Api.Capabilities.Implementations.Events;
using PluginManager.Api.Hooks;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Capabilities.Events;

internal sealed class HandlerBucket
{
    private readonly List<DelegateProxy> _preHandlers = [];
    private readonly List<DelegateProxy> _postHandlers = [];

    public void Add(DelegateProxy proxy, HookMode mode) => GetHandlers(mode).Add(proxy);

    public void Remove(DelegateProxy proxy, HookMode mode)
    {
        var handlers = GetHandlers(mode);

        for (var i = handlers.Count - 1; i >= 0; i--)
        {
            if (handlers[i].Equals(proxy))
            {
                handlers.RemoveAt(i);
            }
        }
    }

    public HookResult Invoke<T>(T evt, HookMode mode) where T : IGameEvent
    {
        var final = HookResult.Continue;
        var handlers = GetHandlers(mode);

        for (int i = 0, count = handlers.Count; i < count; i++)
        {
            var result = (HookResult)handlers[i].Invoke(evt);

            switch (result)
            {
                case HookResult.Continue: break;

                case HookResult.Changed:
                    if (final == HookResult.Continue)
                        final = HookResult.Changed;
                    break;

                case HookResult.Handled:
                    final = HookResult.Handled;
                    break;

                case HookResult.Stop: return HookResult.Stop;
            }
        }

        return final;
    }

    private List<DelegateProxy> GetHandlers(HookMode mode) => mode == HookMode.Pre ? _preHandlers : _postHandlers;
}