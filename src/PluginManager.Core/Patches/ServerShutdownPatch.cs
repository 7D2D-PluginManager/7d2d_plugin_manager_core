using System;
using System.Linq;
using HarmonyLib;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.Cleanup))]
public static class ServerShutdownPatch
{
    static void Prefix()
    {
        try
        {
            Log.Out("Unloading all plugins...");

            var loadedPlugins = ModContext.PluginManager.GetLoadedPlugins().ToList();
            foreach (var plugin in loadedPlugins)
            {
                try
                {
                    ModContext.PluginManager.UnloadPlugin(plugin.Name);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error unloading plugin '{plugin.Name}' during server shutdown: {ex.Message}");
                }
            }

            Log.Out("All plugins unloaded successfully during server shutdown");
        }
        catch (Exception ex)
        {
            Log.Error($"Critical error during server shutdown: {ex.Message}");
        }
    }
}