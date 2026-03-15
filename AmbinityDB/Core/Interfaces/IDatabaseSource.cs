using AmbinityDB.Core.Models;

namespace AmbinityDB.Core.Interfaces;

public interface IDatabaseSource
{
    DatabaseSource Source { get; }

    Task<ManifestModel> LoadManifestAsync();
}
