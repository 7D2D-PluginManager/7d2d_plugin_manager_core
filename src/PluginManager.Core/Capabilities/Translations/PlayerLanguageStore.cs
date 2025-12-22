using System.Collections.Concurrent;
using System.Globalization;
using PluginManager.Api.Capabilities.Implementations.Translations;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Capabilities.Translations;

public class PlayerLanguageStore : ProxyObject, IPlayerLanguageStore
{
    public string Name => nameof(PlayerLanguageStore);
    
    private readonly ConcurrentDictionary<string, CultureInfo> _playerLanguages = new();
    
    public void SetLanguage(string platformId, CultureInfo cultureInfo)
    {
        _playerLanguages[platformId] = cultureInfo;
    }

    public CultureInfo GetLanguage(string platformId)
    {
        return _playerLanguages.TryGetValue(platformId, out var cultureInfo) ? cultureInfo : GetDefaultLanguage();
    }

    public CultureInfo GetDefaultLanguage()
    {
        return CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentUICulture;
    }
}