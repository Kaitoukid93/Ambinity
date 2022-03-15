using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    internal class DefaultDeviceCollection
    {
        public static DeviceSettings ambinoBasic = new DeviceSettings {
            DeviceName = "Ambino Basic",
            DeviceID = 0,
            DeviceSerial = "ABBASICCH552",
            DeviceType = "ABRev2",
            Manufacturer = "Ambino Vietnam",
            FirmwareVersion = "1.0.3",
            ProductionDate = "2022",
            IsVisible = true,
            IsEnabled = true,
            OutputPort = "Không có",
            IsTransferActive = true,
            AvailableOutputs =   new OutputSettings[] {DefaulOutputCollection.rectangleFrameLEDSetupOutputType2(0), DefaulOutputCollection.rectangleFrameLEDSetupOutputType1(1) }

        };

    }
}
