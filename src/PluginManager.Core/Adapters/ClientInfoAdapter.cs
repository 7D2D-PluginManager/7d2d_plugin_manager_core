namespace PluginManager.Core.Adapters;

public static class ClientInfoAdapter
{
    private const string Unknown = "<unknown>";

    public static Api.Contracts.ClientInfo FromGame(ClientInfo clientInfo)
    {
        if (clientInfo == null || clientInfo.entityId == -1)
        {
            return new Api.Contracts.ClientInfo(-1, Unknown, Unknown, Unknown, Unknown);
        }

        var entityId = clientInfo.entityId;
        var platformId = SafeString(clientInfo.PlatformId?.CombinedString);
        var crossId = SafeString(clientInfo.CrossplatformId?.CombinedString);
        var name = SafeString(clientInfo.playerName);
        var ip = SafeString(clientInfo.ip);

        return new Api.Contracts.ClientInfo(entityId, platformId, crossId, name, ip);
    }

    private static string SafeString(string value) =>
        string.IsNullOrWhiteSpace(value) ? Unknown : value;
}