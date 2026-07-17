using System;
using System.Linq;
using System.Reflection;
using PluginManager.Api;
using PluginManager.Api.Capabilities;
using PluginManager.Api.Proxy;

namespace PluginManager.Core;

public class PluginBootstrap(string modulePath, string pluginId, string rootDirectory) : ProxyObject
{
    public string ModuleName => _plugin?.ModuleName ?? "(none)";
    public string ModuleVersion => _plugin?.ModuleVersion ?? "(unknown)";
    public string ModuleAuthor => _plugin?.ModuleAuthor ?? "(unknown)";
    public string ModuleDescription => _plugin?.ModuleDescription ?? "(none)";

    private IPlugin _plugin;

    public void Load(ICapabilityRegistry capabilityRegistry)
    {
        var asm = Assembly.LoadFrom(modulePath);
        var type = asm.GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract);

        if (type == null)
            throw new InvalidOperationException($"No IPlugin implementation found in {modulePath}");

        _plugin = (IPlugin)Activator.CreateInstance(type);
        _plugin.Load(new PluginContext(pluginId, rootDirectory), capabilityRegistry);
    }

    public void Unload(ICapabilityRegistry capabilityRegistry)
    {
        if (_plugin != null)
        {
            _plugin.Unload(capabilityRegistry);

            try
            {
                _plugin.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error($"Error disposing plugin '{_plugin.GetType().Name}': {ex.Message}");
            }
        }
        _plugin = null;
    }
}