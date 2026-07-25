using PluginManager.Api.Capabilities.Implementations.Utils;
using PluginManager.Api.Proxy;

namespace PluginManager.Core.Capabilities.Utils;

public class BlockUtil : ProxyObject, IBlockUtil
{
    public string Name => nameof(BlockUtil);

    public bool IsLandClaim(string blockName)
    {
        return blockName == "keystoneBlock";
    }

    public bool IsBedroll(string blockName)
    {
        return Block.GetBlockByName(blockName) is BlockSleepingBag;
    }
}
