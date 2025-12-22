using System;
using System.Collections.Generic;

namespace PluginManager.Core.ConsoleCommands;

public class PluginUnloadCommand : ConsoleCmdAbstract
{
    public override string[] getCommands()
    {
        return ["plugin.unload"];
    }

    public override string getDescription()
    {
        return "Unload a plugin";
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        if (_params.Count < 1)
        {
            SdtdConsole.Instance.Output("Usage: plugin.unload <pluginName>");
            return;
        }

        var dllName = _params[0].Trim();

        if (string.IsNullOrEmpty(dllName))
        {
            SdtdConsole.Instance.Output("Error: Plugin name cannot be empty.");
            return;
        }

        try
        {
            ModContext.PluginManager.UnloadPlugin(dllName);
            SdtdConsole.Instance.Output($"Plugin '{dllName}' unloaded successfully.");
        }
        catch (Exception ex)
        {
            SdtdConsole.Instance.Output($"Error unloading plugin '{dllName}': {ex.Message}");
        }
    }
}