using PluginManager.Api.Contracts;

namespace PluginManager.Core.Capabilities.GeoIp;

public interface IGeoIpService
{
    public GeoIpData GetGeoIpData(string ipAddress);
    public bool TryGeoIpData(string ipAddress, out GeoIpData geoIpData);
}