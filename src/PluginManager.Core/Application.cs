using System;
using System.IO;
using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Commands;
using PluginManager.Api.Capabilities.Implementations.Events;
using PluginManager.Api.Capabilities.Implementations.GeoIp;
using PluginManager.Api.Capabilities.Implementations.Logger;
using PluginManager.Api.Capabilities.Implementations.Translations;
using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Core.Capabilities.Events;
using PluginManager.Core.Capabilities.GeoIp;
using PluginManager.Core.Capabilities.Localization;
using PluginManager.Core.Capabilities.Logger;
using PluginManager.Core.Capabilities.Utils;
using PluginManager.Core.Commands;

namespace PluginManager.Core;

public class Application : IModApi
{
    public void InitMod(Mod modInstance)
    {
        try
        {
            var eventRegistry = new EventRegistry();
            var eventDispatcher = new EventDispatcher(eventRegistry);
            var commandManager = new CommandManager();
            var playerLanguageStore = new PlayerLanguageStore();
            var geoIpDataStorage = new GeoIpDataStorage();

            var capabilities = new CapabilityRegistry();
            capabilities.Register<IEventHandlers>(eventRegistry);
            capabilities.Register<ILogger>(new Logger());
            capabilities.Register<ICommandManager>(commandManager);
            capabilities.Register<IPlayerUtil>(new PlayerUtil());
            capabilities.Register<IGameUtil>(new GameUtil());
            capabilities.Register<IGameStatsUtil>(new GameStatsUtil());
            capabilities.Register<IGamePrefsUtil>(new GamePrefsUtil());
            capabilities.Register<IPlayerLanguageStore>(playerLanguageStore);
            capabilities.Register<IGeoIpDataStorage>(geoIpDataStorage);

            var pluginManager = new PluginManager(modInstance.Path, capabilities);

            ModContext.Config = new Config();
            ModContext.PluginManager = pluginManager;
            ModContext.Capabilities = capabilities;
            ModContext.EventRunner = eventDispatcher;
            ModContext.CommandRegistry = commandManager;
            ModContext.PlayerLanguageStore = playerLanguageStore;
            ModContext.GeoIpDataStorage = geoIpDataStorage;

            try
            {
                ModContext.GeoIpService =
                    new GeoIpService(Path.Combine(modInstance.Path, "GeoIp", "GeoLite2-City.mmdb"));
            }
            catch (Exception ex)
            {
                Log.Error($"GeoIP init failed: {ex}");
            }

            try
            {
                var harmony = new Harmony("by.touchme.pluginmanager");
                harmony.PatchAll();
            }
            catch (Exception ex)
            {
                Log.Error($"Harmony patching failed: {ex}");
            }

            try
            {
                pluginManager.Load();
            }
            catch (Exception ex)
            {
                Log.Error($"Plugin loading failed: {ex}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Fatal init error: {ex}");
            throw;
        }
    }
}