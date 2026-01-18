
using System.IO.Compression;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;
using AmbinityCore.Repositories;
using AmbinityCore.Repositories.Services;
using Newtonsoft.Json;
using Serilog;

namespace AmbinityCore.Models.Profile;

public sealed class LightingProfileImportService : IAssetImportService
{
    public string AssetType => AssetTypes.Profile;

    private readonly LightingProfileRepository _repository;

    public LightingProfileImportService(LightingProfileRepository repository)
    {
        _repository = repository;
    }

    // =========================================================
    // PUBLIC ENTRY POINT
    // =========================================================
    public ImportResult Import(ImportRequest request)
    {
        try
        {
            var workingPath = request.IsZip
                ? ExtractToTemp(request.SourcePath)
                : request.SourcePath;

            var profile = LoadProfileFromFolder(workingPath);

            ResolveNameConflict(profile, request.OverwriteExisting);

            _repository.Add(profile); // repository updates index

            CleanupTempIfNeeded(request.IsZip);

            return new ImportResult
            {
                Success = true,
                ImportedAsset = _repository.CreateItemDescriptor(profile)
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Lighting profile import failed");

            CleanupTempIfNeeded(request.IsZip);

            return new ImportResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    // =========================================================
    // INTERNAL HELPERS
    // =========================================================
    private static LightingProfile LoadProfileFromFolder(string folderPath)
    {
        var configPath = Path.Combine(folderPath, "config.json");
        if (!File.Exists(configPath))
            configPath = Path.Combine(folderPath, "profile.json");

        if (!File.Exists(configPath))
            throw new FileNotFoundException("Profile config not found");

        var profile = JsonHelpers.DeserializeJson<LightingProfile>(configPath)
                      ?? throw new InvalidOperationException("Invalid profile config");

        profile.ID = Guid.NewGuid();
        profile.IsDefault = false;

        CopyAssets(folderPath, profile);

        return profile;
    }

    private static void CopyAssets(string sourceFolder, LightingProfile profile)
    {
        var iconPath = Path.Combine(sourceFolder, "icon.png");
        if (File.Exists(iconPath))
            File.Copy(iconPath, Path.Combine(profile.AssetPath, "icon.png"), true);

        var assetsPath = Path.Combine(sourceFolder, "assets");
        if (Directory.Exists(assetsPath))
            LocalFileHelpers.CopyDirectory(assetsPath, profile.AssetPath, true);
    }

    private void ResolveNameConflict(LightingProfile profile, bool overwrite)
    {
        if (overwrite)
            return;

        var existingNames = _repository
            .GetAllDescriptors()
            .Where(d => d.Name.StartsWith(profile.Name))
            .ToList();

        if (existingNames.Count == 0)
            return;

        profile.Name = $"{profile.Name} ({existingNames.Count})";
        Log.Information("Profile name conflict resolved: {Name}", profile.Name);
    }

    // =========================================================
    // TEMP / ZIP HANDLING
    // =========================================================
    private static string ExtractToTemp(string zipPath)
    {
        var tempPath = Path.Combine(
            Path.GetTempPath(),
            "AmbinityProfileImport");

        if (Directory.Exists(tempPath))
            Directory.Delete(tempPath, true);

        Directory.CreateDirectory(tempPath);
        ZipFile.ExtractToDirectory(zipPath, tempPath, true);

        return Directory.GetDirectories(tempPath).FirstOrDefault() ?? tempPath;
    }

    private static void CleanupTempIfNeeded(bool usedZip)
    {
        if (!usedZip)
            return;

        var tempPath = Path.Combine(Path.GetTempPath(), "AmbinityProfileImport");
        if (Directory.Exists(tempPath))
            Directory.Delete(tempPath, true);
    }
}
