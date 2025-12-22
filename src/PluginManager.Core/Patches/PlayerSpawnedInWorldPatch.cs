using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;
using PluginManager.Core.Adapters;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.PlayerSpawnedInWorld))]
public static class PlayerSpawnedInWorldPatch
{
    static void Postfix(ClientInfo _cInfo, RespawnType _respawnReason)
    {
        var playerSpawningEvent = new PlayerSpawnedInWorldEvent(ClientInfoAdapter.FromGame(_cInfo), (Api.Contracts.RespawnType)_respawnReason);
        
        var result = ModContext.EventRunner.Publish(playerSpawningEvent, HookMode.Pre);

        if (result == HookResult.Continue) 
            ModContext.EventRunner.Publish(playerSpawningEvent, HookMode.Post);
    }
}