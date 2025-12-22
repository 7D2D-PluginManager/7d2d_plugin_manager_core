using System.Collections.Generic;
using PluginManager.Api.Capabilities.Implementations.Commands;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Commands;

public class CommandManager : ProxyObject, ICommandManager, ICommandRegistry
{
    public string Name => nameof(CommandManager);
    
    private readonly Dictionary<string, ICommandDefinition> _commands = new();

    public void RegisterCommand(ICommandDefinition definition)
    {
        _commands[definition.Name.ToLower()] = definition;
    }

    public void DeregisterCommand(ICommandDefinition definition)
    {
        _commands.Remove(definition.Name.ToLower());
    }

    public bool TryGetCommand(string name, out ICommandDefinition command)
    {
        return _commands.TryGetValue(name.ToLower(), out command);
    }

    public IEnumerable<ICommandDefinition> GetAllCommands() => _commands.Values;
}