using System;
using System.Collections.Generic;
using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Proxy;
using PluginManager.Core.Adapters;
using Poi = PluginManager.Api.Contracts.Poi;
using Vector3 = PluginManager.Api.Contracts.Vector3;
using Vector3Int = PluginManager.Api.Contracts.Vector3Int;

namespace PluginManager.Core.Capabilities.Utils;

public class PoiUtil : ProxyObject, IPoiUtil
{
    public string Name => nameof(PoiUtil);

    public Poi GetPoiAt(Vector3 position)
    {
        var world = GameManager.Instance?.World;
        if (world == null) return null;

        var prefab = world.GetPOIAtPosition(Vector3Adapter.ToGame(position));
        return prefab == null ? null : Build(prefab);
    }

    public Poi[] GetPoisInArea(Vector3Int min, Vector3Int max)
    {
        var world = GameManager.Instance?.World;
        if (world == null) return Array.Empty<Poi>();

        var prefabs = new List<PrefabInstance>();
        world.GetPOIsAtXZ(min.X, max.X, min.Z, max.Z, prefabs);

        var result = new List<Poi>();
        foreach (var prefab in prefabs)
        {
            if (prefab?.prefab == null) continue;
            if (prefab.prefab.Tags.Test_AnySet(DynamicPrefabDecorator.streetTileTag)) continue;
            result.Add(Build(prefab));
        }

        return result.ToArray();
    }

    private static Poi Build(PrefabInstance prefab)
    {
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
