using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;
using PluginManager.Core.Adapters;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.PlayerDisconnected))]
public static class PlayerDisconnectPatch
{
    static void Postfix(ClientInfo _cInfo)
    {
        var playerDisconnectedEvent = new PlayerDisconnectedEvent(ClientInfoAdapter.FromGame(_cInfo));
        
        var result = ModContext.EventRunner.Publish(playerDisconnectedEvent, HookMode.Pre);

        if (result == HookResult.Continue) 
            ModContext.EventRunner.Publish(playerDisconnectedEvent, HookMode.Post);
    }
}