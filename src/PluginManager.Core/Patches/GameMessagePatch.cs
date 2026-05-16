using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Contracts;
using PluginManager.Api.Hooks;
using PluginManager.Core.Mappers;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.DisplayGameMessage))]
public class GameMessagePatch
{
    static bool Prefix(
        GameManager __instance,
        ref string __result,
        EnumGameMessages _type,
        int _mainEntity,
        int _secondaryEntity,
        bool _log)
    {
        var gameMessageEvent = new GameMessageEvent(EnumMapper<EnumGameMessages, GameMessages>.Map(_type), _mainEntity,
            _secondaryEntity);
        var result = ModContext.EventRunner.Publish(gameMessageEvent, HookMode.Pre);

        switch (result)
        {
            case HookResult.Stop:
            {
                __result = null;
                return false;
            }

            case HookResult.Handled:
            {
                Log.Out(
                    $"Game message handled by plugin manager for ${gameMessageEvent.MainEntity}: {gameMessageEvent.Type}");
                __result = null;
                return false;
            }
        }

        var displayName1 = __instance.persistentPlayers.GetPlayerDataFromEntityID(_mainEntity)?.PlayerName?.DisplayName;
        var displayName2 = _secondaryEntity == -1
            ? null
            : __instance.persistentPlayers.GetPlayerDataFromEntityID(_secondaryEntity)?.PlayerName?.DisplayName;
        string txt;
        string message;
        switch (_type)
        {
            case EnumGameMessages.EntityWasKilled:
                if (!string.IsNullOrEmpty(displayName2))
                {
                    txt = $"GMSG: Player '{displayName1}' killed by '{displayName2}'";
                    message = string.Format(Localization.Get("killedGameMessage"), displayName2, displayName1);
                    break;
                }

                txt = $"GMSG: Player '{displayName1}' died";
                message = string.Format(Localization.Get("diedGameMessage"), displayName1);
                break;
            case EnumGameMessages.JoinedGame:
                txt = $"GMSG: Player '{displayName1}' joined the game";
                message = string.Format(Localization.Get("joinGameMessage"), displayName1);
                break;
            case EnumGameMessages.LeftGame:
                txt = $"GMSG: Player '{displayName1}' left the game";
                message = string.Format(Localization.Get("leaveGameMessage"), displayName1);
                break;
            case EnumGameMessages.BlockedPlayerAlert:
                txt = $"GMSG: Blocked player '{displayName1}' is present on this server!";
                message = string.Format("[FF0000A0]" + Localization.Get("blockedPlayerMessage"), displayName1);
                break;
            default:
                __result = null;
                return false;
        }

        if (_log)
            Log.Out(txt);

        if (!GameManager.IsDedicatedServer)
        {
            if (_type == EnumGameMessages.BlockedPlayerAlert)
            {
                XUiC_ChatOutput.AddMessage(__instance.myEntityPlayerLocal.PlayerUI.xui, _type, message);
            }
            else
            {
                foreach (var localPlayer in __instance.m_World.GetLocalPlayers())
                    XUiC_ChatOutput.AddMessage(LocalPlayerUI.GetUIForPlayer(localPlayer).xui, _type, message);
            }
        }

        ModContext.EventRunner.Publish(gameMessageEvent, HookMode.Post);

        __result = txt;
        return false;
    }
}