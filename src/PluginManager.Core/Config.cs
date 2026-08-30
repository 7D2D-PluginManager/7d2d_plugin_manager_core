namespace PluginManager.Core;

public class Config
{
    public string ChatTrigger { get; set; } = "/";

    public bool PublishGameUpdateEvent { get; set; } = false;
}
