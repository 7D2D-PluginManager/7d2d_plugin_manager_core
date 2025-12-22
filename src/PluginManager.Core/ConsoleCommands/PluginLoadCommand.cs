using System;
using System.Collections.Generic;

namespace PluginManager.Core.ConsoleCommands;

public class PluginLoadCommand : ConsoleCmdAbstract
{
    public override string[] getCommands()
    {
        return ["plugin.load"];
    }

    public override string getDescription()
    {
        return "Loads a plugin";
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        if (_params.Count < 1)
        {
            SdtdConsole.Instance.Output("Usage: plugin.load <dllName>");
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
            ModContext.PluginManager.LoadPlugin(dllName);
            SdtdConsole.Instance.Output($"Plugin '{dllName}' loaded successfully.");
        }
        catch (Exception ex)
        {
            SdtdConsole.Instance.Output($"Error loading plugin '{dllName}': {ex.Message}");
        }
    }
}