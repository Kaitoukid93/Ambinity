namespace AmbinityDB.Core.Interfaces;

public interface IAssetCache
{
    bool TryGet(string key, out object? value);

    void Store(string key, object value);

    void Remove(string key);
}
