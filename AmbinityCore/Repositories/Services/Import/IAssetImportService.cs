namespace AmbinityCore.Repositories.Services;
public interface IAssetImportService
{
    string AssetType { get; }
    Task<ImportResult> ImportAsync(ImportRequest request);
}
