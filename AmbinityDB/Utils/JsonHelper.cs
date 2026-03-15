using Newtonsoft.Json;

namespace AmbinityDB.Utils;

public static class JsonHelper
{
    public static async Task<T?> DeserializeAsync<T>(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        return JsonConvert.DeserializeObject<T>(json);
    }

    public static async Task SerializeToFileAsync<T>(string path, T obj)
    {
        var json = JsonConvert.SerializeObject(
            obj,
            Formatting.Indented);

        await File.WriteAllTextAsync(path, json);
    }
}
