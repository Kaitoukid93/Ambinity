namespace AmbinityCore.Repositories.Services;
public interface IAssetImportService
{
    string AssetType { get; }
    ImportResult Import(ImportRequest request);
}
