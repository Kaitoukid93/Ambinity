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
            //Bind<IDesktopDuplicatorReader>().To<DesktopDuplicatorReader>().InSingletonScope().WithConstructorArgument("graphicAdapter",0).WithConstructorArgument("output",0);
            //Bind<IDesktopDuplicatorReaderSecondary>().To<DesktopDuplicatorReaderSecondary>().InSingletonScope().WithConstructorArgument("graphicAdapter", 0).WithConstructorArgument("output", 1);
            //Bind<IDesktopDuplicatorReaderThird>().To<DesktopDuplicatorReaderThird>().InSingletonScope().WithConstructorArgument("graphicAdapter", 0).WithConstructorArgument("output", 2);
            Bind<IGeneralSpotSet>().To<GeneralSpotSet>().InSingletonScope();
            Bind<IGeneralSettings>().ToConstant(generalSettings);
            Bind<IOpenRGBClientDevice>().To<OpenRGBClientDevice>().InSingletonScope();
            Bind<ISerialDeviceDetection>().To<SerialDeviceDetection>().InSingletonScope();
            Bind<IShaderEffect>().To<ShaderEffect>().InSingletonScope();
            Bind<IContext>().To<WpfContext>().InSingletonScope();
            Bind<IDesktopDuplicator>().To<DesktopDuplicator>().InSingletonScope();

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
