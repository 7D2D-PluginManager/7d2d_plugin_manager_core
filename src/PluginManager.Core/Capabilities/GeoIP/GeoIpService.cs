using System;
using MaxMind.GeoIP2;
using PluginManager.Api.Contracts;

namespace PluginManager.Core.Capabilities.GeoIp;

public class GeoIpService(string dbPath) : IGeoIpService, IDisposable
{
    private readonly DatabaseReader _reader = new(dbPath);

    public GeoIpData GetGeoIpData(string ipAddress)
    {
        return _reader.TryCity(ipAddress, out var response)
            ? new GeoIpData(response.City.Name, response.Country.Name, response.Country.IsoCode)
            : null;
    }

    public bool TryGeoIpData(string ipAddress, out GeoIpData geoIpData)
    {
        geoIpData = null;
        
        if (_reader.TryCity(ipAddress, out var response))
        {
            geoIpData = new GeoIpData(
                response.City.Name, 
                response.Country.Name, 
                response.Country.IsoCode
            );
            return true;
        }
        
        return false;
    }
    
    public void Dispose()
    {
        _reader?.Dispose();
    }
}