using AmbinityDB.Core.Models;

namespace AmbinityDB.Core.Interfaces;

public interface IAssetResolver
{
    Task<Stream> ResolveAsync(ManifestEntry entry);
}
