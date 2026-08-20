using System.Collections.Generic;
using PluginManager.Api.Capabilities.Implementations.Commands;
using PluginManager.Core.Adapters;

namespace PluginManager.Core.Commands;

public class PluginConsoleCommand : ConsoleCmdAbstract
{
    private readonly IConsoleCommandDefinition _definition;

    public PluginConsoleCommand(IConsoleCommandDefinition definition)
    {
        _definition = definition;
    }

    public override int DefaultPermissionLevel => _definition.DefaultPermissionLevel;

    public override bool AllowedInMainMenu => false;

    public override string[] getCommands() => new[] { _definition.Name };

    public override string getDescription() => _definition.Description ?? string.Empty;

    public override string getHelp() => _definition.Help ?? string.Empty;

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        var sender = ClientInfoAdapter.FromGame(_senderInfo.RemoteClientInfo);
        var context = new ConsoleCommandContext(_params, sender);
        _definition.Callback.Invoke(context);
    }
}
