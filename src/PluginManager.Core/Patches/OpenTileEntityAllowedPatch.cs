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
        Vector3i position;
        if (_te.chunk == null)
        {
            var entity = GameManager.Instance.World.GetEntity(_te.entityId);
            position = entity != null ? new Vector3i(entity.position) : Vector3i.zero;
        }
        else
        {
            position = _te.ToWorldPos();
        }

        var tileEntityAccessAttemptEvent = new TileEntityAccessAttemptEvent(
            _te.EntityId,
            _entityIdThatOpenedIt,
            EnumMapper<TileEntityType, Api.Contracts.TileEntityType>.Map(_te.GetTileEntityType()),
            Vector3IntAdapter.FromGame(position)
        );
        var result = ModContext.EventRunner.Publish(tileEntityAccessAttemptEvent, HookMode.Pre);

        if (result == HookResult.Continue) return true;

        __result = false;
        return false;
    }
}