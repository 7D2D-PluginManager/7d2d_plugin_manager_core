using System.Collections.Generic;
using PluginManager.Api.Capabilities.Implementations.Commands;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Commands;

public class CommandContext(IReadOnlyList<string> args, Api.Contracts.ClientInfo clientInfo)
    : ProxyObject, ICommandContext
{
    public IReadOnlyList<string> Args { get; } = args;
    public Api.Contracts.ClientInfo ClientInfo { get; } = clientInfo;
}