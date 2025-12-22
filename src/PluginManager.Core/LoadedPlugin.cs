using System;
using PluginManager.Api.Capabilities;

namespace PluginManager.Core;

internal class LoadedPlugin(AppDomain domain, PluginBootstrap bootstrap, PluginInfo info)
{
    private AppDomain Domain { get; } = domain;
    private PluginBootstrap Bootstrap { get; } = bootstrap;
    public PluginInfo Info { get; } = info;
    public bool IsUnloaded { get; private set; }

    public void Unload(ICapabilityRegistry registry)
    {
        if (IsUnloaded) 
            return;

        try
        {
            Bootstrap.Unload(registry);
        }
        catch (Exception ex)
        {
            Log.Error($"Error unloading plugin '{Info.Name}': {ex.Message}");
        }
        finally
        {
            try
            {
                AppDomain.Unload(Domain);
                IsUnloaded = true;
            }
            catch (Exception ex)
            {
                Log.Error($"Error unloading AppDomain for plugin '{Info.Name}': {ex.Message}");
                throw;
            }
        }
    }
}