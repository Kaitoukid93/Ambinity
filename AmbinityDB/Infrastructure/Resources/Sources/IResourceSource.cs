using System.IO;
using System.Threading;
using System.Threading.Tasks;

public interface IResourceSource
{
    Task<Stream> GetArchiveAsync(CancellationToken cancellationToken = default);
}
