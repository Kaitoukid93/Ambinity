using adrilight.Resources;
using adrilight.Spots;
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
            //Bind<IOpenRGBClientDevice>().To<OpenRGBClientDevice>().InSingletonScope();
            Bind<ISerialDeviceDetection>().To<SerialDeviceDetection>().InSingletonScope();
            Bind<IShaderEffect>().To<ShaderEffect>().InSingletonScope();
            Bind<IContext>().To<WpfContext>().InSingletonScope();
            //Bind<IOpenRGBStream>().To<OpenRGBStream>().InSingletonScope();
            Bind<IDesktopFrame>().To<DesktopFrame>().InSingletonScope();
            Bind<ISecondDesktopFrame>().To<SecondDesktopFrame>().InSingletonScope();
            Bind<IThirdDesktopFrame>().To<ThirdDesktopFrame>().InSingletonScope();
            Bind<IRainbowTicker>().To<RainbowTicker>().InSingletonScope();

            if (alldevicesettings!=null)
            {
                if (alldevicesettings.Count > 0)
                {
                    foreach (var devicesetting in alldevicesettings)
                    {
                        var devicename = devicesetting.DeviceID.ToString();
                        //if (devicename == "151293")//non Ambino Device
                        //{
                        //    var DeviceSerial = devicesetting.DeviceSerial;
                        //    Bind<IDeviceSettings>().ToConstant(devicesetting).Named(DeviceSerial);
                        //}
                        //else
                        //{
                            Bind<IDeviceSettings>().ToConstant(devicesetting).Named(devicename);
                      
                        //}



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
