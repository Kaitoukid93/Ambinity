using Newtonsoft.Json;

namespace AmbinityCore.Helpers;

public class ObjectHelpers
{
    private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { 
        TypeNameHandling = TypeNameHandling.Auto
    };
    /// <summary>
    /// Clones Any Object.
    /// </summary>
    /// <param name="objectToClone">The object to clone.</param>
    /// <return>The Clone</returns>
    public static T Clone<T>(T objectToClone)
    {
        T cloned_obj = default(T);

        var objectJson = JsonConvert.SerializeObject(objectToClone, jsonSerializerSettings);

        cloned_obj = JsonConvert.DeserializeObject<T>(objectJson,jsonSerializerSettings);


        return cloned_obj;
    }
}