using System.Collections.Generic;
using HarmonyLib;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(NetPackageSetBlock), nameof(NetPackageSetBlock.ProcessPackage))]
public static class SetBlockPatch
{
    public static bool Prefix(ref NetPackageSetBlock __instance, World _world,
        GameManager _callbacks, PlatformUserIdentifierAbs ___persistentPlayerId, int ___localPlayerThatChanged,
        ref List<BlockChangeInfo> ___blockChanges)
    {
        //Log.Out($"Player {___persistentPlayerId}({___localPlayerThatChanged}) changed {___blockChanges.Count} blocks");
        return true;
    }
}