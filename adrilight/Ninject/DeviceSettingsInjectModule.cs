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
            var existedDevices = settingsManager.LoadDeviceIfExists();
            var allgroupsettings = settingsManager.LoadGroupIfExisrs();
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
            //Bind<IHotKeyManager>().To<HotKeyManager>().InSingletonScope();

            if (existedDevices != null)
            {
                if (existedDevices.Count > 0)
                {
                    foreach (var device in existedDevices)
                    {
                        var iD = device.DeviceID.ToString();
                       
                            Bind<IDeviceSettings>().ToConstant(device).Named(iD);
                        foreach(var output in device.AvailableOutputs)
                        {
                            var outputID = iD+output.OutputID.ToString();
                            Bind<IOutputSettings>().ToConstant(output).Named(outputID);
                        }
                    }
                }
            }
            else
            {
                // require user to add device then restart the app
            }

            if (allgroupsettings != null)
            {
                if (allgroupsettings.Count > 0)
                {
                    foreach (var groupsettings in allgroupsettings)
                    {
                        var groupID = groupsettings.GroupID.ToString();

                        Bind<IGroupSettings>().ToConstant(groupsettings).Named(groupID);
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
