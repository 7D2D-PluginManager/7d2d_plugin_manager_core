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
        {
            throw new FileNotFoundException($"Plugin file not found: {dllPath}", dllPath);
        }

        if (_plugins.ContainsKey(dllPath))
        {
            throw new InvalidOperationException($"Plugin '{name}' is already loaded");
        }

        var setup = new AppDomainSetup
        {
            ApplicationBase = _pluginsDirectory,
            PrivateBinPath = _rootDirectory
        };

        var domain = AppDomain.CreateDomain($"PluginDomain_{Guid.NewGuid()}", null, setup);

        var bootstrap = (PluginBootstrap)domain.CreateInstanceAndUnwrap(
            typeof(PluginBootstrap).Assembly.FullName,
            typeof(PluginBootstrap).FullName!,
            false, BindingFlags.Default, null, [dllPath],
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
        var defaultPluginFile = Path.Combine(_rootDirectory, "Config", "plugins.txt");
        var pluginNames = ReadPluginNamesFromFile(defaultPluginFile);
        LoadPlugins(pluginNames);
    }

    private IEnumerable<string> ReadPluginNamesFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Log.Error($"Plugins list file not found: {filePath}");
            return Enumerable.Empty<string>();
        }

        try
        {
            return File.ReadAllLines(filePath)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();
        }
        catch (Exception ex)
        {
            Log.Error($"Error reading plugins file '{filePath}': {ex.Message}");
            return Enumerable.Empty<string>();
        }
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
            catch (FileNotFoundException)
            {
                Log.Error($"Plugin file not found: {pluginName}");
            }
            catch (InvalidOperationException ex)
            {
                Log.Error($"Plugin already loaded or invalid: {pluginName} - {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error($"Unexpected error loading plugin '{pluginName}': {ex.Message}");
            }
        }
    }

    private string GetPluginDllPath(string name)
    {
        return Path.Combine(_pluginsDirectory, name, name + ".dll");
    }
}