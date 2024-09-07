using Avalonia.Media;
using Newtonsoft.Json;

namespace AmbinityCore.Helpers;

public class HexColorConverter : JsonConverter<Color>
{
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        // Convert the Color to its hex representation (e.g., "#RRGGBB")
        string hexColor = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
        writer.WriteValue(hexColor);
    }

    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            string hexColor = (string)reader.Value;
            if (hexColor.StartsWith("#") && hexColor.Length == 7)
            {
                // Parse the hex string and create a Color instance
                byte r = Convert.ToByte(hexColor.Substring(1, 2), 16);
                byte g = Convert.ToByte(hexColor.Substring(3, 2), 16);
                byte b = Convert.ToByte(hexColor.Substring(5, 2), 16);
                return Color.FromRgb(r, g, b);
            }
            else if (hexColor.StartsWith("#") && hexColor.Length == 9)
            {
                byte a = Convert.ToByte(hexColor.Substring(1, 2), 16);
                byte r = Convert.ToByte(hexColor.Substring(3, 2), 16);
                byte g = Convert.ToByte(hexColor.Substring(5, 2), 16);
                byte b = Convert.ToByte(hexColor.Substring(7, 2), 16);
                return Color.FromRgb(r, g, b);
            }
           
        }
        // If parsing fails, return a default color (e.g., black)
        return Avalonia.Media.Colors.Black;
    }
}