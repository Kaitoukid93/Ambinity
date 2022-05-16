using adrilight.Settings;
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
        System.Object SerialIncoming = new object();
        public SerialDeviceDetection()
        {

        }

        private List<IDeviceSettings> _detectedSerialDevices;
        public List<string> ScanSupportedDevices()
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

        public List<IDeviceSettings> DetectedSerialDevices()
        {








            var availableSerialDevices = ScanSupportedDevices();
            _detectedSerialDevices = new List<IDeviceSettings>();
            byte[] requestMessage = { (byte)'d', (byte)'i', (byte)'d' };
            foreach (var device in availableSerialDevices)
            {




                try
                {
                    var _serialPort = new SerialPort();
                    _serialPort.PortName = device;//Set your board COM
                    _serialPort.BaudRate = 1000000;
                    _serialPort.DtrEnable = true;
                    _serialPort.ReadTimeout = 5000;
                    _serialPort.WriteTimeout = 500;
                    _serialPort.DataReceived += OnSerialDataReceived;
                    _serialPort.Open();
                    lock (SerialIncoming)//waits N seconds for a condition variable
                    {
                        int retryCount = 0;
                        while (!Monitor.Wait(SerialIncoming, 2000))
                        {//if timeout
                            if (retryCount == 10)
                            {

                                break;
                            }

                            _serialPort.Write(requestMessage, 0, 3); //wait for respond
                            retryCount++;

                        }

                    }

                }

                catch (Exception)
                {
                    Debug.WriteLine("Serial Port " + device + " access denied, added to Blacklist");
                    lock (SerialIncoming)
                    {
                        Monitor.Pulse(SerialIncoming);
                    }
                }


                }
        

            return _detectedSerialDevices;
                //Stop scanning for serial devices because no more device is available


        }
    void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        lock (SerialIncoming)
        {
            Monitor.Pulse(SerialIncoming);
        }
        SerialPort serialPort = (SerialPort)sender;
        string message = serialPort.ReadLine();
        string[] info = message.Split('|');

        //create new DeviceSettings based on data received
        if (info[0] == "abn")//valid device
        {
            IDeviceSettings newDevice = new DeviceSettings();

            switch (info[3])
            {
                case "ABBASIC":// General Ambino Basic USB Device

                    newDevice = DefaultDeviceCollection.ambinoBasic24;
                    newDevice.DeviceType = "ABBASIC";
                    newDevice.DeviceSerial = info[1];
                    newDevice.DeviceName = info[2];
                    newDevice.FirmwareVersion = info[4];
                    //newDevice.AvailableOutputs[0].OutputPowerVoltage = Int32.Parse(info[6]);
                    //newDevice.AvailableOutputs[0].OutputPowerMiliamps = Int32.Parse(info[7]);
                    newDevice.OutputPort = serialPort.PortName;

                    _detectedSerialDevices.Add(newDevice);



                    break;
                case "ABFANHUB":
                    newDevice = DefaultDeviceCollection.ambinoFanHub;
                    newDevice.DeviceType = "ABFANHUB";
                    newDevice.DeviceSerial = info[1];
                    newDevice.DeviceName = info[2];
                    newDevice.FirmwareVersion = info[4];
                    //foreach(var output in newDevice.AvailableOutputs)
                    //{
                    //    output.OutputPowerVoltage = Int32.Parse(info[6]);
                    //    output.OutputPowerMiliamps = Int32.Parse(info[7]) / (newDevice.AvailableOutputs.Length);// split power for all outputs since the wiring is parallel
                    //}
                    newDevice.OutputPort = serialPort.PortName;
                    _detectedSerialDevices.Add(newDevice);
                    break;
            }



        }


        serialPort.Close();
        serialPort.Dispose();
    }
    List<string> ComPortNames(String VID, String PID)
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
