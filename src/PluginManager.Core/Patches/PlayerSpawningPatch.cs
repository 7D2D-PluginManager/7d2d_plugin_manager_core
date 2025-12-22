using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;
using PluginManager.Core.Adapters;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.RequestToSpawnPlayer))]
public static class PlayerSpawningPatch
{
    static void Postfix(ClientInfo _cInfo)
    {
        var playerSpawningEvent = new PlayerSpawningEvent(ClientInfoAdapter.FromGame(_cInfo));
        
        var result = ModContext.EventRunner.Publish(playerSpawningEvent, HookMode.Pre);

        if (result == HookResult.Continue) 
            ModContext.EventRunner.Publish(playerSpawningEvent, HookMode.Post);
    }
}