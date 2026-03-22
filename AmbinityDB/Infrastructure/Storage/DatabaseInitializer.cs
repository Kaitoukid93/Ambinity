namespace AmbinityDB.Storage.Infrastructure;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AmbinityDB.Infrastructure.Resources;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(CancellationToken ct = default)
    {
        EnsureStorage();

        var resourceInitializer = CreateResourceInitializer();

        await resourceInitializer.InitializeAsync(StoragePaths.Root, ct);
    }

    private static void EnsureStorage()
    {
        foreach (var dir in StoragePaths.AllDirectories)
        {
            Directory.CreateDirectory(dir);
        }
    }


    private static ResourceInitializer CreateResourceInitializer()
    {
        var  primary = new
            List<ResourcePackage>
            {
                new()
                {
                    Name = "Locales",
                    DestinationPath = StoragePaths.Locales,
                    Source = new GitHubResourceSource(
                        "https://github.com/yourrepo/releases/download/v1/resources1.zip"
                    )
                },
                 new()
                {
                    Name = "Locales",
                    DestinationPath = StoragePaths.Locales,
                    Source = new GitHubResourceSource(
                        "https://github.com/yourrepo/releases/download/v1/resources2.zip"
                    )
                },
                 new()
                {
                    Name = "Locales",
                    DestinationPath = StoragePaths.Locales,
                    Source = new GitHubResourceSource(
                        "https://github.com/yourrepo/releases/download/v1/resources3.zip"
                    )
                }
            };

        var fallback = new List<ResourcePackage>
{
    new()
    {
        Name = "Locales",
        DestinationPath = StoragePaths.Locales,
        Source = new EmbeddedResourceSource("AmbinityDB.Infrastructure.Resources.EmbeddedResources.Locales.Langs.zip")
    },
    new()
    {
        Name = "Images",
        DestinationPath = StoragePaths.Images,
        Source = new EmbeddedResourceSource("AmbinityDB.Infrastructure.Resources.EmbeddedResources.Images.Images.zip")
    },
    new()
    {
        Name = "Layouts",
        DestinationPath = StoragePaths.DeviceLayoutsFolderPath,
        Source = new EmbeddedResourceSource("AmbinityDB.Infrastructure.Resources.EmbeddedResources.AmbinityDevices.DefaultDevices.zip")
    },
    new()
    {
        Name = "Profiles",
        DestinationPath = StoragePaths.ProfilesFolderPath,
        Source = new EmbeddedResourceSource("AmbinityDB.Infrastructure.Resources.EmbeddedResources.AmbinityLightingProfiles.Profiles.zip")
    },
      new()
    {
        Name = "FirmwareTools",
        DestinationPath = StoragePaths.FirmwareTools,
        Source = new EmbeddedResourceSource("AmbinityDB.Infrastructure.Resources.EmbeddedResources.Tools.FirmwareTools.zip")
    },
     new()
    {
        Name = "ProfileCategories",
        DestinationPath = StoragePaths.ProfileCategoriesFolderPath,
        Source = new EmbeddedResourceSource("AmbinityDB.Infrastructure.Resources.EmbeddedResources.ProfileCategories.DefaultCategories.zip")
    }
};

        return new ResourceInitializer(primary, fallback, new ResourceInstaller());
    }
}
