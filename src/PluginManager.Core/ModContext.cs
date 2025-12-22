using PluginManager.Api.Capabilities;
using PluginManager.Api.Capabilities.Implementations.Commands;
using PluginManager.Api.Capabilities.Implementations.Events;
using PluginManager.Api.Capabilities.Implementations.GeoIp;
using PluginManager.Api.Capabilities.Implementations.Translations;
using PluginManager.Core.Capabilities.GeoIp;

namespace PluginManager.Core;

public static class ModContext
{
    public static Config Config { get; internal set; }
    public static IPluginManager PluginManager { get; internal set; }
    public static ICapabilityRegistry Capabilities { get; internal set; }
    public static IEventRunner EventRunner { get; internal set; }
    public static ICommandRegistry CommandRegistry { get; internal set; }
    public static IGeoIpService GeoIpService { get; internal set; }
    public static IPlayerLanguageStore PlayerLanguageStore { get; internal set; }
    public static IGeoIpDataStorage GeoIpDataStorage { get; internal set; }
}