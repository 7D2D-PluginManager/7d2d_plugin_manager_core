using System.Collections.Generic;

namespace PluginManager.Core;

public interface IPluginManager
{
    void LoadPlugin(string name);
    void UnloadPlugin(string name);
    IEnumerable<PluginInfo> GetLoadedPlugins();
    void Load();
}