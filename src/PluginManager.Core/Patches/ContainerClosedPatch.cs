using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;
using PluginManager.Core.Adapters;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(TEFeatureStorage), nameof(TEFeatureStorage.OnUnlockedServer))]
public static class ContainerClosedPatch
{
    static void Postfix(TEFeatureStorage __instance, int _unlockingPlayerId)
    {
        var containerClosedEvent = new ContainerClosedEvent(
            _unlockingPlayerId,
            Vector3IntAdapter.FromGame(__instance.ToWorldPos()));

        var result = ModContext.EventRunner.Publish(containerClosedEvent, HookMode.Pre);

        if (result == HookResult.Continue)
            ModContext.EventRunner.Publish(containerClosedEvent, HookMode.Post);
    }
}
