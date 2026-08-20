using System.Collections.Generic;
using PluginManager.Api.Capabilities.Implementations.Commands;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Commands;

public class ConsoleCommandManager : ProxyObject, IConsoleCommandManager
{
    public string Name => nameof(ConsoleCommandManager);

    private readonly Dictionary<string, PluginConsoleCommand> _commands = new();

    public void RegisterCommand(IConsoleCommandDefinition definition)
    {
        var console = SdtdConsole.Instance;
        if (console == null) return;

        var name = definition.Name;
        if (string.IsNullOrEmpty(name)) return;

        if (console.m_CommandsAllVariants.ContainsKey(name))
        {
            Log.Warning($"[PluginManager] Console command '{name}' already registered, skipping.");
            return;
        }

        var command = new PluginConsoleCommand(definition);
        console.m_Commands.Add(command);
        console.m_CommandsAllVariants.Add(name, command);
        _commands[name] = command;

        var adminTools = GameManager.Instance?.adminTools;
        if (adminTools != null && definition.DefaultPermissionLevel != 0 &&
            !adminTools.Commands.IsPermissionDefined(command.GetCommands()))
        {
            adminTools.Commands.AddCommand(name, definition.DefaultPermissionLevel, _save: false);
        }
    }

    public void DeregisterCommand(IConsoleCommandDefinition definition)
    {
        var console = SdtdConsole.Instance;
        if (console == null) return;

        var name = definition.Name;
        if (string.IsNullOrEmpty(name) || !_commands.TryGetValue(name, out var command)) return;

        console.m_Commands.Remove(command);
        console.m_CommandsAllVariants.Remove(name);
        _commands.Remove(name);
    }
}
