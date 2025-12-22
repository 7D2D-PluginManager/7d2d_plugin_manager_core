using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(MinEventActionLogMessage), nameof(MinEventActionLogMessage.Execute))]
public static class MinEventActionLogPatch
{
    static void Prefix(MinEventActionLogMessage __instance, MinEventParams _params)
    {
        var minEventLogMessageEvent = new MinEventLogMessageEvent(__instance.message);
        ModContext.EventRunner.Publish(minEventLogMessageEvent, HookMode.Pre);
    }
}