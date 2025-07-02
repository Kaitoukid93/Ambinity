using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Serilog.Core;

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

    // /// <summary>
    // /// get screenshot from path and cache
    // /// </summary>
    // /// <param name="path"></param>
    // /// <returns></returns>
    // public async Task<List<Bitmap>> GetScreenshots(string path, int width = 600)
    // {
    //     List<Bitmap> screenshots = new List<Bitmap>();
    //     var availableScreenshotPath = await _client.SftpServer.GetAllFilesAddressInFolder(path);
    //     foreach (var address in availableScreenshotPath)
    //     {
    //         Bitmap screenshot;
    //         if (!_cache.TryGetValue(address, out screenshot))
    //         {
    //             // Not in the cache, so load from ambinity server
    //             _client.Init();
    //             await using (var stream = await _client.SftpServer.GetThumb(address))
    //             {
    //                 screenshot = Bitmap.DecodeToWidth(stream, width);
    //             }
    //             screenshots.Add(screenshot);
    //             _cache.Add(address, screenshot);
    //         }
    //     }
    //
    //     return screenshots;
    // }
    /// <summary>
    /// load thumbnail from local path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<Bitmap> LoadThumbnail(string path, int width = 100, bool discardCache = false)
    {
        Bitmap thumbnail;

        if (discardCache)
        {
            _cache.Remove(path);
        }
        if (!_cache.TryGetValue(path, out thumbnail))
        {
            // Not in the cache, so load from path server
            await using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                thumbnail = Bitmap.DecodeToWidth(stream, width);
            }

            _cache.Add(path, thumbnail);
        }

        return thumbnail;
    }

    /// <summary>
    /// get gif animated stream
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task LoadAnimatedStream(string path, Stream output)
    {
        // Not in the cache, so load from path server
        _client.Init();
        await _client.SftpServer.DownloadFile(path, output);
    }

    public void ClearCache(string path)
    {
        _cache.Remove(path);
    }
}
