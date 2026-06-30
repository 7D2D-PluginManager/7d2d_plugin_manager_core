using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Capabilities.Utils;

public class GameUtil : ProxyObject, IGameUtil
{
    public string Name => nameof(GameUtil);

    public string GetEntityType(int entityId)
    {
        var entity = GameManager.Instance.World.GetEntity(entityId);
        return entity == null ? null : entity.GetType().Name;
    }

    public ulong GetWorldTime() => GameManager.Instance.World.GetWorldTime();

    public int WorldTimeToDays(ulong worldTime) => GameUtils.WorldTimeToDays(worldTime);

    public int WorldTimeToHours(ulong worldTime) => GameUtils.WorldTimeToHours(worldTime);

    public int WorldTimeToMinutes(ulong worldTime) => GameUtils.WorldTimeToMinutes(worldTime);

    public bool IsBloodMoonActive()
    {
        var duskDawn = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
        return GameUtils.IsBloodMoonTime(GameManager.Instance.World.worldTime, duskDawn, GameStats.GetInt(EnumGameStats.BloodMoonDay));
    }
}