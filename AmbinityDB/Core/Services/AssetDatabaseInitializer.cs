using AmbinityDB.Core.Interfaces;
using AmbinityDB.Core.Models;
using AmbinityDB.Providers.Local;

namespace AmbinityDB.Core.Services;

public static class AssetDatabaseInitializer
{
    public static async Task<DatabaseManager> InitializeAsync(
        string assetRoot,
        string manifestPath)
    {
        var loader = new ManifestLoader(manifestPath);
        var validator = new ManifestValidator();
        var builder = new ManifestBuilder(assetRoot);

        ManifestModel? manifest = await loader.LoadAsync();

        bool rebuild = false;

        if (manifest == null)
        {
            rebuild = true;
        }
        else
        {
            if (!validator.Validate(manifest, assetRoot, out var errors))
            {
                rebuild = true;

                Console.WriteLine("Manifest validation failed:");

                foreach (var error in errors)
                {
                    Console.WriteLine($" - {error}");
                }
            }
        }

        if (rebuild)
        {
            Console.WriteLine("Rebuilding manifest...");

            manifest = builder.Build();

            await loader.SaveAsync(manifest);
        }

        // Build manifest index
        var index = new ManifestIndex();

        foreach (var entry in manifest!.Assets)
        {
            index.Add(entry);
        }

        // Setup providers
        var sources = new List<IDatabaseSource>
        {
            new LocalDatabaseSource("local", assetRoot)
        };

        // Create resolver
        var resolver = new AssetResolver(sources);

        // Create database manager
        var db = new DatabaseManager(sources, resolver);

        // Inject index if your manager supports it
        db.SetIndex(index);

        return db;
    }
}
