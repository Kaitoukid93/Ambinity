namespace AmbinityDB.Infrastructure.Resources;
public class ResourcePackage
{
    public string Name { get; set; } = default!;
    public string DestinationPath { get; set; } = default!;
    public IResourceSource Source { get; set; } = default!;
}
