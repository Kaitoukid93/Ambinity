using AmbinityDB.Core.Models;

namespace AmbinityCore.Repositories.Services;
public sealed class ImportResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public ManifestEntry? Entry { get; init; }
}
