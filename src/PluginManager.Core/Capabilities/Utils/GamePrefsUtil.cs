using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Contracts;
using PluginManager.Api.Proxy;
using PluginManager.Core.Mappers;

namespace PluginManager.Core.Capabilities.Utils;

public class GamePrefsUtil : ProxyObject, IGamePrefsUtil
{
    public string Name => nameof(GamePrefsUtil);

    public string GetString(Api.Contracts.GamePrefs property)
        => GamePrefs.GetString(ToGame(property));

    public float GetFloat(Api.Contracts.GamePrefs property)
        => GamePrefs.GetFloat(ToGame(property));

    public int GetInt(Api.Contracts.GamePrefs property)
        => GamePrefs.GetInt(ToGame(property));

    public bool GetBool(Api.Contracts.GamePrefs property)
        => GamePrefs.GetBool(ToGame(property));

    public object GetObject(Api.Contracts.GamePrefs property)
        => GamePrefs.GetObject(ToGame(property));

    public GamePrefsType GetPrefType(Api.Contracts.GamePrefs property)
    {
        var gameProp = ToGame(property);
        var type = GamePrefs.GetPrefType(gameProp);

        return ToApi(type);
    }

    private static EnumGamePrefs ToGame(Api.Contracts.GamePrefs property)
        => EnumMapper<EnumGamePrefs, Api.Contracts.GamePrefs>.MapBack(property);

    private static GamePrefsType ToApi(GamePrefs.EnumType? t)
    {
        return t == null ? default : EnumMapper<GamePrefs.EnumType, GamePrefsType>.Map(t.Value);
    }
}