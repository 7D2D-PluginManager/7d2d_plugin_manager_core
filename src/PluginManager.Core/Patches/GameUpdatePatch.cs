using System;
using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.Update))]
public static class GameUpdatePatch
{
    private static readonly GameUpdateEvent Event = new();

    static void Postfix()
    {
        try
        {
            ModContext.EventRunner.Publish(Event, HookMode.Post);
        }
        catch (Exception ex)
        {
            Log.Error($"Error publishing GameUpdateEvent: {ex.Message}");
        }
    }
}
