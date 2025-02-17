using System.Reflection;
using Avalonia;
using Avalonia.Platform;
using Newtonsoft.Json;

namespace AmbinityCore.Helpers;

public static class ResourceLoader
{
    public static T LoadJsonResource<T>(string resourceName)
    {
        var uri = new Uri(resourceName);
        using (var stream = AssetLoader.Open(uri))
        {
            if (stream == null)
                throw new FileNotFoundException("Resource not found", resourceName);

            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(json);
            }
        }
    }
}