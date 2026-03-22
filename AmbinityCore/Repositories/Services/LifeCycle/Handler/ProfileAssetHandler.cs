
namespace AmbinityCore.Repositories;
public class ProfileAssetHandler
    : IAssetHandler<UpdateProfilePayload>
{
    public string AssetType => AssetTypes.Profile;

    private readonly LightingProfileImportService _import;

    public ProfileAssetHandler(LightingProfileImportService import)
    {
        _import = import;
    }

    public Task<ManifestEntry> ImportAsync(ImportRequest request)
    {
        return _import.ImportAsync(request)
                      .ContinueWith(t => t.Result.Entry!);
    }

    public Task UpdateAsync(ManifestEntry entry, UpdateProfilePayload payload)
    {
        //update this entry
        //copy files ( thumb..)
        if (payload.Name != null)
            entry.Name = payload.Name;

        if (payload.IconPath != null)
        {
            CopyIcon(entry, payload.IconPath);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ManifestEntry entry)
    {
        // optional type-specific cleanup
        return Task.CompletedTask;
    }

    private void CopyIcon(ManifestEntry entry, string sourcePath)
    {
        var targetDir = Path.Combine(_assetRoot, "profiles", entry.Id);

        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        var targetPath = Path.Combine(targetDir, "icon.png");

        File.Copy(sourcePath, targetPath, true);

        // update manifest path
        entry.Icon = $"profiles/{entry.Id}/icon.png";
    }
}
