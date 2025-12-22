namespace PluginManager.Core.Adapters;

public static class Vector3IntAdapter
{
    public static Api.Contracts.Vector3Int FromGame(Vector3i vector)
    {
        return new Api.Contracts.Vector3Int(vector.x, vector.y, vector.z);
    }

    public static Vector3i ToGame(Api.Contracts.Vector3Int vector)
    {
        return new Vector3i(vector.X, vector.Y, vector.Z);
    }
}