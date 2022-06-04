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
            Geometry = "ambinobasic",
            IsUnionMode = true,
            IsTransferActive = true,
            UnionOutput = DefaulOutputCollection.AmbinoBasic(1, 11, 7, "LED Màn hình 24",true, "24inch"),
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoBasic(0, 11, 7, "LED Màn hình 24",false,"24inch") }

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
            IsUnionMode = true,
            IsTransferActive = true,
            UnionOutput = DefaulOutputCollection.AmbinoBasic(1, 13, 7, "LED Màn hình 27",true,"27inch"),
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoBasic(0, 13, 7, "LED Màn hình 27",false,"27inch") }

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
            IsUnionMode = true,
            Geometry = "ambinobasic",
            IsTransferActive = true,
            UnionOutput = DefaulOutputCollection.AmbinoBasic(1, 14, 7, "LED Màn hình 29",true,"29inch"),
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoBasic(0, 14, 7, "LED Màn hình 29",false,"29inch") }

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
            IsUnionMode = true,
            Geometry = "ambinobasic",
            IsTransferActive = true,
            UnionOutput = DefaulOutputCollection.AmbinoBasic(1, 15, 8, "LED Màn hình 32",true,"32inch"),
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoBasic(0, 15, 8, "LED Màn hình 32",false,"32inch") }

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
            IsUnionMode = true,
            Geometry = "ambinobasic",
            IsTransferActive = true,
            UnionOutput = DefaulOutputCollection.AmbinoBasic(1, 17, 7, "LED Màn hình 34",true,"34inch"),
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoBasic(0, 17, 7, "LED Màn hình 34",false,"34inch") }

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
            IsUnionMode = true,
            IsTransferActive = true,
            UnionOutput = DefaulOutputCollection.AmbinoEdge(1, 24, "LED Cạnh Bàn",1,true,"34inch"),
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoEdge(0, 24, "LED Cạnh Bàn",1,false,"34inch") }

        };
        public static DeviceSettings ambinoEdge2m = new DeviceSettings {
            DeviceName = "Ambino EDGE 2m",
            DeviceSerial = "ABEDGE2.0",
            DeviceType = "ABEDGE2.0",
            Manufacturer = "Ambino Vietnam",
            FirmwareVersion = "1.0.3",
            ProductionDate = "2022",
            IsVisible = true,
            IsEnabled = true,
            OutputPort = "Không có",
            Geometry = "ambinoedge",
            IsUnionMode = true,
            IsTransferActive = true,
            UnionOutput = DefaulOutputCollection.AmbinoEdge(1, 20, "LED Cạnh Bàn",2,true,"34inch"),
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.AmbinoEdge(0, 20, "LED Cạnh Bàn",2,false,"34inch") }

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
            UnionOutput = DefaulOutputCollection.GenericFan("Uni-Fan", 10, 5, 5,false),
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.GenericFan("Fan1",0, 5,5,true),
                                                      DefaulOutputCollection.GenericFan("Fan2",1, 5,5,true),
                                                      DefaulOutputCollection.GenericFan("Fan3",2, 5,5,true),
                                                      DefaulOutputCollection.GenericFan("Fan4",3, 5,5,true),
                                                      DefaulOutputCollection.GenericFan("Fan5",4, 5,5,true),
                                                      DefaulOutputCollection.GenericFan("Fan6",5, 5,5,true),
                                                      DefaulOutputCollection.GenericFan("Fan7",6, 5,5,true),
                                                      DefaulOutputCollection.GenericFan("Fan8",7, 5,5,true),
                                                      DefaulOutputCollection.GenericFan("Fan9",8, 5,5,true),
                                                      DefaulOutputCollection.GenericFan("Fan10",9, 5,5,true),
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
            UnionOutput = DefaulOutputCollection.GenericLEDStrip(6, 16, "Uni-Strip", 4, false,"ledstrip"),
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.GenericLEDStrip(0, 16,"Dải LED 1", 4,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(1, 16, "Dải LED 2", 4,true,"ledstrip"),
                                                      DefaulOutputCollection.AmbinoBasic(2, 11,7, "Màn 1",true,"24inch"),
                                                      DefaulOutputCollection.AmbinoBasic(3, 11,7, "Màn 2",true,"24inch"),
                                                      DefaulOutputCollection.AmbinoBasic(4, 11,7, "Màn 3",true,"24inch"),
                                                      DefaulOutputCollection.GenericLEDStrip(5, 16,"Cạnh Bàn", 1,true,"ledstrip")

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
            UnionOutput = DefaulOutputCollection.GenericLEDStrip(4, 16, "Uni-Strip", 4, false, "ledstrip"),
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.GenericLEDStrip(0, 16,"Dải LED 1", 4,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(1, 16,"Dải LED 2", 4,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(2, 16,"Dải LED 3", 4,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(3, 16,"Dải LED 4", 4,true,"ledstrip")



            }

        };
        public static DeviceSettings ambinoRainPow = new DeviceSettings {
            DeviceName = "Ambino RainPow",
            DeviceSerial = "ABRP",
            DeviceType = "ABRP",
            Manufacturer = "Ambino Vietnam",
            FirmwareVersion = "1.0.0",
            ProductionDate = "2022",
            IsVisible = true,
            IsEnabled = true,
            OutputPort = "Không có",
            Geometry = "ambinohubv3",
            IsTransferActive = true,
            UnionOutput = DefaulOutputCollection.GenericLEDStrip(6, 20, "Uni-Strip",1, false, "ledstrip"),
            AvailableOutputs = new OutputSettings[] { DefaulOutputCollection.GenericLEDStrip(0, 20,"Dây LED 1",1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(1, 20,"Dây LED 2",1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(2, 20,"Dây LED 3",1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(3, 20,"Dây LED 4",1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(4, 20,"Dây LED 5",1,true,"ledstrip"),
                                                      DefaulOutputCollection.GenericLEDStrip(5, 20,"Dây LED 6",1,true,"ledstrip")



            }

        };
    }
}
