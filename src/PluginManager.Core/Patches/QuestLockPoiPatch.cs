using System.Collections.Generic;
using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;
using PluginManager.Core.Adapters;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(QuestEventManager), nameof(QuestEventManager.QuestLockPOI))]
public static class QuestLockPoiPatch
{
    static void Postfix(int entityID, UnityEngine.Vector3 prefabPos, int[] sharedWithList)
    {
        var owners = new List<int> { entityID };
        if (sharedWithList != null) owners.AddRange(sharedWithList);

        var evt = new PoiQuestLockedEvent(
            Vector3IntAdapter.FromGame(new Vector3i(prefabPos)),
            owners.ToArray());

        var result = ModContext.EventRunner.Publish(evt, HookMode.Pre);

        if (result == HookResult.Continue)
            ModContext.EventRunner.Publish(evt, HookMode.Post);
    }
}
