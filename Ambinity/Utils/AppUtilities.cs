using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AmbinityCore.Models.Device;
using AmbinityCore;
using Serilog;

namespace Ambinity.Utils
{
    public static class AppUtilities
    {
        /// <summary>
        /// Removes all devices from the device repository and restarts the app.
        /// </summary>
        public static void RemoveAllDevices()
        {


            // Delete Hardwares folder from AppDataFolder
            string hardwaresFolder = Path.Combine(Constants.AppDataFolder, "Hardwares");
            try
            {
                if (Directory.Exists(hardwaresFolder))
                {
                    Directory.Delete(hardwaresFolder, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete Hardwares folder: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets application settings to default and restarts the app.
        /// </summary>
        public static void ResetAppSettings()
        {

            // Remove the general settings file
            try
            {
                if (File.Exists(Constants.GeneralSettingsFilePath))
                {
                    File.Delete(Constants.GeneralSettingsFilePath);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to delete settings file: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes all application data and restarts the app.
        /// </summary>
        public static void RemoveAppData()
        {
            // Remove ModelDataFolder from Constants
            string modelDataPath = Constants.ModelDataFolder;
            try
            {
                if (Directory.Exists(modelDataPath))
                {
                    Directory.Delete(modelDataPath, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to remove model data: {ex.Message}");
            }
        }


    }
}
