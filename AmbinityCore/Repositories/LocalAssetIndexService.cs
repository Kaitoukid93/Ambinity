namespace AmbinityCore.Repositories;
using Newtonsoft.Json;

public sealed class LocalAssetIndexService
{
    private readonly string _indexPath;
    private readonly Dictionary<string, AssetDescriptor> _items = new();

    public IReadOnlyCollection<AssetDescriptor> Items => _items.Values;

    public LocalAssetIndexService(string indexPath)
    {
        _indexPath = indexPath;
        Load();
    }

    private void Load()
    {
        if (!File.Exists(_indexPath))
            return;

        var json = File.ReadAllText(_indexPath);
        var list = JsonConvert.DeserializeObject<List<AssetDescriptor>>(json) ?? [];

        _items.Clear();
        foreach (var item in list)
            _items[item.Id] = item;
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_indexPath)!);
        File.WriteAllText(
            _indexPath,
            JsonConvert.SerializeObject(_items.Values, Formatting.Indented));
    }

    public void AddOrUpdate(AssetDescriptor descriptor)
    {
        _items[descriptor.Id] = descriptor;
        Save();
    }

    public void Remove(string id)
    {
        if (_items.Remove(id))
            Save();
    }
}
