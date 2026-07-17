using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using PluginManager.Api.Capabilities;

namespace PluginManager.Core;

public class PluginManager : IPluginManager
{
    private readonly string _rootDirectory;
    private readonly string _pluginsDirectory;
    private readonly ICapabilityRegistry _capabilities;

    private readonly Dictionary<string, LoadedPlugin> _plugins = new();

    public PluginManager(string rootDirectory, ICapabilityRegistry capabilities)
    {
        _rootDirectory = Path.GetFullPath(rootDirectory);
        _pluginsDirectory = Path.Combine(_rootDirectory, "Plugins");
        _capabilities = capabilities;
    }

    public void LoadPlugin(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plugin name cannot be null or empty.", nameof(name));

        var dllPath = GetPluginDllPath(name.Trim());

        if (!File.Exists(dllPath))
            throw new FileNotFoundException($"Plugin file not found: {dllPath}", dllPath);

        if (_plugins.ContainsKey(dllPath))
            throw new InvalidOperationException($"Plugin '{name}' is already loaded");

        var setup = new AppDomainSetup
        {
            ApplicationBase = _rootDirectory,
            PrivateBinPath = "Plugins"
        };

        var domain = AppDomain.CreateDomain($"PluginDomain_{Guid.NewGuid()}", null, setup);

        var bootstrap = (PluginBootstrap)domain.CreateInstanceAndUnwrap(
            typeof(PluginBootstrap).Assembly.FullName,
            typeof(PluginBootstrap).FullName!,
            false, BindingFlags.Default, null, [dllPath, name.Trim(), _rootDirectory],
            null, null
        );

        bootstrap.Load(_capabilities);

        var info = new PluginInfo
        {
            Name = bootstrap.ModuleName,
            Version = bootstrap.ModuleVersion,
            Author = bootstrap.ModuleAuthor,
            Description = bootstrap.ModuleDescription,
            Path = dllPath
        };

        _plugins[dllPath] = new LoadedPlugin(domain, bootstrap, info);
    }

    public void UnloadPlugin(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var dllPath = GetPluginDllPath(name.Trim());

        if (!_plugins.TryGetValue(dllPath, out var plugin))
            return;

        try
        {
            plugin.Unload(_capabilities);
        }
        catch (Exception ex)
        {
            Log.Error($"Error unloading plugin '{name}': {ex.Message}");
            throw;
        }
        finally
        {
            _plugins.Remove(dllPath);
        }
    }

    public IEnumerable<PluginInfo> GetLoadedPlugins()
    {
        return _plugins.Values.Select(p => p.Info).ToList();
    }

    public void Load()
    {
        if (!Directory.Exists(_pluginsDirectory))
        {
            Log.Error($"Plugins directory not found: {_pluginsDirectory}");
            return;
        }

        var pluginNames = Directory.GetFiles(_pluginsDirectory, "*.dll")
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        LoadPlugins(pluginNames);
    }

    private void LoadPlugins(IEnumerable<string> pluginNames)
    {
        foreach (var pluginName in pluginNames)
        {
            try
            {
                LoadPlugin(pluginName);
                Log.Out($"Successfully loaded plugin: {pluginName}");
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading plugin '{pluginName}': {ex.Message}");
            }
        }
    }

    private string GetPluginDllPath(string name)
    {
        return Path.Combine(_pluginsDirectory, name + ".dll");
    }
}