using System.Collections.Generic;
using System.Linq;
using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Contracts;
using PluginManager.Api.Proxy;
using PluginManager.Core.Adapters;
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

        GameUtils.KickPlayerForClientInfo(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, _customReason: reason));
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
                .Setup(entityPlayer.position, soundName, AudioRolloffMode.Linear, distance, entityId)
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
        var playerData = GameManager.Instance.persistentPlayers?.GetPlayerDataFromEntityID(entityId);
        if (playerData == null) return LandClaimOwner.None;

        var checkPos = Vector3i.FromVector3Rounded(Vector3IntAdapter.ToGame(position));
        var claimSize = GameStats.GetInt(EnumGameStats.LandClaimSize);
        var halfSize = (claimSize - 1) / 2;

        var minX = checkPos.x - halfSize;
        var maxX = checkPos.x + halfSize;
        var minZ = checkPos.z - halfSize;
        var maxZ = checkPos.z + halfSize;

        var chunkRadiusX = claimSize / 16 + 1;
        var chunkRadiusZ = claimSize / 16 + 1;

        for (var i = -chunkRadiusX; i <= chunkRadiusX; i++)
        {
            var x = minX + i * 16;
            for (var j = -chunkRadiusZ; j <= chunkRadiusZ; j++)
            {
                var z = minZ + j * 16;
                var chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(new Vector3i(x, checkPos.y, z));
                if (!chunk.IndexedBlocks.TryGetValue("lpblock", out var lpBlocks)) continue;

                var worldPos = chunk.GetWorldPos();
                foreach (var localPos in lpBlocks)
                {
                    var blockPos = localPos + worldPos;

                    if (blockPos.x < minX || blockPos.x > maxX || blockPos.z < minZ || blockPos.z > maxZ) continue;

                    if (!BlockLandClaim.IsPrimary(chunk.GetBlock(localPos))) continue;

                    var owner = GameManager.Instance.persistentPlayers.GetLandProtectionBlockOwner(blockPos);
                    if (owner == null || !GameManager.Instance.World.IsLandProtectionValidForPlayer(owner)) continue;

                    if (playerData == owner)
                        return LandClaimOwner.Self;

                    return owner.ACL?.Contains(playerData.PrimaryId) == true
                        ? LandClaimOwner.Ally
                        : LandClaimOwner.Other;
                }
            }
        }

        return LandClaimOwner.None;
    }

    public Api.Contracts.ClientInfo GetClientInfoByEntityId(int entityId)
    {
        return TryClientInfo(entityId, out var clientInfo) ? ClientInfoAdapter.FromGame(clientInfo) : null;
    }

    public IEnumerable<Api.Contracts.ClientInfo> GetClientInfoList()
    {
        return ConnectionManager.Instance.Clients.list.Select(ClientInfoAdapter.FromGame).ToList();
    }

    private bool TryGetEntityPlayer(int entityId, out EntityPlayer entityPlayer)
    {
        return GameManager.Instance.World.Players.dict.TryGetValue(entityId, out entityPlayer);
    }

    private bool TryClientInfo(int entityId, out ClientInfo clientInfo)
    {
        clientInfo = ConnectionManager.Instance.Clients.ForEntityId(entityId);
        return clientInfo is { loginDone: true };
    }
}