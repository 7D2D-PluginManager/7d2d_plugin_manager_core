using System;
using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Proxy;
using PluginManager.Core.Adapters;
using Poi = PluginManager.Api.Contracts.Poi;
using Vector3 = PluginManager.Api.Contracts.Vector3;

namespace PluginManager.Core.Capabilities.Utils;

public class PoiUtil : ProxyObject, IPoiUtil
{
    public string Name => nameof(PoiUtil);

    public Poi GetPoiAt(Vector3 position)
    {
        var world = GameManager.Instance?.World;
        if (world == null) return null;

        var prefab = world.GetPOIAtPosition(Vector3Adapter.ToGame(position));
        if (prefab == null) return null;

        var lockInstance = prefab.lockInstance;
        var hasLock = lockInstance != null;

        return new Poi(
            Vector3IntAdapter.FromGame(prefab.boundingBoxPosition),
            Vector3IntAdapter.FromGame(prefab.boundingBoxSize),
            prefab.name,
            hasLock,
            hasLock && lockInstance.IsLocked,
            hasLock && lockInstance.CheckQuestLock(),
            hasLock ? lockInstance.LockedByEntities.ToArray() : Array.Empty<int>(),
            hasLock ? lockInstance.LockedOutUntil : 0UL);
    }
}
