using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Contracts;
using PluginManager.Api.Hooks;
using PluginManager.Core.Adapters;
using PluginManager.Core.Commands;
using PluginManager.Core.Mappers;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.ChatMessageServer))]
public static class ChatMessagePatch
{
    static bool Prefix(
        GameManager __instance,
        ClientInfo _cInfo,
        EChatType _chatType,
        int _senderEntityId,
        string _msg,
        List<int> _recipientEntityIds,
        EMessageSender _msgSender,
        GeneratedTextManager.BbCodeSupportMode _bbMode
    )
    {
        var messageTrimmed = _msg.Trim();
        if (messageTrimmed.StartsWith(ModContext.Config.ChatTrigger))
        {
            var parts = messageTrimmed.Substring(ModContext.Config.ChatTrigger.Length).Trim().Split(' ');
            if (parts.Length > 0)
            {
                var command = parts[0].ToLower();

                if (ModContext.CommandRegistry.TryGetCommand(command, out var commandDefinition))
                {
                    var args = parts.Skip(1).ToList();
                    var ctx = new CommandContext(args, ClientInfoAdapter.FromGame(_cInfo));
                    commandDefinition.Callback.Invoke(ctx);
                    return false;
                }
            }
        }

        string mainName = null;
        if (_senderEntityId != -1)
            mainName = Utils.EscapeBbCodes(__instance.persistentPlayers.GetPlayerDataFromEntityID(_senderEntityId)
                ?.PlayerName?.AuthoredName?.Text);

        var chatMessageEvent =
            new ChatMessageEvent(ClientInfoAdapter.FromGame(_cInfo), EnumMapper<EChatType, ChatType>.Map(_chatType),
                mainName, _msg,
                _recipientEntityIds,
                EnumMapper<GeneratedTextManager.BbCodeSupportMode, BbCodeSupportMode>.Map(_bbMode));
        var result = ModContext.EventRunner.Publish(chatMessageEvent, HookMode.Pre);

        if (result == HookResult.Continue) return true;

        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            var from = _cInfo?.PlatformId != null ? _cInfo.PlatformId.CombinedString : "-non-player-";
            var log =
                $"Chat (from '{from}', entity id '{_senderEntityId}', to '{_chatType.ToStringCached()}'): {(mainName != null ? $"'{mainName}': " : (object)"")}{_msg}";

            switch (result)
            {
                case HookResult.Stop: return false;

                case HookResult.Handled:
                {
                    Log.Out($"Chat handled by plugin manager: {log}");
                    return false;
                }
            }

            var data = new ModEvents.SChatMessageData(_cInfo, _chatType, _senderEntityId,
                _msg,
                mainName, _recipientEntityIds);
            var (emodEventResult, mod) = ModEvents.ChatMessage.Invoke(ref data);
            __instance.ChatMessageClient(_chatType, _senderEntityId, _msg, _recipientEntityIds, _msgSender,
                GeneratedTextManager.BbCodeSupportMode.Supported);

            if (emodEventResult == ModEvents.EModEventResult.StopHandlersAndVanilla)
            {
                Log.Out($"Chat handled by mod '{mod.Name}': {log}");
                return false;
            }

            Log.Out(log);

            var message = $"{chatMessageEvent.Name}[-]: {chatMessageEvent.Message}";

            if (_recipientEntityIds != null)
            {
                foreach (var recipientEntityId in chatMessageEvent.RecipientEntityIds)
                    SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(recipientEntityId)
                        ?.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(
                            EnumMapper<EChatType, ChatType>.MapBack(chatMessageEvent.ChatType),
                            -1, message, null, EMessageSender.None,
                            EnumMapper<GeneratedTextManager.BbCodeSupportMode, BbCodeSupportMode>.MapBack(
                                chatMessageEvent.BBMode)));
            }
            else
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                    NetPackageManager.GetPackage<NetPackageChat>().Setup(
                        EnumMapper<EChatType, ChatType>.MapBack(chatMessageEvent.ChatType), -1, message,
                        null, EMessageSender.None,
                        EnumMapper<GeneratedTextManager.BbCodeSupportMode, BbCodeSupportMode>.MapBack(chatMessageEvent
                            .BBMode)), true);
        }
        else
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager
                .GetPackage<NetPackageChat>()
                .Setup(_chatType, _senderEntityId, _msg, _recipientEntityIds, _msgSender, _bbMode));

        ModContext.EventRunner.Publish(chatMessageEvent, HookMode.Post);

        return false;
    }
}