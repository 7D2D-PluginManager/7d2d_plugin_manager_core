using System.Collections.Generic;
using System.Linq;
using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Contracts;
using PluginManager.Api.Proxy;
using PluginManager.Core.Adapters;
using PluginManager.Core.Mappers;
using UnityEngine;
using Vector3 = PluginManager.Api.Contracts.Vector3;
using Vector3Int = PluginManager.Api.Contracts.Vector3Int;

namespace PluginManager.Core.Capabilities.Utils;

public class PlayerUtil : ProxyObject, IPlayerUtil
{
    public string Name => nameof(PlayerUtil);

    public void Kick(int entityId, string reason = "")
    {
        if (!TryClientInfo(entityId, out var clientInfo)) return;

        GameUtils.KickPlayerForClientInfo(clientInfo,
            new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, _customReason: reason));
    }

    public void Teleport(int entityId, Vector3 position)
    {
        if (!TryClientInfo(entityId, out var clientInfo)) return;

        clientInfo.SendPackage(
            NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(
                Vector3Adapter.ToGame(position)
            )
        );
    }

    public void PlaySound(int entityId, string soundName, int distance)
    {
        if (!TryClientInfo(entityId, out var clientInfo)) return;
        if (!TryGetEntityPlayer(entityId, out var entityPlayer)) return;

        clientInfo.SendPackage(
            NetPackageManager.GetPackage<NetPackageSoundAtPosition>()
                .Setup(entityPlayer.position, soundName, AudioRolloffMode.Linear, distance, entityId, 1.0f)
        );
    }

    public void PrintToChat(int entityId, string message)
    {
        if (!TryClientInfo(entityId, out var clientInfo)) return;

        clientInfo.SendPackage(
            NetPackageManager.GetPackage<NetPackageChat>()
                .Setup(EChatType.Global, -1, message, null, EMessageSender.None,
                    GeneratedTextManager.BbCodeSupportMode.Supported)
        );
    }

    public bool IsPlayer(int entityId)
    {
        return TryGetEntityPlayer(entityId, out _);
    }

    public bool IsPlayerInVehicle(int entityId)
    {
        return TryGetEntityPlayer(entityId, out var entityPlayer) &&
               entityPlayer.AttachedToEntity is EntityVehicle;
    }

    public Vector3 GetPlayerPosition(int entityId)
    {
        return TryGetEntityPlayer(entityId, out var entityPlayer)
            ? Vector3Adapter.FromGame(entityPlayer.position)
            : null;
    }

    public LandClaimOwner GetClaimOwner(int entityId, Vector3Int position)
    {
        var gm = GameManager.Instance;
        var players = gm.persistentPlayers;

        var playerData = players?.GetPlayerDataFromEntityID(entityId);
        if (playerData == null) return LandClaimOwner.None;

        var worldPosition = Vector3i.FromVector3Rounded(Vector3IntAdapter.ToGame(position));

        return EnumMapper<EnumLandClaimOwner, LandClaimOwner>.Map(
            gm.World.GetLandClaimOwner(worldPosition, playerData));
    }

    public Api.Contracts.ClientInfo GetClientInfoByEntityId(int entityId)
    {
        return TryClientInfo(entityId, out var clientInfo) ? ClientInfoAdapter.FromGame(clientInfo) : null;
    }

    public IEnumerable<Api.Contracts.ClientInfo> GetClientInfoList()
    {
        return ConnectionManager.Instance.Clients.list.Select(ClientInfoAdapter.FromGame).ToList();
    }

    public int GetPermissionLevelByEntityId(int entityId)
    {
        return TryClientInfo(entityId, out var clientInfo)
            ? GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo)
            : 1000;
    }

    private static bool TryGetEntityPlayer(int entityId, out EntityPlayer entityPlayer)
    {
        return GameManager.Instance.World.Players.dict.TryGetValue(entityId, out entityPlayer);
    }

    private static bool TryClientInfo(int entityId, out ClientInfo clientInfo)
    {
        clientInfo = ConnectionManager.Instance.Clients.ForEntityId(entityId);
        return clientInfo is { loginDone: true };
    }
}