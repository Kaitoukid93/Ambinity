namespace AmbinityCore.Repositories;

public interface IAssetHandler<TPayload>
{
    string AssetType { get; }

    Task<ManifestEntry> ImportAsync(ImportRequest request);

    Task UpdateAsync(ManifestEntry entry, object payload);

    Task DeleteAsync(ManifestEntry entry);
}
