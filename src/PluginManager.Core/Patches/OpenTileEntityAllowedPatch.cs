using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;
using PluginManager.Core.Adapters;
using PluginManager.Core.Mappers;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.OpenTileEntityAllowed))]
public static class OpenTileEntityAllowedPatch
{
    static bool Prefix(GameManager __instance, ref bool __result, int _entityIdThatOpenedIt, TileEntity _te,
        string _customUi)
    {
        var position = GetTileEntityPosition(__instance, _te);

        var tileEntityAccessAttemptEvent = new TileEntityAccessAttemptEvent(
            _entityIdThatOpenedIt,
            new Api.Contracts.TileEntity(_te.entityId,
                EnumMapper<TileEntityType, Api.Contracts.TileEntityType>.Map(_te.GetTileEntityType()),
                Vector3IntAdapter.FromGame(position)
            )
        );

        var result = ModContext.EventRunner.Publish(tileEntityAccessAttemptEvent, HookMode.Pre);

        if (result == HookResult.Continue) return true;

        __result = false;
        return false;
    }

    private static Vector3i GetTileEntityPosition(GameManager gameManager, TileEntity te)
    {
        if (te.chunk != null)
            return te.ToWorldPos();

        var entity = gameManager.World.GetEntity(te.entityId);
        return entity != null ? new Vector3i(entity.position) : Vector3i.zero;
    }
}