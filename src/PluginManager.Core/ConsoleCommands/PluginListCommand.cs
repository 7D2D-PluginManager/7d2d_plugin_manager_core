using System.Collections.Generic;
using System.Linq;

namespace PluginManager.Core.ConsoleCommands;

public class PluginListCommand : ConsoleCmdAbstract
{
    public override string[] getCommands()
    {
        return ["plugin.list"];
    }

    public override string getDescription()
    {
        return "Lists all available plugins.";
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        var plugins = ModContext.PluginManager.GetLoadedPlugins().ToList();
        if (!plugins.Any())
        {
            SdtdConsole.Instance.Output("No plugins loaded.");
            return;
        }

        SdtdConsole.Instance.Output($"Loaded plugins ({plugins.Count()}):");

        SdtdConsole.Instance.Output(
            string.Format("{0,-20} {1,-10} {2,-15}",
                "Name", "Version", "Author"));

        SdtdConsole.Instance.Output(new string('-', 55));

        foreach (var plugin in plugins)
        {
            SdtdConsole.Instance.Output(
                string.Format("{0,-20} {1,-10} {2,-15}",
                    plugin.Name, plugin.Version, plugin.Author));
        }
    }
}