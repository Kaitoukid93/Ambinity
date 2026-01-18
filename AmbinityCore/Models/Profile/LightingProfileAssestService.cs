
using AmbinityCore.Helpers;
using Serilog;

namespace AmbinityCore.Models.Profile;
/// <summary>
/// Manages profile-specific asset folders (animations, etc).
/// Extracted from the old LightingProfile model.
/// </summary>
public sealed class LightingProfileAssetService
{
    private readonly string _profilesRoot;

    public LightingProfileAssetService(string libraryRoot)
    {
        _profilesRoot = Path.Combine(libraryRoot, "Profiles");
    }

    // =========================================================
    // PATHS
    // =========================================================
    public string GetProfileFolder(Guid profileId)
        => Path.Combine(_profilesRoot, profileId.ToString());

    public string GetAssetRoot(Guid profileId)
        => Path.Combine(GetProfileFolder(profileId), "assets");

    // =========================================================
    // ENSURE
    // =========================================================
    public void EnsureAssetFolder(Guid profileId)
    {
        var assetRoot = GetAssetRoot(profileId);
        Directory.CreateDirectory(assetRoot);
    }

    // =========================================================
    // LOAD ASSETS (old LoadAssets logic)
    // =========================================================
    public IReadOnlyList<EmbeddedAnimationRepository> LoadAssets(Guid profileId)
    {
        var assetRoot = GetAssetRoot(profileId);

        var result = new List<EmbeddedAnimationRepository>();
        if (!Directory.Exists(assetRoot))
        {
            Log.Information("Profile {ProfileId} has no assets folder", profileId);
            return result;
        }

        foreach (var dir in Directory.GetDirectories(assetRoot))
        {
            var name = Path.GetFileName(dir);

            switch (name)
            {
                case "animations":
                    var animationRepo = new EmbeddedAnimationRepository(dir);
                    animationRepo.LoadAll();
                    result.Add(animationRepo);
                    Log.Information("Loaded animation assets for profile {ProfileId}", profileId);
                    break;

                    // future:
                    // case "palettes":
                    // case "images":
            }
        }

        return result;
    }

    // =========================================================
    // IMPORT HELPERS
    // =========================================================
    public void CopyAssetsFromImport(
        Guid profileId,
        string importFolder)
    {
        var sourceAssets = Path.Combine(importFolder, "assets");
        if (!Directory.Exists(sourceAssets))
            return;

        var targetAssets = GetAssetRoot(profileId);
        Directory.CreateDirectory(targetAssets);

        LocalFileHelpers.CopyDirectory(
            sourceAssets,
            targetAssets, true);

        Log.Information("Copied assets for profile {ProfileId}", profileId);
    }

    public void CopyIconFromImport(
        Guid profileId,
        string importFolder)
    {
        var sourceIcon = Path.Combine(importFolder, "icon.png");
        if (!File.Exists(sourceIcon))
            return;

        var target = Path.Combine(GetProfileFolder(profileId), "icon.png");
        File.Copy(sourceIcon, target, overwrite: true);
    }
    public LightingProfileRuntimeContext CreateRuntimeContext(Guid profileId)
    {
        var profileFolder = GetProfileFolder(profileId);
        var assetsFolder = GetAssetRoot(profileId);

        if (!Directory.Exists(profileFolder))
            throw new DirectoryNotFoundException(profileFolder);

        var requiredFiles = new Dictionary<string, string>();

        // Example: animation scripts required by decoder
        var animationDir = Path.Combine(assetsFolder, "animations");
        if (Directory.Exists(animationDir))
        {
            foreach (var file in Directory.GetFiles(animationDir))
            {
                var key = Path.GetFileName(file);
                requiredFiles[key] = file;
            }
        }

        // You can add more required asset groups here:
        // - shaders
        // - binary LUTs
        // - mappings

        return new LightingProfileRuntimeContext(
            profileId,
            profileFolder,
            assetsFolder,
            requiredFiles);
    }

}
