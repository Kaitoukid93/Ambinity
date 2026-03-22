using AmbinityCore.Repositories.Services;
using AmbinityDB.Core.Models;
using AmbinityDB.Core.Services;

public class AssetLifecycleService
{
    private readonly Dictionary<string, IAssetHandler> _handlers;
    private readonly DatabaseManager _db;
    private readonly string _assetRoot;

    public AssetLifecycleService(
        IEnumerable<IAssetHandler> handlers,
        DatabaseManager db,
        string assetRoot)
    {
        _handlers = handlers.ToDictionary(h => h.AssetType);
        _db = db;
        _assetRoot = assetRoot;
    }

    public async Task ImportAsync(string assetType, ImportRequest request)
    {
        var handler = GetHandler(assetType);

        var entry = await handler.ImportAsync(request);

        await _db.AddAsync(entry);
    }

    public async Task DeleteAsync(string id)
    {
        var entry = _db.GetEntry(id);
        if (entry == null)
            return;

        var handler = GetHandler(entry.Type);

        // delete files
        DeleteFiles(entry);

        await handler.DeleteAsync(entry);

        await _db.RemoveAsync(id);
    }

    public async Task UpdateAsync<TPayload>(string id, TPayload payload)
    {
        var entry = _db.GetEntry(id);
        if (entry == null) return;

        var handler = (IAssetHandler<TPayload>)GetHandler(entry.Type);

        await handler.UpdateAsync(entry, payload);

        await _db.UpdateAsync(entry);
    }

    private IAssetHandler GetHandler(string type)
    {
        if (!_handlers.TryGetValue(type, out var handler))
            throw new NotSupportedException(type);

        return handler;
    }

    private void DeleteFiles(ManifestEntry entry)
    {
        var fullPath = Path.Combine(_assetRoot, entry.Path);
        var dir = Path.GetDirectoryName(fullPath);

        if (Directory.Exists(dir))
            Directory.Delete(dir, true);
    }
}
