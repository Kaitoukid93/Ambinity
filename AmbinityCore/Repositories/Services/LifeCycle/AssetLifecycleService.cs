using AmbinityCore.Models.Device;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;
namespace AmbinityCore.Repositories.Services;


public sealed class AssetLifecycleService
{
    private readonly LightingProfileRepository _profileRepo;
    private readonly AmbinityDeviceLayoutRepository _layoutRepo;
    private readonly ColorPaletteRepository _paletteRepo;

    private readonly LightingProfileImportService _profileImport;
    private readonly DownloadService _downloadService;

    private readonly LocalAssetIndexService _localIndex;

    public AssetLifecycleService(
        LightingProfileRepository profileRepo,
        AmbinityDeviceLayoutRepository layoutRepo,
        ColorPaletteRepository paletteRepo,
        LightingProfileImportService profileImport,
        DownloadService downloadService,
        LocalAssetIndexService localIndex)
    {
        _profileRepo = profileRepo;
        _layoutRepo = layoutRepo;
        _paletteRepo = paletteRepo;
        _profileImport = profileImport;
        _downloadService = downloadService;
        _localIndex = localIndex;
    }

    // =========================================================
    // OPEN (local only)
    // =========================================================
    // public object Open(AssetDescriptor asset)
    // {
    //     EnsureLocal(asset);

    //     return asset.AssetType switch
    //     {
    //         AssetTypes.Profile => _profileRepo.Load(asset.Id),
    //         _ => throw new NotSupportedException(asset.AssetType)
    //     };
    // }

    // =========================================================
    // SAVE (local only)
    // =========================================================
    public void Save(object domainModel)
    {
        switch (domainModel)
        {
            case LightingProfile profile:
                //_profileRepo.Add(profile); // Add = upsert
                break;
            default:
                throw new NotSupportedException(
                    $"Unsupported domain model: {domainModel.GetType().Name}");
        }
    }

    // =========================================================
    // DELETE
    // =========================================================
    public void Delete(AssetDescriptor asset)
    {
        switch (asset.AssetType)
        {
            case AssetTypes.Profile:
               // _profileRepo.Delete(asset.Id);
                break;
            default:
                throw new NotSupportedException(asset.AssetType);
        }
    }

    // =========================================================
    // DOWNLOAD (online → local)
    // =========================================================
    public async Task DownloadAsync(AssetDescriptor asset)
    {
        if (asset.Source != AssetSource.Online)
            return;

        var downloadedPath = string.Empty;
        var request = new ImportRequest
        {
            SourcePath = downloadedPath,
            IsZip = false,
            OverwriteExisting = true
        };

        switch (asset.AssetType)
        {
            case AssetTypes.Profile:
                _profileImport.Import(request);
                break;
            default:
                throw new NotSupportedException(asset.AssetType);
        }
    }

    // =========================================================
    // IMPORT (ZIP or external folder)
    // =========================================================
    public void Import(string filePath, string assetType)
    {
        var request = new ImportRequest
        {
            SourcePath = filePath,
            IsZip = Path.GetExtension(filePath).Equals(".zip", StringComparison.OrdinalIgnoreCase),
            OverwriteExisting = false
        };
        switch (assetType)
        {
            case AssetTypes.Profile:
                _profileImport.Import(request);
                break;
            default:
                throw new NotSupportedException(assetType);
        }
    }

    // =========================================================
    // UTIL
    // =========================================================
    private static void EnsureLocal(AssetDescriptor asset)
    {
        if (asset.Source != AssetSource.Local)
            throw new InvalidOperationException(
                $"Asset '{asset.Name}' is not local.");
    }
}
