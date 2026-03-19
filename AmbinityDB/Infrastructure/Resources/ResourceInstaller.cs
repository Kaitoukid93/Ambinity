
using System.IO;
using System.IO.Compression;

namespace AmbinityDB.Infrastructure.Resources;

public class ResourceInstaller
{
    public void Install(Stream archiveStream, string destination)
    {
        Directory.CreateDirectory(destination);

        using var archive = new ZipArchive(archiveStream);

        archive.ExtractToDirectory(destination, overwriteFiles: true);

        CleanupMacOSFolder(destination);
    }

    private void CleanupMacOSFolder(string root)
    {
        var macFolder = Path.Combine(root, "__MACOSX");
        if (Directory.Exists(macFolder))
        {
            Directory.Delete(macFolder, true);
        }
    }
}
