
using AmbinityDB.Core.Models;

namespace AmbinityCore.Repositories.Loader;

public interface IAssetLoader
{
    Task<T> LoadAsync<T>(ManifestEntry entry);
}
