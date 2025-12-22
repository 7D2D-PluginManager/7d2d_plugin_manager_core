using HarmonyLib;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(NetPackageTELock), nameof(NetPackageTELock.ProcessPackage))]
public static class TileEntityLockPatch
{
    static void Postfix(NetPackageTELock __instance, World _world, GameManager _callbacks)
    {
        // var tileEntity = _world.GetTileEntity(___clrIdx, new Vector3i(___posX, ___posY, ___posZ));
        // if (tileEntity == null)
        // {
        //     return true;
        // }
    }
}