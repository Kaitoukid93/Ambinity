﻿using BO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace adrilight
{
    class UserSettingsManager
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");

        private string JsonFileNameAndPath => Path.Combine(JsonPath, "adrilight-settings.json");
        private string JsonDeviceFileNameAndPath => Path.Combine(JsonPath, "adrilight-deviceInfos.json");
        private string JsonGroupFileNameAndPath => Path.Combine(JsonPath, "adrilight-groupInfos.json");

        private void SaveSettings(IGeneralSettings generalSettings)
        {
            var json = JsonConvert.SerializeObject(generalSettings, Formatting.Indented);
            Directory.CreateDirectory(JsonPath);
            File.WriteAllText(JsonFileNameAndPath, json);
        }
        

        public IGeneralSettings LoadIfExists()
        {
            if (!File.Exists(JsonFileNameAndPath)) return null;

            var json = File.ReadAllText(JsonFileNameAndPath);

            var generalSettings = JsonConvert.DeserializeObject<GeneralSettings>(json);
            generalSettings.PropertyChanged += (_, __) => SaveSettings(generalSettings);

           HandleAutostart(generalSettings);
            return generalSettings;
        }

        public List<DeviceSettings> LoadDeviceIfExists()
        {
            if (!File.Exists(JsonDeviceFileNameAndPath)) return null;

            var json = File.ReadAllText(JsonDeviceFileNameAndPath);

            var devices = JsonConvert.DeserializeObject<List<DeviceSettings>>(json);

            return devices;
        }
        public List<GroupSettings> LoadGroupIfExisrs()
        {
            if (!File.Exists(JsonGroupFileNameAndPath)) return null;

            var json = File.ReadAllText(JsonGroupFileNameAndPath);

            var groups = JsonConvert.DeserializeObject<List<GroupSettings>>(json);

            return groups;
        }

        private static void HandleAutostart(GeneralSettings settings)
        {
            if (settings.Autostart)
            {
                StartUpManager.AddApplicationToCurrentUserStartup();
            }
            else
            {
                StartUpManager.RemoveApplicationFromCurrentUserStartup();
            }
        }
        public IGeneralSettings MigrateOrDefault()
        {
           
            var generalSettings = new GeneralSettings();
            generalSettings.PropertyChanged += (_, __) => SaveSettings(generalSettings);
            var legacyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight");
            if (!Directory.Exists(legacyPath)) return generalSettings;

            var legacyFiles = Directory.GetFiles(legacyPath, "user.config", SearchOption.AllDirectories);

            var file = legacyFiles
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(fi => fi.LastWriteTimeUtc)
                        .FirstOrDefault();


            return generalSettings;
        }

    

        //private void ReadAndApply<T>(XDocument xdoc, IUserSettings settings, string settingName, Expression<Func<IUserSettings, T>> targetProperty)
        //{
        //    var content = xdoc.XPathSelectElement($"//setting[@name='{settingName}']/value");
        //    if (content == null) return;


        //    var text = content.Value;
        //    var propertyExpression = (MemberExpression)targetProperty.Body;
        //    var member = (PropertyInfo)propertyExpression.Member;

        //    object targetValue;

        //    if (typeof(T) == typeof(int))
        //    {
        //        //int
        //        targetValue = Convert.ToInt32(text);
        //    }
        //    else if (typeof(T) == typeof(byte))
        //    {
        //        //byte
        //        targetValue = Convert.ToByte(text);
        //    }
        //    else if (typeof(T) == typeof(bool))
        //    {
        //        //bool
        //        targetValue = Convert.ToBoolean(text);
        //    }
        //    else if (typeof(T) == typeof(string))
        //    {
        //        //string
        //        targetValue = text;
        //    }
        //    else
        //    {
        //        throw new NotImplementedException($"converting to {typeof(T).FullName} is not implemented");
        //    }

        //    member.SetValue(settings, targetValue);
        //}
    }
}
