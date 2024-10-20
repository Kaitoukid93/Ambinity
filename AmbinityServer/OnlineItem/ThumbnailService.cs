using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AmbinityServer.OnlineItem;

public sealed class ThumbnailService
{
    public ThumbnailService(AmbinityClient client)
    {
        _client = client;
        var bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://Ambinity/Assets/Images/generalImage.png")));
        _cache.Add("null", bitmap);
    }

    private AmbinityClient _client;
    readonly string _connectionString;
    readonly Dictionary<string, Bitmap> _cache = new Dictionary<string, Bitmap>();

    /// <summary>
    /// get thumbnail from path and cache
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<Bitmap> GetThumbnail(string path, int width = 50)
    {
        Bitmap thumbnail;

        if (!_cache.TryGetValue(path, out thumbnail))
        {
            // Not in the cache, so load from ambinity server
            _client.Init();
            await using (var stream = await _client.SftpServer.GetThumb(path))
            {
                thumbnail = Bitmap.DecodeToWidth(stream, width);
            }

            _cache.Add(path, thumbnail);
        }
        return thumbnail;
    }
    /// <summary>
    /// load thumbnail from local path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>

    public async Task<Bitmap> LoadThumbnail(string path)
    {
        Bitmap thumbnail;

        if (!_cache.TryGetValue(path, out thumbnail))
        {
            // Not in the cache, so load from path server
            await using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                thumbnail = Bitmap.DecodeToWidth(stream, 100);
            }
            _cache.Add(path, thumbnail);
        }
        return thumbnail;
    }

    public void ClearCache(string path)
    {
        _cache.Remove(path);
    }
}