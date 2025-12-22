using PluginManager.Api.Capabilities.Implementations.Translations;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Capabilities.Translations;

public class PlayerLocalizationFactory(IPlayerLanguageStore languageStore) : ProxyObject, IPlayerLocalizationFactory
{
    public string Name => nameof(PlayerLocalization);

    public IPlayerLocalization Create(string langPath)
    {
        return new PlayerLocalization(new JsonStringLocalizer(langPath), languageStore);
    }
}
