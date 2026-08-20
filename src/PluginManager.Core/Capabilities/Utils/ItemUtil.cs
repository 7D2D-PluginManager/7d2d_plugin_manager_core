using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Proxy;
using UnityEngine;
using ItemInfo = PluginManager.Api.Contracts.ItemInfo;

namespace PluginManager.Core.Capabilities.Utils;

public class ItemUtil : ProxyObject, IItemUtil
{
    public string Name => nameof(ItemUtil);

    public ItemInfo GetItem(string name)
    {
        var itemValue = ItemClass.GetItem(name, true);
        if (itemValue == null || itemValue.IsEmpty()) return null;

        var itemClass = itemValue.ItemClass;
        if (itemClass == null) return null;

        return new ItemInfo(itemClass.Name, itemClass.GetLocalizedItemName() ?? itemClass.Name);
    }

    public bool SpawnItemInInventory(int entityId, string name, int count)
    {
        return SpawnItemInInventory(entityId, name, count, 0);
    }

    public bool SpawnItemInInventory(int entityId, string name, int count, int quality)
    {
        if (count <= 0) return false;

        var world = GameManager.Instance?.World;
        if (world == null) return false;

        var clientInfo = ConnectionManager.Instance?.Clients?.ForEntityId(entityId);
        if (clientInfo is not { loginDone: true }) return false;

        var source = ItemClass.GetItem(name, true);
        if (source == null || source.IsEmpty()) return false;

        if (!world.Players.dict.TryGetValue(entityId, out var player)) return false;
        if (player == null || !player.IsSpawned() || player.IsDead()) return false;

        var itemValue = new ItemValue(source.type, quality, quality, false, null, 1f);
        var stack = new ItemStack(itemValue, count);

        var entityCreationData = new EntityCreationData
        {
            entityClass = EntityClass.FromString("item"),
            id = EntityFactory.nextEntityID++,
            itemStack = stack.Clone(),
            pos = player.position,
            rot = new Vector3(20f, 0f, 20f),
            lifetime = 60f,
            belongsPlayerId = entityId
        };

        var entity = EntityFactory.CreateEntity(entityCreationData);
        world.SpawnEntityInWorld(entity);
        clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entity.entityId, entityId));
        world.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Despawned);
        return true;
    }
}
