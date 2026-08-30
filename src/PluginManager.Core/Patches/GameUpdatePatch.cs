using System;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.Update))]
public static class GameUpdatePatch
{
    private const long SecondTickIntervalMs = 1000L;
    private const long ErrorLogIntervalMs = 30000L;

    private static readonly GameUpdateEvent FrameEvent = new();
    private static readonly Stopwatch Clock = Stopwatch.StartNew();

    private static long _nextSecondTickMs;
    private static long _lastSecondTickMs;
    private static long _lastErrorMs = long.MinValue;

    static void Postfix()
    {
        if (!GameState.WorldReady)
        {
            _nextSecondTickMs = 0L;
            _lastSecondTickMs = 0L;
            return;
        }

        try
        {
            if (ModContext.Config != null && ModContext.Config.PublishGameUpdateEvent)
                ModContext.EventRunner.Publish(FrameEvent, HookMode.Post);

            var now = Clock.ElapsedMilliseconds;
            if (now < _nextSecondTickMs) return;

            _nextSecondTickMs = now + SecondTickIntervalMs;
            var delta = _lastSecondTickMs == 0L ? 0d : (now - _lastSecondTickMs) / 1000d;
            _lastSecondTickMs = now;

            ModContext.EventRunner.Publish(BuildSecondTick(delta), HookMode.Post);
        }
        catch (Exception ex)
        {
            LogThrottled(ex);
        }
    }

    private static SecondTickEvent BuildSecondTick(double delta)
    {
        var world = GameManager.Instance.World;
        var worldTime = world.GetWorldTime();
        var bloodMoonDay = GameStats.GetInt(EnumGameStats.BloodMoonDay);
        var duskDawn = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));

        return new SecondTickEvent
        {
            Now = DateTime.Now,
            DeltaSeconds = delta,
            WorldTime = worldTime,
            Day = GameUtils.WorldTimeToDays(worldTime),
            Hour = GameUtils.WorldTimeToHours(worldTime),
            Minute = GameUtils.WorldTimeToMinutes(worldTime),
            BloodMoonDay = bloodMoonDay,
            BloodMoonActive = GameUtils.IsBloodMoonTime(worldTime, duskDawn, bloodMoonDay),
            PlayerCount = world.Players == null ? 0 : world.Players.Count
        };
    }

    private static void LogThrottled(Exception ex)
    {
        var now = Clock.ElapsedMilliseconds;
        if (now - _lastErrorMs < ErrorLogIntervalMs) return;
        _lastErrorMs = now;

        var root = ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;
        Log.Error($"Error publishing SecondTickEvent: {root}");
    }
}
