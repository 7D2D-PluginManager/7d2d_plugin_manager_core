using System.Collections.Generic;
using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;
using PluginManager.Core.Adapters;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.ChangeBlocks))]
public static class ChangeBlocksPatch
{
    static void Prefix(GameManager __instance, PlatformUserIdentifierAbs persistentPlayerId,
        List<BlockChangeInfo> _blocksToChange)
    {
        if (persistentPlayerId == null || _blocksToChange == null) return;
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;

        var world = __instance.World;
        if (world == null) return;

        var playerData = __instance.persistentPlayers?.GetPlayerData(persistentPlayerId);
        if (playerData == null || playerData.EntityId == -1) return;

        var entityId = playerData.EntityId;

        for (int i = _blocksToChange.Count - 1; i >= 0; i--)
        {
            var change = _blocksToChange[i];
            if (change == null || !change.bChangeBlockValue) continue;

            var blockValue = change.blockValue;
            if (blockValue.isair || blockValue.Block == null) continue;
            if (!change.blockValueRef.TryGetBlockPos(out var pos)) continue;

            var evt = new BlockPlacedEvent(entityId, Vector3IntAdapter.FromGame(pos), blockValue.Block.blockName);
            var result = ModContext.EventRunner.Publish(evt, HookMode.Pre);

            if (result != HookResult.Handled && result != HookResult.Stop) continue;

            _blocksToChange.RemoveAt(i);
            world.SetBlockRPC(change.blockValueRef, BlockValue.Air);
        }
    }
}
