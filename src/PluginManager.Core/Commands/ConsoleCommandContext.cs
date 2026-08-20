using System.Collections.Generic;
using PluginManager.Api.Capabilities.Implementations.Commands;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Commands;

public class ConsoleCommandContext(IReadOnlyList<string> args, Api.Contracts.ClientInfo sender)
    : ProxyObject, IConsoleCommandContext
{
    public IReadOnlyList<string> Args { get; } = args;
    public Api.Contracts.ClientInfo Sender { get; } = sender;

    public void Reply(string message)
    {
        SdtdConsole.Instance.Output(message);
    }
}
