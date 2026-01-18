namespace AmbinityCore.Repositories.Services;
public sealed class ImportRequest
{
    public string SourcePath { get; init; } = string.Empty;
    public bool IsZip { get; init; }
    public bool OverwriteExisting { get; init; }
    public object? Context { get; init; } // optional (category, etc.)
}
