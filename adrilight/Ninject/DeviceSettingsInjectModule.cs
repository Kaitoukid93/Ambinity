using adrilight.Resources;
using adrilight.Util;
using Ninject.Modules;

namespace adrilight.Ninject
{
    class DeviceSettingsInjectModule : NinjectModule
    {
        public override void Load()
        {
            var settingsManager = new UserSettingsManager();
            var generalSettings = settingsManager.LoadIfExists() ?? settingsManager.MigrateOrDefault();
            var alldevicesettings = settingsManager.LoadDeviceIfExists();
            Bind<IGeneralSettings>().ToConstant(generalSettings);
            Bind<IOpenRGBClientDevice>().To<OpenRGBClientDevice>().InSingletonScope();
            Bind<ISerialDeviceDetection>().To<SerialDeviceDetection>().InSingletonScope();
            Bind<IShaderEffect>().To<ShaderEffect>().InSingletonScope();
            Bind<IContext>().To<WpfContext>().InSingletonScope();
           
            Bind<IDesktopFrame>().To<DesktopFrame>().InSingletonScope();

            if (alldevicesettings!=null)
            {
                if (alldevicesettings.Count > 0)
                {
                    foreach (var devicesetting in alldevicesettings)
                    {
                        var devicename = devicesetting.DeviceID.ToString();
                        if (devicename == "151293")//non Ambino Device
                        {
                            var DeviceSerial = devicesetting.DeviceSerial;
                            Bind<IDeviceSettings>().ToConstant(devicesetting).Named(DeviceSerial);
                        }
                        else
                        {
                            Bind<IDeviceSettings>().ToConstant(devicesetting).Named(devicename);
                        }
                            
             

                    }
                }
            }
            else
            {
                // require user to add device then restart the app
            }
           
          
        }
    }
}
