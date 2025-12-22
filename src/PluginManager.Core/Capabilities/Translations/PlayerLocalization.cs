using Microsoft.Extensions.Localization;
using PluginManager.Api.Capabilities.Implementations.Translations;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Capabilities.Translations;

public class PlayerLocalization(IStringLocalizer localizer, IPlayerLanguageStore languageStore)
    : ProxyObject, IPlayerLocalization
{
    public string Translate(string platformId, string key, params object[] args)
    {
        var culture = languageStore.GetLanguage(platformId);

        using (new WithTemporaryCulture(culture))
        {
            return localizer[key, args];
        }
    }

    public string Translate(string platformId, string key)
    {
        var culture = languageStore.GetLanguage(platformId);

        using (new WithTemporaryCulture(culture))
        {
            return localizer[key];
        }
    }
}