using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using AmbinityCore;
using Serilog;

namespace Ambinity.Localization
{
    public static class Loc
    {
        private static Dictionary<string, string> _strings = new();

        public static string CurrentLanguage { get; private set; } = "en";

        public static event Action? LanguageChanged;

        public static void Load(string langCode)
        {
            var file = Path.Combine(Constants.AmbinityLocalesFolder, $"{langCode}.json");
            if (!File.Exists(file))
                Log.Error("Locale file not found: {File}", file);

            var json = File.ReadAllText(file);
            _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
            CurrentLanguage = langCode;
            LanguageChanged?.Invoke();
        }

        public static string Get(string key)
        {
            if (_strings.TryGetValue(key, out var value))
                return value;
            return $"[{key}]"; // fallback
        }
        public static string TryGetTranslated(string key, string fallback)
        {
            if (_strings.TryGetValue(key, out var value))
                return value;
            return fallback; // fallback
        }

    }
}
