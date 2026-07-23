using System;
using System.Collections.Generic;
using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Proxy;
using PluginManager.Core.Adapters;
using Vector3Int = PluginManager.Api.Contracts.Vector3Int;

namespace PluginManager.Core.Capabilities.Utils;

public class ContainerUtil : ProxyObject, IContainerUtil
{
    public string Name => nameof(ContainerUtil);

    public string GetSignText(Vector3Int position)
    {
        var te = GameManager.Instance.World.GetTileEntity(Vector3IntAdapter.ToGame(position));
        return te.GetSelfOrFeature<TEFeatureSignable>()?.GetAuthoredText()?.Text;
    }

    public int[] GetContainerItemTypes(Vector3Int position)
    {
        var storage = GetStorage(position);
        if (storage == null) return Array.Empty<int>();

        var types = new List<int>();
        var items = storage.items;
        for (var i = 0; i < items.Length; i++)
        {
            if (items[i].IsEmpty()) continue;

            var type = items[i].itemValue.type;
            if (!types.Contains(type)) types.Add(type);
        }

        return types.ToArray();
    }

    public Vector3Int[] GetContainersInArea(Vector3Int min, Vector3Int max)
    {
        var world = GameManager.Instance.World;
        var result = new List<Vector3Int>();
        if (world == null) return result.ToArray();

        int minX = Math.Min(min.X, max.X), maxX = Math.Max(min.X, max.X);
        int minZ = Math.Min(min.Z, max.Z), maxZ = Math.Max(min.Z, max.Z);

        for (var cx = World.toChunkXZ(minX); cx <= World.toChunkXZ(maxX); cx++)
        for (var cz = World.toChunkXZ(minZ); cz <= World.toChunkXZ(maxZ); cz++)
        {
            if (world.GetChunkSync(cx, cz) is not Chunk chunk) continue;

            var tileEntities = chunk.GetTileEntities().list;
            for (var i = 0; i < tileEntities.Count; i++)
            {
                var storage = tileEntities[i].GetSelfOrFeature<TEFeatureStorage>();
                if (storage == null || !storage.bPlayerStorage) continue;

                var pos = tileEntities[i].ToWorldPos();
                if (pos.x < minX || pos.x > maxX || pos.z < minZ || pos.z > maxZ) continue;

                result.Add(Vector3IntAdapter.FromGame(pos));
            }
        }

        return result.ToArray();
    }

    public int MoveItemType(Vector3Int source, Vector3Int target, int itemType)
    {
        var src = GetStorage(source);
        var dst = GetStorage(target);
        if (src == null || dst == null || src == dst) return 0;

        var moved = 0;
        var items = src.items;
        for (var i = 0; i < items.Length; i++)
        {
            if (items[i].IsEmpty() || items[i].itemValue.type != itemType) continue;

            var stack = items[i];
            var before = stack.count;

            dst.TryStackItem(0, stack);
            if (stack.count > 0 && dst.AddItem(stack.Clone()))
                stack.count = 0;

            if (before - stack.count <= 0) continue;

            moved += before - stack.count;
            src.UpdateSlot(i, stack.IsEmpty() ? ItemStack.Empty : stack);
        }

        if (moved > 0) src.SetModified();
        return moved;
    }

    public Api.Contracts.DroppedBackpack GetDroppedBackpack(int entityId)
    {
        var players = GameManager.Instance.persistentPlayers;
        if (players == null) return new Api.Contracts.DroppedBackpack(false, -1, null, 0);

        foreach (var owner in players.Players.Values)
        {
            var found = false;
            uint timestamp = 0;

            owner.ProcessBackpacks(backpack =>
            {
                if (backpack.EntityID != entityId) return;
                found = true;
                timestamp = backpack.Timestamp;
            });

            if (found)
                return new Api.Contracts.DroppedBackpack(true, owner.EntityId, owner.PrimaryId?.CombinedString, (int)timestamp);
        }

        return new Api.Contracts.DroppedBackpack(false, -1, null, 0);
    }

    private static TEFeatureStorage GetStorage(Vector3Int position)
    {
        var te = GameManager.Instance.World.GetTileEntity(Vector3IntAdapter.ToGame(position));
        return te.GetSelfOrFeature<TEFeatureStorage>();
    }
}
