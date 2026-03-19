using System.Text;
using Newtonsoft.Json;

namespace AmbinityDB.Utils;

public static class JsonHelper
{

    private static readonly JsonSerializerSettings DefaultSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };
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
    public static async Task SerializeToFileAsync<T>(
       Stream stream,
       T value,
       CancellationToken cancellationToken = default)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        using var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true);
        using var jsonWriter = new JsonTextWriter(writer)
        {
            Formatting = Formatting.Indented
        };

        var serializer = JsonSerializer.Create(DefaultSettings);

        serializer.Serialize(jsonWriter, value);

        await jsonWriter.FlushAsync(cancellationToken);
    }


}
