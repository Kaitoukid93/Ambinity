namespace AmbinityCore.Repositories.Services;
public interface IAssetQueryService
{
    IEnumerable<AssetDescriptor> GetByType(string type);
}

public sealed class AssetQueryService : IAssetQueryService
{
    private readonly LocalAssetIndexService _local;

    public AssetQueryService(LocalAssetIndexService local)
    {
        _local = local;
    }

    public IEnumerable<AssetDescriptor> GetByType(string type)
        => _local.Items.Where(i => i.AssetType == type);
}
