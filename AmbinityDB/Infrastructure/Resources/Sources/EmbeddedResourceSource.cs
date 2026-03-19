using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

public class EmbeddedResourceSource : IResourceSource
{
    private readonly Assembly _assembly;
    private readonly string _resourceName;

    public EmbeddedResourceSource(string resourceName, Assembly? assembly = null)
    {
        _assembly = assembly ?? Assembly.GetExecutingAssembly();
        _resourceName = resourceName;
    }

   public Task<Stream> GetArchiveAsync(CancellationToken cancellationToken = default)
    {
        var stream = _assembly.GetManifestResourceStream(_resourceName);

        if (stream == null)
        {
            var available = string.Join("\n", _assembly.GetManifestResourceNames());

            throw new InvalidOperationException(
                $"Embedded resource '{_resourceName}' not found.\nAvailable:\n{available}");
        }

        return Task.FromResult(stream);
    }
}
