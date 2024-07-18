using Newtonsoft.Json;

namespace AmbinityCore.Helpers;

public class JsonHelpers
{
    private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { 
        TypeNameHandling = TypeNameHandling.Auto
    };
    public static void WriteSimpleJson(object obj, string path)
    {
        try
        {
            var json = JsonConvert.SerializeObject(obj,jsonSerializerSettings);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            //log
        }
    }

    public static T DeserializeJson<T>(string path)
    {
        var jsonData = File.ReadAllText(path);
        T obj = JsonConvert.DeserializeObject<T>(jsonData,jsonSerializerSettings);
        return obj;
    }
}