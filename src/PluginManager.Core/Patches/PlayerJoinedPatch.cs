using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;
using PluginManager.Core.Adapters;
using PluginManager.Core.Mappers;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.RequestToEnterGame))]
public static class PlayerJoinedPatch
{
    static void Postfix(ClientInfo _cInfo)
    {
        var playerJoinedGameEvent = new PlayerJoinedGameEvent(ClientInfoAdapter.FromGame(_cInfo));

        var result = ModContext.EventRunner.Publish(playerJoinedGameEvent, HookMode.Pre);

        if (result == HookResult.Continue)
            ModContext.EventRunner.Publish(playerJoinedGameEvent, HookMode.Post);

        if (ModContext.GeoIpService.TryGeoIpData(_cInfo.ip, out var geoIpData))
        {
            ModContext.GeoIpDataStorage.SetGetGeoIpData(_cInfo.CrossplatformId.CombinedString, geoIpData);

            if (CountryToCultureMapper.TryGet(geoIpData.IsoCode, out var playerCulture, true))
            {
                ModContext.PlayerLanguageStore.SetLanguage(_cInfo.CrossplatformId.CombinedString, playerCulture);
            }
        }
    }
}