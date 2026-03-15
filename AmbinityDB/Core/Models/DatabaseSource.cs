namespace AmbinityDB.Core.Models;

public class DatabaseSource
{
    public string Name { get; set; } = default!;

    public string BasePath { get; set; } = default!;

    public bool IsRemote { get; set; }
}
