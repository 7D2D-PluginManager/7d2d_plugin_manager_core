using System;
using PluginManager.Api.Capabilities.Implementations.Logger;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Capabilities.Logger;

public class Logger : ProxyObject, ILogger
{
    public string Name => nameof(Logger);

    public void Out(string message)
    {
        Log.Out(message);
    }

    public void Warning(string message)
    {
        Log.Warning(message);
    }

    public void Error(string message)
    {
        Log.Error(message);
    }

    public void Exception(Exception e)
    {
        Log.Exception(e);
    }
}