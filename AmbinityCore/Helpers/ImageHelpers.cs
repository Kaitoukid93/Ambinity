using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AmbinityCore.Helpers;

public  class ImageHelpers
{
    public  Bitmap LoadFromResource(Uri resourceUri)
    {
        return new Bitmap(AssetLoader.Open(resourceUri));
    }
 
    public  async Task<Bitmap?> LoadFromWeb(Uri url)
    {
        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            return new Bitmap(new MemoryStream(data));
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"An error occurred while downloading image '{url}' : {ex.Message}");
            return null;
        }
    }
    public async Task<Bitmap?> LoadFromFile(string file)
    {
        FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        var memory = new MemoryStream();
        fs.CopyTo(memory);
        return new Bitmap(memory);
    }
}