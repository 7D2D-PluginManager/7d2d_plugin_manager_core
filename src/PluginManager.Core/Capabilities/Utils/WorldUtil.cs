using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Proxy;
using PluginManager.Core.Adapters;
using Vector3 = PluginManager.Api.Contracts.Vector3;

namespace PluginManager.Core.Capabilities.Utils;

public class WorldUtil : ProxyObject, IWorldUtil
{
    public string Name => nameof(WorldUtil);

    public int GetTerrainHeight(int worldX, int worldZ)
    {
        var world = GameManager.Instance?.World;
        return world == null ? 0 : world.GetTerrainHeight(worldX, worldZ);
    }

    public bool CanPlayerSpawnAt(Vector3 position)
    {
        var world = GameManager.Instance?.World;
        return world != null && world.CanPlayersSpawnAtPos(Vector3Adapter.ToGame(position));
    }

    public bool IsWaterAt(Vector3 position)
    {
        var world = GameManager.Instance?.World;
        return world != null && world.IsWater(Vector3Adapter.ToGame(position));
    }
}
