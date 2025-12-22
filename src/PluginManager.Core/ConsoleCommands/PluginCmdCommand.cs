using System.Collections.Generic;
using System.Linq;
using PluginManager.Core.Adapters;
using PluginManager.Core.Commands;

namespace PluginManager.Core.ConsoleCommands;

public class PluginCmdCommand : ConsoleCmdAbstract
{
    public override string[] getCommands()
    {
        return ["plugin.cmd"];
    }

    public override string getDescription()
    {
        return "Run plugins command.";
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        if (_params.Count == 0)
        {
            SdtdConsole.Instance.Output("Usage: plugin.cmd <command> [args]");
            return;
        }

        var command = _params[0].ToLower();

        if (ModContext.CommandRegistry.TryGetCommand(command, out var commandDefinition))
        {
            var args = _params.Skip(1).ToList();
            var clientInfo = ClientInfoAdapter.FromGame(_senderInfo.RemoteClientInfo);
            var commandCtx = new CommandContext(args, clientInfo);
            commandDefinition.Callback.Invoke(commandCtx);
        }
        else
        {
            SdtdConsole.Instance.Output($"Unknown command '{command}'.");
        }
    }
}