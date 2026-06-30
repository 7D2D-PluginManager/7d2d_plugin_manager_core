using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;
using PluginManager.Core.Adapters;
using PluginManager.Core.Mappers;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(NetPackageLockRequest), nameof(NetPackageLockRequest.ProcessPackage))]
public static class LockRequestPatch
{
    static bool Prefix(NetPackageLockRequest __instance, World _world)
    {
        if (!__instance.locking)
        {
            return true;
        }

        var targets = __instance.targets;
        if (targets == null || targets.Length == 0)
            return false;

        ILockTarget target = null;
        foreach (var t in targets)
        {
            if (t == null)
                continue;

            target = t;
        }

        switch (target)
        {
            case null:
                break;
            case TileEntity entity:
            {
                var tileEntityAccessAttemptEvent = new TileEntityAccessAttemptEvent(
                    __instance.Sender.entityId,
                    new Api.Contracts.TileEntity(-1,
                        EnumMapper<TileEntityType, Api.Contracts.TileEntityType>.Map(entity.GetTileEntityType()),
                        Vector3IntAdapter.FromGame(entity.ToWorldPos())
                    )
                );

                var result = ModContext.EventRunner.Publish(tileEntityAccessAttemptEvent, HookMode.Pre);

                if (result != HookResult.Continue)
                {
                    NetPackageLockResponseHelper.SetupAndSend(
                        __instance.Sender,
                        false,
                        "Rejected by PluginManager",
                        targets,
                        __instance.context,
                        __instance.channel
                    );
                    return false;
                }

                break;
            }
            case EntityBackpack backpack:
            {
                var tileEntityAccessAttemptEvent = new TileEntityAccessAttemptEvent(
                    __instance.Sender.entityId,
                    new Api.Contracts.TileEntity(backpack.entityId,
                        Api.Contracts.TileEntityType.Loot,
                        Vector3IntAdapter.FromGame(new Vector3i(backpack.position))
                    )
                );

                var result = ModContext.EventRunner.Publish(tileEntityAccessAttemptEvent, HookMode.Pre);

                if (result != HookResult.Continue)
                {
                    NetPackageLockResponseHelper.SetupAndSend(
                        __instance.Sender,
                        false,
                        "Rejected by PluginManager",
                        targets,
                        __instance.context,
                        __instance.channel
                    );
                    return false;
                }

                break;
            }
        }

        return true;
    }

    static class NetPackageLockResponseHelper
    {
        private static readonly MethodInfo SetupMethod;

        static NetPackageLockResponseHelper()
        {
            SetupMethod = typeof(NetPackageLockResponse).GetMethod("Setup",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new Type[]
                {
                    typeof(bool),
                    typeof(string),
                    typeof(ILockTarget[]),
                    typeof(ILockContext),
                    typeof(ushort)
                },
                null);

            if (SetupMethod == null)
            {
                Log.Error("Failed to find Setup method in NetPackageLockResponse");
            }
        }

        public static void SetupAndSend(
            object sender,
            bool success,
            string errorMsg,
            ILockTarget[] targets,
            ILockContext context,
            ushort channel = 0)
        {
            if (SetupMethod == null)
                return;

            var package = NetPackageManager.GetPackage<NetPackageLockResponse>();

            SetupMethod.Invoke(package, new object[]
            {
                success,
                errorMsg,
                targets,
                context,
                channel
            });

            sender.GetType().GetMethod("SendPackage")?.Invoke(sender, new object[] { package });
        }
    }
}