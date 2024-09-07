using Newtonsoft.Json;
using Serilog;

namespace AmbinityCore.Helpers;

public class JsonHelpers
{
    private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { 
        TypeNameHandling = TypeNameHandling.Auto
    };
    
    private static object Lock = new object();
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
    public static void WriteSimpleJson(object obj, string path,JsonConverter customConverter)
    {
        var jsonSerializerSettings =  new JsonSerializerSettings() { 
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = new List<JsonConverter>(){customConverter}
        };
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
        if (!File.Exists(path))
        {
            Log.Error(path + " not found");
            return default(T);
        }
        
        lock (Lock)
        {
            var jsonData = File.ReadAllText(path);
            T obj = JsonConvert.DeserializeObject<T>(jsonData,jsonSerializerSettings);
            return obj;
        }
        
    }
    public static T DeserializeJson<T>(string path, JsonConverter customConverter)
    {
        var jsonDeserializerSettings =  new JsonSerializerSettings() { 
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = new List<JsonConverter>(){customConverter}
        };
        lock (Lock)
        {
            var jsonData = File.ReadAllText(path);
            T obj = JsonConvert.DeserializeObject<T>(jsonData,jsonDeserializerSettings);
            return obj;
        }
        
    }
}