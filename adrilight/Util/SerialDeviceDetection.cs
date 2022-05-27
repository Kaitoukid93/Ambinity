﻿using adrilight.Settings;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace adrilight.Util
{
    internal class SerialDeviceDetection : ISerialDeviceDetection
    {
        
        
        private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r' };
        private static byte[] expectedValidHeader = { 15, 12, 93 };
        private static CancellationToken cancellationtoken;

        public SerialDeviceDetection()
        {

        }

        
        public static List<string> ValidDevice()
        {
            List<string> names = ComPortNames("1209", "c550");
            List<string> devices = new List<string>();
            if (names.Count > 0)
            {
                int counter = 0;
                foreach (String s in SerialPort.GetPortNames())
                {
                    if (names.Contains(s))
                    {
                        counter++;
                        devices.Add(s);

                    }


                }
            }
            else
            {
                Console.WriteLine("Không tìm thấy thiết bị nào của Ambino, hãy thêm thiết bị theo cách thủ công");
                // return null;
            }
            return devices;
        }

        

        //static async Task SearchingForDevice(CancellationToken cancellationToken)
        //{
        //    var jobTask = Task.Run(() => {
        //        // Organize critical sections around logical serial port operations somehow.
        //        lock (_syncRoot)
        //        {
        //            return RequestDeviceInformation();
        //        }
        //    });
        //    if (jobTask != await Task.WhenAny(jobTask, Task.Delay(Timeout.Infinite, cancellationToken)))
        //    {
        //        // Timeout;
        //        return;
        //    }
        //    var newDevices = await jobTask;
        //    foreach(var device in newDevices)
        //    {
        //        Console.WriteLine("Name: " + device.DeviceName);
        //        Console.WriteLine("ID: " + device.DeviceSerial);
        //        Console.WriteLine("Firmware Version: " + device.FirmwareVersion);
        //        Console.WriteLine();
        //        Console.WriteLine();
        //        Console.WriteLine();
        //    }
            


        //    // Process response.
        //}
        public List<IDeviceSettings> DetectedDevices {
            get
            {
                return RequestDeviceInformation();
            }
        }
        static List<IDeviceSettings> RequestDeviceInformation()
        {
            // Assume serial port timeouts are set.
            byte[] id = new byte[256];
            byte[] name = new byte[256];
            byte[] fw = new byte[256];
            List<IDeviceSettings> newDevices = new List<IDeviceSettings>();
           
           
            foreach(var device in ValidDevice())
            {
                bool isValid = true;
               var _serialPort = new SerialPort(device,1000000);
                _serialPort.DtrEnable = true;
                _serialPort.ReadTimeout = 5000;
                _serialPort.WriteTimeout = 1000;
                try
                {
                    _serialPort.Open();
                }
                catch(UnauthorizedAccessException)
                {
                    continue;
                }
                
                //write request info command
                _serialPort.Write(requestCommand, 0, 3);
                int retryCount = 0;
                int offset = 0;
                int idLength = 0; // Expected response length of valid deviceID 
                int nameLength = 0; // Expected response length of valid deviceName 
                int fwLength = 0;
                IDeviceSettings newDevice = new DeviceSettings();
                while (offset < 3)
                {


                    try
                    {
                        byte header = (byte)_serialPort.ReadByte();
                        if (header == expectedValidHeader[offset])
                        {
                            offset++;
                        }
                    }
                    catch (TimeoutException)// retry until received valid header
                    {
                        _serialPort.Write(requestCommand, 0, 3);
                        retryCount++;
                        if (retryCount == 3)
                        {
                            Console.WriteLine("timeout waiting for respond on serialport " + _serialPort.PortName);
                            HandyControl.Controls.MessageBox.Show("Device at "+ _serialPort.PortName+ "is not responding, try adding it manually","Device is not responding", MessageBoxButton.OK, MessageBoxImage.Warning);
                            isValid = false;
                            break;
                        }
                        Debug.WriteLine("no respond, retrying...");
                    }


                }
                if (offset == 3) //3 bytes header are valid
                {
                    idLength = (byte)_serialPort.ReadByte();
                    int count = idLength;
                    id = new byte[count];
                    while (count > 0)
                    {
                        var readCount = _serialPort.Read(id, 0, count);
                        offset += readCount;
                        count -= readCount;
                    }


                    newDevice.DeviceSerial = BitConverter.ToString(id).Replace('-', ' ');
                }
                if (offset == 3 + idLength) //3 bytes header are valid
                {
                    nameLength = (byte)_serialPort.ReadByte();
                    int count = nameLength;
                    name = new byte[count];
                    while (count > 0)
                    {
                        var readCount = _serialPort.Read(name, 0, count);
                        offset += readCount;
                        count -= readCount;
                    }
                    newDevice.DeviceName = Encoding.ASCII.GetString(name, 0, name.Length);
                    switch (newDevice.DeviceName)
                    {
                        case "Ambino Basic":// General Ambino Basic USB Device

                            newDevice = DefaultDeviceCollection.ambinoBasic24;
                            newDevice.DeviceType = "ABBASIC";
                            newDevice.OutputPort = device;
                            break;
                        case "Ambino FanHub":
                            newDevice = DefaultDeviceCollection.ambinoFanHub;
                            newDevice.DeviceType = "ABFANHUB";
                            newDevice.OutputPort = device;
                            break;
                    }

                }
                if (offset == 3 + idLength + nameLength) //3 bytes header are valid
                {
                    fwLength = (byte)_serialPort.ReadByte();
                    int count = fwLength;
                    fw = new byte[count];
                    while (count > 0)
                    {
                        var readCount = _serialPort.Read(fw, 0, count);
                        offset += readCount;
                        count -= readCount;
                    }
                    newDevice.FirmwareVersion = Encoding.ASCII.GetString(fw, 0, fw.Length);
                }
                _serialPort.Close();
                _serialPort.Dispose();
                if(isValid)
                newDevices.Add(newDevice);

            }
           
            return newDevices;

        }
        static List<string> ComPortNames(String VID, String PID)
    {
        String pattern = String.Format("^VID_{0}.PID_{1}", VID, PID);
        Regex _rx = new Regex(pattern, RegexOptions.IgnoreCase);
        List<string> comports = new List<string>();
        RegistryKey rk1 = Registry.LocalMachine;
        RegistryKey rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");
        foreach (String s3 in rk2.GetSubKeyNames())
        {
            RegistryKey rk3 = rk2.OpenSubKey(s3);
            foreach (String s in rk3.GetSubKeyNames())
            {
                if (_rx.Match(s).Success)
                {
                    RegistryKey rk4 = rk3.OpenSubKey(s);
                    foreach (String s2 in rk4.GetSubKeyNames())
                    {
                        RegistryKey rk5 = rk4.OpenSubKey(s2);
                        RegistryKey rk6 = rk5.OpenSubKey("Device Parameters");
                        comports.Add((string)rk6.GetValue("PortName"));
                    }
                }
            }
        }
        return comports;
    }

}

}
