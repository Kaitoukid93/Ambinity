using AmbinityDB.Core.Models;

namespace AmbinityDB.Core.Interfaces;

public interface IManifestProvider
{
    Task<ManifestModel> LoadManifestAsync(DatabaseSource source);
}
