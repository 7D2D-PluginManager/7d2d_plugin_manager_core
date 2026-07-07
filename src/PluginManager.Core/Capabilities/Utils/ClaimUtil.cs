using System;
using System.Collections.Generic;
using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Contracts;
using PluginManager.Api.Proxy;
using PluginManager.Core.Adapters;

namespace PluginManager.Core.Capabilities.Utils;

public class ClaimUtil : ProxyObject, IClaimUtil
{
    public string Name => nameof(ClaimUtil);

    public int GetClaimSize()
    {
        return GameStats.GetInt(EnumGameStats.LandClaimSize);
    }

    public LandClaim[] GetLandClaims()
    {
        var players = GameManager.Instance.persistentPlayers;
        if (players == null) return Array.Empty<LandClaim>();

        var result = new List<LandClaim>();
        foreach (var pair in players.m_lpBlockMap)
        {
            var ownerId = pair.Value?.PrimaryId?.CombinedString ?? string.Empty;
            result.Add(new LandClaim(Vector3IntAdapter.FromGame(pair.Key), ownerId));
        }

        return result.ToArray();
    }
}
