namespace PluginManager.Core.Adapters;

public static class Vector3Adapter
{
    public static Api.Contracts.Vector3 FromGame(UnityEngine.Vector3 vector)
    {
        return new Api.Contracts.Vector3(vector.x, vector.y, vector.z);
    }

    public static UnityEngine.Vector3 ToGame(Api.Contracts.Vector3 vector)
    {
        return new UnityEngine.Vector3(vector.X, vector.Y, vector.Z);
    }
}