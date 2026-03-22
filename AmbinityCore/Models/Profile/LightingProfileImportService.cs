using System.IO.Compression;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using AmbinityCore.Repositories.Services;
using AmbinityDB.Core.Models;
using AmbinityDB.Core.Services;
using Newtonsoft.Json;
using Serilog;

namespace AmbinityCore.Models.Profile;

public sealed class LightingProfileImportService : IAssetImportService
{
    public string AssetType => AssetTypes.Profile;

    private readonly DatabaseManager _db;
    private readonly string _assetRoot;

    public LightingProfileImportService(DatabaseManager db, string assetRoot)
    {
        _db = db;
        _assetRoot = assetRoot;
    }

    // =========================================================
    // PUBLIC ENTRY POINT
    // =========================================================
    public async Task<ImportResult> ImportAsync(ImportRequest request)
    {
        string? workingPath = null;

        try
        {
            workingPath = request.IsZip
                ? ExtractToTemp(request.SourcePath)
                : request.SourcePath;

            var configPath = ResolveConfigPath(workingPath);

            // 🔥 Read metadata only (lightweight)
            var json = await File.ReadAllTextAsync(configPath);
            var data = JsonConvert.DeserializeObject<LightingProfile>(json);

            var id = Guid.NewGuid();

            // 🔥 Copy into managed storage
            var targetDir = Path.Combine(_assetRoot, "profiles", id.ToString());
            Directory.CreateDirectory(targetDir);

            var targetConfig = Path.Combine(targetDir, "config.json");
            File.Copy(configPath, targetConfig, true);

            CopyAssets(workingPath, targetDir);

            // 🔥 Create ManifestEntry
            var entry = new ManifestEntry
            {
                Id = id.ToString(),
                Name = data?.Name ?? Path.GetFileNameWithoutExtension(request.SourcePath),
                Path = $"profiles/{id}/config.json",
                Source = "local",
                Type = AssetTypes.Profile
            };

            // 🔥 Register to DB (this triggers repository update)
            await _db.AddAsync(entry);

            return new ImportResult
            {
                Success = true,
                Entry = entry
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Lighting profile import failed");

            return new ImportResult
            {
                Success = false,
                Error = ex.Message
            };
        }
        finally
        {
            CleanupTempIfNeeded(request.IsZip);
        }
    }

    // =========================================================
    // INTERNAL HELPERS
    // =========================================================
    private static string ResolveConfigPath(string folderPath)
    {
        var config = Path.Combine(folderPath, "config.json");
        if (File.Exists(config)) return config;

        config = Path.Combine(folderPath, "profile.json");
        if (File.Exists(config)) return config;

        throw new FileNotFoundException("Profile config not found");
    }

    private static void CopyAssets(string sourceFolder, string targetDir)
    {
        var iconPath = Path.Combine(sourceFolder, "icon.png");
        if (File.Exists(iconPath))
            File.Copy(iconPath, Path.Combine(targetDir, "icon.png"), true);

        var assetsPath = Path.Combine(sourceFolder, "assets");
        if (Directory.Exists(assetsPath))
            LocalFileHelpers.CopyDirectory(
                assetsPath,
                Path.Combine(targetDir, "assets"),
                true);
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
