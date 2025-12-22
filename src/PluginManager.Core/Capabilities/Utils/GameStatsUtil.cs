using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Contracts;
using PluginManager.Api.Proxy;
using PluginManager.Core.Mappers;

namespace PluginManager.Core.Capabilities.Utils;

public class GameStatsUtil : ProxyObject, IGameStatsUtil
{
    public string Name => nameof(GameStatsUtil);

    public string GetString(Api.Contracts.GameStats property)
        => GameStats.GetString(ToGame(property));

    public float GetFloat(Api.Contracts.GameStats property)
        => GameStats.GetFloat(ToGame(property));

    public int GetInt(Api.Contracts.GameStats property)
        => GameStats.GetInt(ToGame(property));

    public bool GetBool(Api.Contracts.GameStats property)
        => GameStats.GetBool(ToGame(property));

    public object GetObject(Api.Contracts.GameStats property)
        => GameStats.GetObject(ToGame(property));

    public GameStatsType GetStatType(Api.Contracts.GameStats property)
    {
        var gameProp = ToGame(property);
        var type = GameStats.GetStatType(gameProp);

        return ToApi(type);
    }

    private static EnumGameStats ToGame(Api.Contracts.GameStats property)
        => EnumMapper<EnumGameStats, Api.Contracts.GameStats>.MapBack(property);

    private static GameStatsType ToApi(GameStats.EnumType? t)
    {
        return t == null ? default : EnumMapper<GameStats.EnumType, GameStatsType>.Map(t.Value);
    }
}