using AmbinityDB.Core.Interfaces;
using AmbinityDB.Core.Services;

public interface IWritableDatabaseSource : IDatabaseSource
{
    Task SaveManifestAsync(ManifestIndex index);
}
