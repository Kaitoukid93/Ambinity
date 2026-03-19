using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AmbinityDB.Infrastructure.Resources;

public class ResourceInitializer
{
    private readonly IEnumerable<ResourcePackage> _primary;
    private readonly IEnumerable<ResourcePackage> _fallback;
    private readonly ResourceInstaller _installer;

    public ResourceInitializer(
        IEnumerable<ResourcePackage> primary,
        IEnumerable<ResourcePackage> fallback,
        ResourceInstaller installer)
    {
        _primary = primary;
        _fallback = fallback;
        _installer = installer;
    }

    public async Task InitializeAsync(string rootPath, CancellationToken ct = default)
    {
        var initFlag = Path.Combine(rootPath, ".init");

        if (File.Exists(initFlag))
            return;


        try
        {
            foreach (var package in _primary)
            {
                // if (Directory.Exists(package.DestinationPath))
                //     continue;

                using var stream = await package.Source.GetArchiveAsync(ct);

                _installer.Install(stream, package.DestinationPath);
            }
        }
        catch
        {
            foreach (var package in _fallback)
            {
                // if (Directory.Exists(package.DestinationPath))
                //     continue;

                using var stream = await package.Source.GetArchiveAsync(ct);

                _installer.Install(stream, package.DestinationPath);
            }
        }
        File.WriteAllText(initFlag, "initialized");
    }
}
