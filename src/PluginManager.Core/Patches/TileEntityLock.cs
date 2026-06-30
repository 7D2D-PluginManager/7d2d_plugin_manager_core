using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;
using PluginManager.Core.Adapters;
using PluginManager.Core.Mappers;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(TileEntity), nameof(TileEntity.CanLockOnServer))]
public static class TileEntityCanLockPatch
{
    static void Postfix(TileEntity __instance, int _lockingPlayerId, ref bool __result)
    {
        if (!__result) return;

        var type = EnumMapper<TileEntityType, Api.Contracts.TileEntityType>.Map(__instance.GetTileEntityType());

        if (!TileEntityAccessGuard.IsAllowed(_lockingPlayerId, -1, type, __instance.ToWorldPos()))
            __result = false;
    }
}

[HarmonyPatch(typeof(TEFeatureAbs), nameof(TEFeatureAbs.CanLockOnServer))]
public static class TileEntityFeatureCanLockPatch
{
    static void Postfix(TEFeatureAbs __instance, int _lockingPlayerID, ref bool __result)
    {
        if (!__result) return;

        var type = __instance is TEFeatureStorage
            ? Api.Contracts.TileEntityType.Loot
            : Api.Contracts.TileEntityType.None;

        if (!TileEntityAccessGuard.IsAllowed(_lockingPlayerID, -1, type, __instance.ToWorldPos()))
            __result = false;
    }
}

internal static class TileEntityAccessGuard
{
    public static bool IsAllowed(int entityId, int tileEntityId, Api.Contracts.TileEntityType type, Vector3i position)
    {
        var tileEntityAccessAttemptEvent = new TileEntityAccessAttemptEvent(
            entityId,
            new Api.Contracts.TileEntity(tileEntityId, type, Vector3IntAdapter.FromGame(position)));

        return ModContext.EventRunner.Publish(tileEntityAccessAttemptEvent, HookMode.Pre) == HookResult.Continue;
    }
}