namespace AmbinityCore.Models.Profile;
public sealed class LightingProfileRuntimeContext
{
    public Guid ProfileId { get; }
    public string ProfileFolder { get; }
    public string AssetsFolder { get; }

    public IReadOnlyDictionary<string, string> RequiredFiles { get; }

    public LightingProfileRuntimeContext(
        Guid profileId,
        string profileFolder,
        string assetsFolder,
        IDictionary<string, string> requiredFiles)
    {
        ProfileId = profileId;
        ProfileFolder = profileFolder;
        AssetsFolder = assetsFolder;
        RequiredFiles = new Dictionary<string, string>(requiredFiles);
    }

    public string GetRequiredFile(string key)
    {
        if (!RequiredFiles.TryGetValue(key, out var path))
            throw new FileNotFoundException(
                $"Required asset '{key}' not found for profile {ProfileId}");

        return path;
    }
}
