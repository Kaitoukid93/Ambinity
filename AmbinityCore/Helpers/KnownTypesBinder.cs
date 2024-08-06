using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace AmbinityCore.Helpers;

public class KnownTypesBinder : ISerializationBinder
{
    public KnownTypesBinder()
    {
        KnownTypes = new List<Type>();
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            KnownTypes.Add(type);
        }
    }
    public IList<Type> KnownTypes { get; set; }

    public Type BindToType(string assemblyName, string typeName)
    {
        var name = typeName.Split('.').Last();
        return KnownTypes.SingleOrDefault(t => t.Name == name);
    }

    public void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        assemblyName = null;
        typeName = serializedType.Name;
    }
}