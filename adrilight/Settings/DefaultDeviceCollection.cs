using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Settings
{
    internal class DefaultDeviceCollection
    {
        public static DeviceSettings ambinoBasic24 = new DeviceSettings {
            DeviceName = "Ambino Basic 24 inch",
            DeviceSerial = "ABBASIC24",
            DeviceType = "ABBASIC24",
            Manufacturer = "Ambino Vietnam",
            FirmwareVersion = "1.0.3",
            ProductionDate = "2022",
            IsVisible = true,
            IsEnabled = true,
            OutputPort = "Không có",
            Geometry= "ambinobasic",
            IsTransferActive = true,
            AvailableOutputs =   new OutputSettings[] {DefaulOutputCollection.AmbinoBasic(0,11,7)}

        };
        public static DeviceSettings ambinoBasic27 = new DeviceSettings {
            DeviceName = "Ambino Basic 27 inch",
            DeviceSerial = "ABBASIC27",
            DeviceType = "ABBASIC27",
            Manufacturer = "Ambino Vietnam",
            FirmwareVersion = "1.0.3",
            ProductionDate = "2022",
            IsVisible = true,
            IsEnabled = true,
            OutputPort = "Không có",
            Geometry = "ambinobasic",
            IsTransferActive = true,
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoBasic(0, 13, 7) }

        };
        public static DeviceSettings ambinoBasic29 = new DeviceSettings {
            DeviceName = "Ambino Basic 29 inch",
            DeviceSerial = "ABBASIC29",
            DeviceType = "ABBASIC29",
            Manufacturer = "Ambino Vietnam",
            FirmwareVersion = "1.0.3",
            ProductionDate = "2022",
            IsVisible = true,
            IsEnabled = true,
            OutputPort = "Không có",
            Geometry = "ambinobasic",
            IsTransferActive = true,
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoBasic(0, 14, 7) }

        };
        public static DeviceSettings ambinoBasic32 = new DeviceSettings {
            DeviceName = "Ambino Basic 32 inch",
            DeviceSerial = "ABBASIC32",
            DeviceType = "ABBASIC32",
            Manufacturer = "Ambino Vietnam",
            FirmwareVersion = "1.0.3",
            ProductionDate = "2022",
            IsVisible = true,
            IsEnabled = true,
            OutputPort = "Không có",
            Geometry = "ambinobasic",
            IsTransferActive = true,
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoBasic(0, 15, 8) }

        };
        public static DeviceSettings ambinoBasic34 = new DeviceSettings {
            DeviceName = "Ambino Basic 34 inch",
            DeviceSerial = "ABBASIC34",
            DeviceType = "ABBASIC34",
            Manufacturer = "Ambino Vietnam",
            FirmwareVersion = "1.0.3",
            ProductionDate = "2022",
            IsVisible = true,
            IsEnabled = true,
            OutputPort = "Không có",
            Geometry = "ambinobasic",
            IsTransferActive = true,
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoBasic(0, 17, 7) }

        };
        public static DeviceSettings ambinoEdge1m2 = new DeviceSettings {
            DeviceName = "Ambino EDGE 1.2m",
            DeviceSerial = "ABEDGE1.2",
            DeviceType = "ABEDGE1.2",
            Manufacturer = "Ambino Vietnam",
            FirmwareVersion = "1.0.3",
            ProductionDate = "2022",
            IsVisible = true,
            IsEnabled = true,
            OutputPort = "Không có",
            Geometry = "ambinoedge",
            IsTransferActive = true,
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoEdge(0, 40) }

        };
         public static DeviceSettings ambinoEdge2m = new DeviceSettings {
             DeviceName = "Ambino EDGE 1.2m",
             DeviceSerial = "ABEDGE2.0",
             DeviceType = "ABEDGE2.0",
             Manufacturer = "Ambino Vietnam",
             FirmwareVersion = "1.0.3",
             ProductionDate = "2022",
             IsVisible = true,
             IsEnabled = true,
             OutputPort = "Không có",
             Geometry = "ambinoedge",
             IsTransferActive = true,
             AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoEdge(0, 80) }

         };
        public static DeviceSettings ambinoFanHub = new DeviceSettings {
            DeviceName = "Ambino FanHub",
            DeviceSerial = "ABFANHUB",
            DeviceType = "ABFANHUB",
            Manufacturer = "Ambino Vietnam",
            FirmwareVersion = "1.0.0",
            ProductionDate = "2022",
            IsVisible = true,
            IsEnabled = true,
            OutputPort = "Không có",
            Geometry = "ambinofanhub",
            IsTransferActive = true,
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.GenericRectangle("Fan1",0, 5,5),
                                                      DefaulOutputCollection.GenericRectangle("Fan2",1, 5,5),
                                                      DefaulOutputCollection.GenericRectangle("Fan3",2, 5,5),
                                                      DefaulOutputCollection.GenericRectangle("Fan4",3, 5,5),
                                                      DefaulOutputCollection.GenericRectangle("Fan5",4, 5,5),
                                                      DefaulOutputCollection.GenericRectangle("Fan6",5, 5,5),
                                                      DefaulOutputCollection.GenericRectangle("Fan7",6, 5,5),
                                                      DefaulOutputCollection.GenericRectangle("Fan8",7, 5,5),
                                                      DefaulOutputCollection.GenericRectangle("Fan9",8, 5,5),
                                                      DefaulOutputCollection.GenericRectangle("Fan10",9, 5,5)
            },
            
                                                      
            

        };
        public static DeviceSettings ambinoHUBV2 = new DeviceSettings {
            DeviceName = "Ambino HUBV2",
            DeviceSerial = "ABHUBV2",
            DeviceType = "ABHUBV2",
            Manufacturer = "Ambino Vietnam",
            FirmwareVersion = "1.0.7",
            ProductionDate = "2020",
            IsVisible = true,
            IsEnabled = true,
            OutputPort = "Không có",
            Geometry = "ambinohub",
            IsTransferActive = true,
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.GenericLEDStrip(0, 64),
                                                      DefaulOutputCollection.GenericLEDStrip(1, 64),
                                                      DefaulOutputCollection.AmbinoBasic(2, 11,7),
                                                      DefaulOutputCollection.AmbinoBasic(3, 11,7),
                                                      DefaulOutputCollection.AmbinoBasic(4, 11,7),
                                                      DefaulOutputCollection.GenericLEDStrip(5, 12)
                                                     
            }

        };
        public static DeviceSettings ambinoHUBV3 = new DeviceSettings {
            DeviceName = "Ambino HUBV3",
            DeviceSerial = "ABHUBV3",
            DeviceType = "ABHUBV3",
            Manufacturer = "Ambino Vietnam",
            FirmwareVersion = "1.0.0",
            ProductionDate = "2020",
            IsVisible = true,
            IsEnabled = true,
            OutputPort = "Không có",
            Geometry = "ambinohubv3",
            IsTransferActive = true,
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.GenericLEDStrip(0, 64),
                                                      DefaulOutputCollection.GenericLEDStrip(1, 64),
                                                      DefaulOutputCollection.GenericLEDStrip(2, 64),
                                                      DefaulOutputCollection.GenericLEDStrip(2, 64)



            }

        };
    }
}
