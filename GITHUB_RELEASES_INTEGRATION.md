# GitHub Releases Integration Guide

## Overview
The Ambinity installer has been updated to download releases from GitHub instead of SFTP. The solution provides **dual support** - it can fall back to SFTP if GitHub is unavailable.

## Usage

### Option 1: Configure GitHub in App.axaml.cs (Recommended)

Update the `ConfigureIoc()` method in [Ambinity.Installer/App.axaml.cs](Ambinity.Installer/App.axaml.cs) to initialize AmbinityClient with GitHub credentials:

```csharp
private static void ConfigureIoc()
{
    var postInstallationSettings = new PostInstallationSettings();
    Ioc.Default.ConfigureServices(
        new ServiceCollection()
            // Initialize with GitHub releases support
            .AddSingleton(new AmbinityClient(
                gitHubOwner: "your-github-username",  // e.g., "Ambino"
                gitHubRepo: "your-repo-name",          // e.g., "Ambinity"
                assetName: "Ambinity.zip"              // Asset name to download
            ))
            .AddSingleton<RootViewModel>()
            // ... rest of configuration
            .AddSingleton<InstallationService>()
            // ... more registrations
```

### Option 2: Environment Variables

You can also set environment variables before running the installer:
```batch
set GITHUB_OWNER=your-github-username
set GITHUB_REPO=your-repo-name
set GITHUB_ASSET=Ambinity.zip
```

Then modify the initialization to read from environment:
```csharp
var owner = Environment.GetEnvironmentVariable("GITHUB_OWNER") ?? "";
var repo = Environment.GetEnvironmentVariable("GITHUB_REPO") ?? "";
var asset = Environment.GetEnvironmentVariable("GITHUB_ASSET") ?? "Ambinity.zip";

.AddSingleton(new AmbinityClient(owner, repo, asset))
```

## How It Works

### Installation Flow

1. **GetAvailableRelease()** - Fetches releases:
   - First attempts to get releases from GitHub API
   - Falls back to SFTP if GitHub is unavailable or not configured
   - Returns a list of `AppReleaseInformation` objects with download URLs

2. **DownloadRelease()** - Downloads the asset:
   - Uses the GitHub download URL (browser CDN)
   - Reports progress during download
   - Validates the downloaded file exists

3. **InstallRelease()** - Installs (unchanged):
   - Extracts the Ambinity.zip file
   - Creates shortcuts and registry entries

### GitHub API Details

- **API Endpoint**: `https://api.github.com/repos/{owner}/{repo}/releases`
- **Asset Download**: Uses `browser_download_url` for direct downloads
- **Rate Limiting**: 60 requests/hour (unauthenticated), 5000/hour (authenticated)
- **No Authentication Required**: Public repositories don't need GitHub tokens

## Implementation Details

### New Class: GitHubReleaseClient

Located in `AmbinityServer/AppRelease/GitHubReleaseClient.cs`

**Public Methods:**
- `GetAvailableReleases()` - Fetches all releases from GitHub
- `DownloadAsset(downloadUrl, outputPath, progress)` - Downloads a specific asset with progress reporting

**Features:**
- Automatic asset filtering by name (`Ambinity.zip`)
- Version information from tag names
- Changelog from release descriptions
- Progress tracking during downloads
- Comprehensive error logging

### Modified Classes

#### AmbinityClient
- Added `GitHubClient` property
- Updated constructor to accept GitHub parameters (optional)
- Maintains backward compatibility with SFTP

#### InstallationService
- `GetAvailableRelease()` now tries GitHub first, falls back to SFTP
- `DownloadRelease()` uses GitHub client when available
- SFTP path still available as fallback

## Configuration Examples

### Example 1: GitHub Only
```csharp
.AddSingleton(new AmbinityClient("Ambino", "Ambinity", "Ambinity.zip"))
```

### Example 2: Fallback to SFTP
```csharp
// In constants file
const string GITHUB_OWNER = ""; // Empty = use SFTP
const string GITHUB_REPO = "";

.AddSingleton(new AmbinityClient(GITHUB_OWNER, GITHUB_REPO))
```

### Example 3: Dynamic Configuration
```csharp
var config = GetConfigurationFromFile(); // Your config loading
.AddSingleton(new AmbinityClient(
    config.GitHubOwner,
    config.GitHubRepo,
    config.AssetName
))
```

## Requirements

### NuGet Dependencies
- **Newtonsoft.Json** (already included) - for JSON parsing
- **System.Net.Http** (built-in) - for HTTP requests

### Repository Requirements
- Public GitHub repository
- Releases with attached `Ambinity.zip` asset
- Tag name or release name (will be displayed as version)
- (Optional) Release description for changelog

## Testing

### Test Release Creation
Create a test release on GitHub:
```bash
# Create a tag
git tag v1.0.0

# Push tag
git push origin v1.0.0

# Upload Ambinity.zip as asset in GitHub UI or via API
```

### Local Testing
```csharp
var client = new GitHubReleaseClient("your-username", "test-repo");
var releases = await client.GetAvailableReleases();
await client.DownloadAsset(releases[0].Path, "test.zip", null);
```

## Troubleshooting

### No Releases Found
- Verify repository is public
- Check that releases have `Ambinity.zip` asset
- Ensure asset name matches exactly (case-sensitive)
- Check network connectivity to GitHub

### Download Fails
- Verify GitHub rate limit hasn't been exceeded
-  Check file system write permissions
- Ensure sufficient disk space
- Check network connectivity

### SFTP Fallback Active
- Check logs for "Failed to fetch from GitHub"
- Verify GitHub owner/repo are correctly configured
- Confirm `GitHubClient` is not null

## Logging

The implementation logs all operations to Serilog:
- GitHub connection attempts
- Release fetches and counts
- Download progress
- Errors and fallbacks
- SFTP usage (when GitHub fails)

Example log output:
```
[INF] Fetching releases from GitHub
[INF] Found 5 releases on GitHub
[INF] Successfully downloaded asset to C:\Cache\Ambinity.zip
```

## Migration Checklist

- [ ] Update AmbinityClient initialization in App.axaml.cs
- [ ] Set GitHub owner and repository name
- [ ] Test release listing
- [ ] Test download functionality
- [ ] Verify fallback to SFTP works (optional)
- [ ] Update any documentation/user guides
- [ ] Deploy updated installer

## Notes

- All network requests use `User-Agent: Ambinity-Installer`
- Downloads are async and report progress via `IProgress<int>`
- SFTP fallback requires existing SFTP credentials (unchanged)
- Assets with different names can be downloaded by configuring `assetName`
