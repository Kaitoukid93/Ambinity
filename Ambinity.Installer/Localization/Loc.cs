using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Serilog;

namespace Ambinity.Installer.Localization
{
    public static class Loc
    {
        private static Dictionary<string, string> _strings = new();

        public static string CurrentLanguage { get; private set; } = "en";

        public static event Action? LanguageChanged;

        public static void Load(string langCode)
        {
            // 1) Try loading embedded resource from this assembly: AppResource/Locales/{langCode}.json
            var asm = typeof(Loc).Assembly;
            string? resourceName = asm.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith($"AppResource.Locales.{langCode}.json", StringComparison.OrdinalIgnoreCase)
                                     || n.EndsWith($".{langCode}.json", StringComparison.OrdinalIgnoreCase));

            string? json = null;

            if (resourceName != null)
            {
                try
                {
                    using var stream = asm.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        json = reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to read embedded locale resource '{Resource}'", resourceName);
                }
            }

            // 2) Fallback to disk: AppResource/Locales/{langCode}.json under application base dir
            if (json == null)
            {
                var projectLocalePath = Path.Combine(AppContext.BaseDirectory, "AppResource", "Locales", $"{langCode}.json");
                var localFallbackPath = Path.Combine(AppContext.BaseDirectory, "Locales", $"{langCode}.json");

                string? fileToUse = null;
                if (File.Exists(projectLocalePath)) fileToUse = projectLocalePath;
                else if (File.Exists(localFallbackPath)) fileToUse = localFallbackPath;

                if (fileToUse != null)
                {
                    try
                    {
                        json = File.ReadAllText(fileToUse);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to read locale file from disk: {File}", fileToUse);
                    }
                }
                else
                {
                    Log.Error("Locale file not found. Embedded resource: {Resource}, disk paths: {ProjectPath}, {LocalPath}",
                        resourceName ?? "<none>", projectLocalePath, localFallbackPath);
                }
            }

            if (json == null)
            {
                _strings = new Dictionary<string, string>();
                CurrentLanguage = langCode;
                LanguageChanged?.Invoke();
                return;
            }

            try
            {
                _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                CurrentLanguage = langCode;
                LanguageChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to parse locale JSON for language {Lang}", langCode);
                _strings = new Dictionary<string, string>();
                CurrentLanguage = langCode;
                LanguageChanged?.Invoke();
            }
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
