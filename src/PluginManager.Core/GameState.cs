namespace PluginManager.Core;

public static class GameState
{
    private static volatile bool _started;
    private static volatile bool _stopping;

    public static bool Started => _started;

    public static bool Stopping => _stopping;

    public static void MarkStarted()
    {
        _stopping = false;
        _started = true;
    }

    public static void MarkStopping()
    {
        _stopping = true;
        _started = false;
    }

    public static bool WorldReady
    {
        get
        {
            if (!_started || _stopping) return false;

            var gameManager = GameManager.Instance;
            return gameManager != null && gameManager.World != null;
        }
    }
}
