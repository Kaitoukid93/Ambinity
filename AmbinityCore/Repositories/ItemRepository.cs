namespace AmbinityCore.Repositories;

public abstract class ItemRepository<TItem>
{
    protected readonly string BasePath;
    protected readonly LocalAssetIndexService Index;
    /// <summary>
    /// The asset type this repository manages (Profile, Device, etc.)
    /// </summary>
    protected abstract string AssetType { get; }

    protected ItemRepository(
        string basePath,
        LocalAssetIndexService index)
    {
        BasePath = basePath;
        Index = index;
        Directory.CreateDirectory(BasePath);
    }

    public void Add(TItem item)
    {
        SaveItem(item);
        Index.AddOrUpdate(CreateDescriptor(item));
    }

    public void Delete(string id)
    {
        DeleteItem(id);
        Index.Remove(id);
    }
    public AssetDescriptor CreateItemDescriptor(TItem item) => CreateDescriptor(item);

    public TItem Load(string id) => LoadItem(id);

    protected abstract void SaveItem(TItem item);
    protected abstract TItem LoadItem(string id);
    protected abstract void DeleteItem(string id);
    protected abstract AssetDescriptor CreateDescriptor(TItem item);
    public IReadOnlyList<AssetDescriptor> GetAllDescriptors()
    {
        return Index.Items
            .Where(d => d.AssetType == AssetType)
            .ToList();
    }
}
