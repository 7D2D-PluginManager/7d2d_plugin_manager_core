using System.Collections.Concurrent;
using PluginManager.Api.Capabilities.Implementations.GeoIp;
using PluginManager.Api.Contracts;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Capabilities.GeoIp;

public class GeoIpDataStorage: ProxyObject, IGeoIpDataStorage
{
    public string Name => nameof(GeoIpDataStorage);
    
    private readonly ConcurrentDictionary<string, GeoIpData> _playerGeoIpData = new();
    
    public void SetGetGeoIpData(string platformId, GeoIpData cultureInfo)
    {
        _playerGeoIpData[platformId] = cultureInfo;
    }

    public GeoIpData GetGeoIpData(string platformId)
    {
        return _playerGeoIpData.TryGetValue(platformId, out var geoIpData) ? geoIpData : null;
    }
}